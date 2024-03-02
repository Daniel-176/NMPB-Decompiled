using System;
using System.Runtime.CompilerServices;

namespace NMPB
{
	public class NoteReleasedEventArgs : EventArgs
	{
		public int Note
		{
			get;
			private set;
		}

		public NoteReleasedEventArgs(int note)
		{
			this.Note = note;
		}
	}
}