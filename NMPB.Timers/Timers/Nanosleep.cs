using System;
using System.Runtime.InteropServices;

namespace NMPB.Timers
{
	public static class Nanosleep
	{
		[DllImport("libc.so.6", CharSet=1, ExactSpelling=false)]
		private static extern void nanosleep(ref Nanosleep.Timespec rqtp, ref Nanosleep.Timespec rmtp);

		public static void NanoSleep(int s, long ns)
		{
			Nanosleep.Timespec timespec = new Nanosleep.Timespec()
			{
				TvSec = s,
				TvNsec = ns
			};
			Nanosleep.Timespec timespec1 = timespec;
			Nanosleep.Timespec timespec2 = new Nanosleep.Timespec();
			Nanosleep.nanosleep(ref timespec1, ref timespec2);
		}

		public static void Sleep1Us()
		{
			Nanosleep.Timespec timespec = new Nanosleep.Timespec()
			{
				TvSec = 0,
				TvNsec = (long)1000
			};
			Nanosleep.Timespec timespec1 = timespec;
			Nanosleep.Timespec timespec2 = new Nanosleep.Timespec();
			Nanosleep.nanosleep(ref timespec1, ref timespec2);
		}

		internal struct Timespec
		{
			public int TvSec;

			public long TvNsec;
		}
	}
}