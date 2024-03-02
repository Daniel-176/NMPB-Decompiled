using System;
using System.Threading;

namespace NMPB
{
	public class DelayedThread
	{
		private bool _cancelled;

		private readonly int _delay;

		private readonly Action _action;

		public DelayedThread(Action action, int delay)
		{
			this._cancelled = false;
			this._delay = delay;
			this._action = action;
			(new Thread(new ThreadStart(this.WaitAndDoWork))
			{
				IsBackground = true
			}).Start();
		}

		public void Cancel()
		{
			this._cancelled = true;
		}

		private void WaitAndDoWork()
		{
			Thread.Sleep(this._delay);
			if (this._cancelled)
			{
				return;
			}
			this._action();
		}
	}
}