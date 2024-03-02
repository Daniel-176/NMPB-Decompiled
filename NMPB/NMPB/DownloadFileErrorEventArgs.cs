using System;
using System.Runtime.CompilerServices;

namespace NMPB
{
	public class DownloadFileErrorEventArgs : EventArgs
	{
		public DownloadError Error
		{
			get;
			private set;
		}

		public System.Exception Exception
		{
			get;
			private set;
		}

		public string Message
		{
			get;
			private set;
		}

		public System.Uri Uri
		{
			get;
			private set;
		}

		public DownloadFileErrorEventArgs(System.Uri uri, DownloadError error, string message, System.Exception exception)
		{
			this.Uri = uri;
			this.Error = error;
			this.Message = message;
			this.Exception = exception;
		}
	}
}