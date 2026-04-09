@echo off
setlocal enabledelayedexpansion

echo ========================================
echo NMPB Build Script
echo ========================================
echo.
echo Requirements:
echo - Visual Studio 2022 with ".NET desktop development" workload installed
echo   (Open VS Installer ^> Modify VS2022 ^> Add workload)
echo.

set MSBUILD_DIR=C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin
set MSBUILD="%MSBUILD_DIR%\MSBuild.exe"
if not exist %MSBUILD% (
    echo ERROR: MSBuild.exe not found.
    echo Please install Visual Studio 2022 with .NET desktop development workload.
    goto error
)

if not exist "%MSBUILD_DIR%\Roslyn\Microsoft.CSharp.Core.targets" (
    echo ERROR: Roslyn compiler not found.
    echo Please install .NET desktop development workload in Visual Studio 2022.
    echo Open Visual Studio Installer ^> Modify ^> Add ".NET desktop development"
    goto error
)

set BUILD_DIR=%~dp0build
set NEEDS_DIR=%~dp0build-needs

if not exist "%BUILD_DIR%" mkdir "%BUILD_DIR%"
if not exist "%NEEDS_DIR%" (
    echo ERROR: build-needs folder not found.
    echo Please copy required DLLs to build-needs folder.
    goto error
)

echo Copying dependencies from build-needs...
xcopy /s /y "%NEEDS_DIR%\*" "%BUILD_DIR%\" >nul

echo Building NMPB Solution...
echo.

echo === Building NMPB (Core) ===
%MSBUILD% NMPB\NMPB.sln /p:Configuration=Release /p:Platform="x86" /v:minimal
if errorlevel 1 goto error
if exist "NMPB\bin\Release\*" xcopy /s /y "NMPB\bin\Release\*" "%BUILD_DIR%\" >nul

echo === Building NMPB.Client ===
%MSBUILD% NMPB.Client\NMPB.Client.sln /p:Configuration=Release /p:Platform="Any CPU" /v:minimal
if errorlevel 1 goto error
if exist "NMPB.Client\bin\Release\*" xcopy /s /y "NMPB.Client\bin\Release\*" "%BUILD_DIR%\" >nul

REM NMPB.RemoteControl skipped - missing decompiled source files
REM NMPB.Timers removed - unfixable decompilation issues

echo === Building NMPB-GUI ===
if not exist "NMPB-Gui\NMPB-GUIReferences" mkdir "NMPB-Gui\NMPB-GUIReferences"
if exist "NMPB\bin\Release\NMPB.dll" xcopy /y "NMPB\bin\Release\NMPB.dll" "NMPB-Gui\NMPB-GUIReferences\" >nul
%MSBUILD% NMPB-Gui\NMPB-GUI.sln /p:Configuration=Release /p:Platform="x86" /v:minimal
if errorlevel 1 goto error
if exist "NMPB-Gui\bin\Release\*" xcopy /s /y "NMPB-Gui\bin\Release\*" "%BUILD_DIR%\" >nul

echo === Building NMPB-FileExporter ===
if not exist "NMPB-FileExporter\NMPB-FileExporterReferences" mkdir "NMPB-FileExporter\NMPB-FileExporterReferences"
if exist "NMPB\bin\Release\NMPB.dll" xcopy /y "NMPB\bin\Release\NMPB.dll" "NMPB-FileExporter\NMPB-FileExporterReferences\" >nul
%MSBUILD% NMPB-FileExporter\NMPB-FileExporter.sln /p:Configuration=Release /p:Platform="x86" /v:minimal
if errorlevel 1 goto error
if exist "NMPB-FileExporter\bin\Release\*" xcopy /s /y "NMPB-FileExporter\bin\Release\*" "%BUILD_DIR%\" >nul

echo.
echo ========================================
echo Build completed successfully!
echo Output copied to: %BUILD_DIR%
echo ========================================
goto end

:error
echo.
echo ========================================
echo Build FAILED!
echo ========================================
exit /b 1

:end
pause