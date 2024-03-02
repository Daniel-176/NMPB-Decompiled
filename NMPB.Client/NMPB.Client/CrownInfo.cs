using Newtonsoft.Json;
using System;

namespace NMPB.Client
{
	public class CrownInfo
	{
		[JsonProperty("participantId")]
		public string ParticipantId;

		[JsonProperty("userId")]
		public string UserId;

		[JsonProperty("time", DefaultValueHandling=DefaultValueHandling.IgnoreAndPopulate)]
		public long Time;

		[JsonProperty("startPos")]
		public CrownPoint StartPos;

		[JsonProperty("endPos")]
		public CrownPoint EndPos;

		public CrownInfo(string participantId, string userId, long time, CrownPoint startPos, CrownPoint endPos)
		{
			this.ParticipantId = participantId;
			this.UserId = userId;
			this.Time = time;
			this.StartPos = startPos;
			this.EndPos = endPos;
		}
	}
}