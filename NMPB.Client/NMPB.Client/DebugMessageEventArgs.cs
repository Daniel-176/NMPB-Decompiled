using System;
using System.Runtime.CompilerServices;

namespace NMPB.Client
{
	public class DebugMessageEventArgs : EventArgs
	{
		public string Message
		{
			get;
			private set;
		}

		public DebugMessageEventArgs(string message)
		{
			this.Message = message;
		}
	}
}