using Newtonsoft.Json;
using System;

namespace NMPB.Client
{
	public class ChannelSettings
	{
		[JsonProperty("lobby")]
		public bool Lobby;

		[JsonProperty("visible")]
		public bool Visible;

		[JsonProperty("chat")]
		public bool Chat;

		[JsonProperty("crownsolo")]
		public bool Crownsolo;

		[JsonProperty("color")]
		public string Color;

		public ChannelSettings(bool lobby, bool visible, bool chat, bool crownsolo, string color = null)
		{
			this.Lobby = lobby;
			this.Visible = visible;
			this.Chat = chat;
			this.Crownsolo = crownsolo;
			this.Color = color;
		}

		public bool Equals(ChannelSettings b)
		{
			if (this.Lobby != b.Lobby || this.Visible != b.Visible || this.Chat != b.Chat || this.Crownsolo != b.Crownsolo)
			{
				return false;
			}
			if (this.Color == null || b.Color == null)
			{
				return true;
			}
			return this.Color.Equals(b.Color, StringComparison.OrdinalIgnoreCase);
		}
	}
}