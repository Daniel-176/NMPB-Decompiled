using NMPB.Client;
using System;
using System.Runtime.CompilerServices;

namespace NMPB
{
	public class UserChatMessageEventArgs : ChatMessageEventArgs
	{
		public bool PreventDefault;

		public NMPB.User User
		{
			get;
			private set;
		}

		public UserChatMessageEventArgs(NMPB.User user, string username, string message, string color, string auid) : base(username, message, color, auid)
		{
			this.User = user;
			this.PreventDefault = false;
		}
	}
}