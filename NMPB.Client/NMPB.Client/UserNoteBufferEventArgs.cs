using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NMPB.Client
{
	public class UserNoteBufferEventArgs : UserBaseEventArgs
	{
		public List<Note> Notes
		{
			get;
			private set;
		}

		public long Time
		{
			get;
			private set;
		}

		public UserNoteBufferEventArgs(UserBase user, long time, List<Note> notes) : base(user)
		{
			this.Notes = notes;
			this.Time = time;
		}
	}
}