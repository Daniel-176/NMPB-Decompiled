using System;
using System.Runtime.CompilerServices;

namespace NMPB
{
	public class TextMessageEventArgs : EventArgs
	{
		public string Message
		{
			get;
			private set;
		}

		public TextMessageEventArgs(string message)
		{
			this.Message = message;
		}
	}
}