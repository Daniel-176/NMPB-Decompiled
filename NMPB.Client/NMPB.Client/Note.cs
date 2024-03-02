using Newtonsoft.Json;
using System;

namespace NMPB.Client
{
	public class Note
	{
		[JsonProperty("n")]
		public string Value;

		[JsonProperty("v", NullValueHandling=NullValueHandling.Ignore)]
		public double Velocity;

		[JsonProperty("d", NullValueHandling=NullValueHandling.Ignore)]
		public long Delay;

		[JsonProperty("s", NullValueHandling=NullValueHandling.Ignore)]
		public int Stop;

		public Note()
		{
			this.Value = "a0";
			this.Velocity = 0.5;
			this.Delay = (long)0;
			this.Stop = 0;
		}
	}
}