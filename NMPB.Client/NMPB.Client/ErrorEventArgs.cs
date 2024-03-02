using System;
using System.Runtime.CompilerServices;

namespace NMPB.Client
{
	public class ErrorEventArgs : EventArgs
	{
		public System.Exception Exception
		{
			get;
			private set;
		}

		public ErrorEventArgs(System.Exception exception)
		{
			this.Exception = exception;
		}
	}
}