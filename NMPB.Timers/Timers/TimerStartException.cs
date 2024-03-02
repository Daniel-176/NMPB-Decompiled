using System;

namespace NMPB.Timers
{
	public class TimerStartException : ApplicationException
	{
		public TimerStartException(string message) : base(message)
		{
		}
	}
}