using System;
using System.Runtime.CompilerServices;

namespace NMPB.Client
{
	public class UserBaseEventArgs : EventArgs
	{
		public UserBase User
		{
			get;
			private set;
		}

		public UserBaseEventArgs(UserBase user)
		{
			this.User = user;
		}
	}
}