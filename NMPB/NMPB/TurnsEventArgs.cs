using System;
using System.Runtime.CompilerServices;

namespace NMPB
{
	public class TurnsEventArgs : EventArgs
	{
		public User Master
		{
			get;
			private set;
		}

		public NMPB.TurnState TurnState
		{
			get;
			private set;
		}

		public TurnsEventArgs(NMPB.TurnState turnState, User master)
		{
			this.TurnState = turnState;
			this.Master = master;
		}
	}
}