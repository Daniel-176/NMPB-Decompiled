using System;
using System.Runtime.CompilerServices;

namespace NMPB
{
	public class PlayerIdEventArgs : EventArgs
	{
		public string Id
		{
			get;
			private set;
		}

		public PlayerIdEventArgs(string id)
		{
			this.Id = id;
		}
	}
}