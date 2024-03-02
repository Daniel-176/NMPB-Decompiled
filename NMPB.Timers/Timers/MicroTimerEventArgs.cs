using System;
using System.Runtime.CompilerServices;

namespace NMPB.Timers
{
	public class MicroTimerEventArgs : EventArgs
	{
		public long CallbackFunctionExecutionTime
		{
			get;
			private set;
		}

		public long ElapsedMicroseconds
		{
			get;
			private set;
		}

		public int TimerCount
		{
			get;
			private set;
		}

		public long TimerLateBy
		{
			get;
			private set;
		}

		public MicroTimerEventArgs(int timerCount, long elapsedMicroseconds, long timerLateBy, long callbackFunctionExecutionTime)
		{
			this.TimerCount = timerCount;
			this.ElapsedMicroseconds = elapsedMicroseconds;
			this.TimerLateBy = timerLateBy;
			this.CallbackFunctionExecutionTime = callbackFunctionExecutionTime;
		}
	}
}