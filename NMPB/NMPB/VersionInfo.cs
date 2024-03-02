using System;
using System.Reflection;

namespace NMPB
{
	public static class VersionInfo
	{
		public static System.Version Version;

		static VersionInfo()
		{
			VersionInfo.Version = Assembly.GetExecutingAssembly().GetName().Version;
		}

		public static new string ToString()
		{
			return string.Format("NMPB v{0}.{1}. Build: {2}.", VersionInfo.Version.Major, VersionInfo.Version.Minor, VersionInfo.Version.Build);
		}
	}
}