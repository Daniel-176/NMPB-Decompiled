using System;
using System.Runtime.CompilerServices;

namespace NMPB.Client
{
	public class ChatMessageEventArgs : EventArgs
	{
		public string Auid
		{
			get;
			private set;
		}

		public string Color
		{
			get;
			private set;
		}

		public string Message
		{
			get;
			private set;
		}

		public string Username
		{
			get;
			private set;
		}

		public ChatMessageEventArgs(string username, string message, string color, string auid)
		{
			this.Message = message;
			this.Username = username;
			this.Color = color;
			this.Auid = auid;
		}
	}
}