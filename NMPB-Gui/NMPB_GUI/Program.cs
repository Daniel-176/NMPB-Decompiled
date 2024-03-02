using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;

namespace NMPB_GUI
{
	internal static class Program
	{
		private static string _restartConfig;

		private static bool _requiresRestart;

		static Program()
		{
		}

		public static dynamic GetJSON(string file)
		{
			object obj;
			try
			{
				obj = JObject.Parse(File.ReadAllText(file));
			}
			catch (Exception exception)
			{
				obj = JObject.Parse(File.ReadAllText(file).Replace("\\", "\\\\"));
			}
			return obj;
		}

		private static void GlobalThreadExceptionHandler(object sender, ThreadExceptionEventArgs e)
		{
			Program.Log(e.Exception);
		}

		private static void GlobalUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
		{
			Program.Log((Exception)e.ExceptionObject);
		}

		private static void Log(Exception ex)
		{
			string str = string.Concat(ex.Message, Environment.NewLine);
			str = string.Concat(str, JsonConvert.SerializeObject(ex, 1).Replace("\\r\\n", Environment.NewLine));
			string str1 = string.Format("Crash-{0:yyyy-MM-dd_hh-mm-ss-tt}.txt", DateTime.Now);
			File.WriteAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), str1), str);
			if (Program._requiresRestart)
			{
				Program._requiresRestart = false;
				Process.Start(Assembly.GetEntryAssembly().Location, string.Format("\"{0}\"", Program._restartConfig));
			}
			Environment.Exit(1);
		}

		[STAThread]
		private static void Main(string[] args)
		{
			if (!AppDomain.CurrentDomain.FriendlyName.EndsWith("vshost.exe"))
			{
				AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Program.GlobalUnhandledExceptionHandler);
				Application.ThreadException += new ThreadExceptionEventHandler(Program.GlobalThreadExceptionHandler);
			}
			Application.EnableVisualStyles();
			Application.CurrentCulture = new CultureInfo("en-US");
			Application.SetCompatibleTextRenderingDefault(false);
			string str = null;
			if (args.Length != 0)
			{
				str = args[0];
				dynamic jSON = Program.GetJSON(str);
				dynamic obj = jSON != (dynamic)null;
				dynamic obj1 = (!obj ? obj : obj & jSON.gui != (dynamic)null);
				if ((!obj1 ? obj1 : obj1 & jSON.gui.autoRestart == true))
				{
					Program._restartConfig = str;
					Program._requiresRestart = true;
				}
			}
			Application.Run(new Form1(str));
		}
	}
}