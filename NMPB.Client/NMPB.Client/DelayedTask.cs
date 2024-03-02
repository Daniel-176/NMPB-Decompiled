using System;
using System.Threading;
using System.Threading.Tasks;

namespace NMPB.Client
{
	public class DelayedTask
	{
		private readonly int _delay;

		private readonly Action _action;

		private readonly CancellationTokenSource _token;

		public DelayedTask(Action action, int delay)
		{
			this._delay = delay;
			this._action = action;
			this._token = new CancellationTokenSource();
			(new Task(new Action(this.WaitAndDoWork), this._token.Token)).Start();
		}

		public void Cancel()
		{
			this._token.Cancel();
		}

		private void WaitAndDoWork()
		{
			Thread.Sleep(this._delay);
			if (this._token.IsCancellationRequested)
			{
				return;
			}
			this._action();
		}
	}
}