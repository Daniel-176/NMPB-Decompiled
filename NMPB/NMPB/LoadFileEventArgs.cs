using System;
using System.Runtime.CompilerServices;

namespace NMPB
{
	public class LoadFileEventArgs : EventArgs
	{
		public string Filename
		{
			get;
			private set;
		}

		public int Id
		{
			get;
			private set;
		}

		public string Trackname
		{
			get;
			private set;
		}

		public LoadFileEventArgs(int id, string trackname, string filename)
		{
			this.Id = id;
			this.Trackname = trackname;
			this.Filename = filename;
		}
	}
}