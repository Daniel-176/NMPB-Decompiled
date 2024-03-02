using System;
using System.Runtime.CompilerServices;

namespace NMPB
{
	public class RenameEventArgs : EventArgs
	{
		public int Id
		{
			get;
			private set;
		}

		public string NewName
		{
			get;
			private set;
		}

		public string OldName
		{
			get;
			private set;
		}

		public RenameEventArgs(int id, string oldName, string newName)
		{
			this.Id = id;
			this.OldName = oldName;
			this.NewName = newName;
		}
	}
}