using System;
using System.Runtime.CompilerServices;

namespace NMPB
{
	public class SayMessageEventArgs : EventArgs
	{
		public bool Local
		{
			get;
			private set;
		}

		public string Message
		{
			get;
			private set;
		}

		public SayMessageEventArgs(string message, bool locally)
		{
			this.Message = message;
			this.Local = locally;
		}
	}
}