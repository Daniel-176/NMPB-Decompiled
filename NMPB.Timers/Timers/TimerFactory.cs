using System;
using System.Runtime.CompilerServices;

namespace NMPB.Timers
{
	public static class TimerFactory
	{
		public static bool IsUnix;

		public static bool IsMac;

		public static bool IsManual;

		public static bool UseManagedOnWin;

		public static TimerCaps Capabilities
		{
			get;
			private set;
		}

		static TimerFactory()
		{
			TimerFactory.IsUnix = Environment.get_OSVersion().get_Platform() == 4;
			TimerFactory.IsMac = Environment.get_OSVersion().get_Platform() == 6;
			TimerFactory.IsManual = false;
			TimerFactory.UseManagedOnWin = false;
			TimerCaps timerCap = new TimerCaps()
			{
				periodMin = 1,
				periodMax = 2147483647
			};
			TimerFactory.Capabilities = timerCap;
		}

		public static ITimer GetTimer()
		{
			if (TimerFactory.IsManual)
			{
				return new ManualTimer();
			}
			if (TimerFactory.IsUnix)
			{
				return new NixTimer();
			}
			if (!TimerFactory.IsMac && !TimerFactory.UseManagedOnWin)
			{
				return new WinTimer();
			}
			return new ThreadTimer();
		}
	}
}