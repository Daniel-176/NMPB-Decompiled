using System;
using System.Diagnostics;

namespace NMPB.Timers
{
	public class MicroStopwatch : Stopwatch
	{
		private readonly double _microSecPerTick = 1000000 / (double)Stopwatch.Frequency;

		public long ElapsedMicroseconds
		{
			get
			{
				return (long)((double)base.get_ElapsedTicks() * this._microSecPerTick);
			}
		}

		public MicroStopwatch()
		{
			if (!Stopwatch.IsHighResolution)
			{
				throw new Exception("On this system the high-resolution performance counter is not available");
			}
		}
	}
}