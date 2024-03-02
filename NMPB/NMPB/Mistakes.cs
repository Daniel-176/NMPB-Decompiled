using System;

namespace NMPB
{
	internal struct Mistakes
	{
		public bool Enabled;

		public int Delimiter;

		public int MaxDelay;

		public int MinOffset;

		public int MaxOffset;

		public Mistakes(bool enabled = false, int delimiter = 20, int maxDelay = 30)
		{
			this.Enabled = enabled;
			this.Delimiter = delimiter;
			this.MaxDelay = maxDelay;
			this.MinOffset = -2;
			this.MaxOffset = 2;
		}
	}
}