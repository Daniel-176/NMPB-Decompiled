using System;
using System.Runtime.CompilerServices;

namespace NMPB.Client
{
	public class ConnectedEventArgs : UserBaseEventArgs
	{
		public string Motd
		{
			get;
			private set;
		}

		public string Version
		{
			get;
			private set;
		}

		public ConnectedEventArgs(string version, string motd, UserBase user) : base(user)
		{
			this.Version = version;
			this.Motd = motd;
		}
	}
}