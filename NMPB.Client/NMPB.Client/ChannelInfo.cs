using Newtonsoft.Json;
using System;

namespace NMPB.Client
{
	public class ChannelInfo
	{
		[JsonProperty("_id")]
		public string Id;

		[JsonProperty("settings")]
		public ChannelSettings Settings;

		[JsonProperty("crown")]
		public CrownInfo Crown;

		[JsonProperty("count", DefaultValueHandling=DefaultValueHandling.IgnoreAndPopulate)]
		public int UserCount;

		public ChannelInfo()
		{
		}
	}
}