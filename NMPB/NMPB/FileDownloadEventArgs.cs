using System;
using System.Runtime.CompilerServices;

namespace NMPB
{
	public class FileDownloadEventArgs : EventArgs
	{
		public int Id
		{
			get;
			private set;
		}

		public string Name
		{
			get;
			private set;
		}

		public Uri URL
		{
			get;
			private set;
		}

		public FileDownloadEventArgs(Uri url, int id, string name)
		{
			this.URL = url;
			this.Id = id;
			this.Name = name;
		}
	}
}