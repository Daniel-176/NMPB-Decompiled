using System;
using System.Runtime.CompilerServices;

namespace NMPB
{
	public class UserEventArgs : EventArgs
	{
		public NMPB.User User
		{
			get;
			private set;
		}

		public UserEventArgs(NMPB.User user)
		{
			this.User = user;
		}
	}
}