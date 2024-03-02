using System;
using System.Runtime.CompilerServices;

namespace NMPB
{
	public class TrackPositionEventArgs : EventArgs
	{
		public int Absolute
		{
			get;
			private set;
		}

		public double Percent
		{
			get;
			private set;
		}

		public int TrackLength
		{
			get;
			private set;
		}

		public TrackPositionEventArgs(double percent, int absolute, int trackLength)
		{
			this.Percent = percent;
			this.Absolute = absolute;
			this.TrackLength = trackLength;
		}
	}
}