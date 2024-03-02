using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NMPB.Timers
{
	public class ManualTimer : ITimer
	{
		public long Period
		{
			get;
			set;
		}

		static ManualTimer()
		{
			ManualTimer.TickRaised = new EventHandler(ManualTimer.<>c.<>9, (object argument0, EventArgs argument1) => {
			});
		}

		public ManualTimer()
		{
			EventHandler u003cu003e9_130 = ManualTimer.<>c.<>9__13_0;
			if (u003cu003e9_130 == null)
			{
				u003cu003e9_130 = new EventHandler(ManualTimer.<>c.<>9, (object argument0, EventArgs argument1) => {
				});
				ManualTimer.<>c.<>9__13_0 = u003cu003e9_130;
			}
			this.Started = u003cu003e9_130;
			EventHandler u003cu003e9_131 = ManualTimer.<>c.<>9__13_1;
			if (u003cu003e9_131 == null)
			{
				u003cu003e9_131 = new EventHandler(ManualTimer.<>c.<>9, (object argument2, EventArgs argument3) => {
				});
				ManualTimer.<>c.<>9__13_1 = u003cu003e9_131;
			}
			this.Stopped = u003cu003e9_131;
			EventHandler u003cu003e9_132 = ManualTimer.<>c.<>9__13_2;
			if (u003cu003e9_132 == null)
			{
				u003cu003e9_132 = new EventHandler(ManualTimer.<>c.<>9, (object argument4, EventArgs argument5) => {
				});
				ManualTimer.<>c.<>9__13_2 = u003cu003e9_132;
			}
			this.Tick = u003cu003e9_132;
			EventHandler u003cu003e9_133 = ManualTimer.<>c.<>9__13_3;
			if (u003cu003e9_133 == null)
			{
				u003cu003e9_133 = new EventHandler(ManualTimer.<>c.<>9, (object argument6, EventArgs argument7) => {
				});
				ManualTimer.<>c.<>9__13_3 = u003cu003e9_133;
			}
			this.Disposed = u003cu003e9_133;
			base();
			ManualTimer.TickRaised += new EventHandler(this, ManualTimer.OnTick);
		}

		public void Dispose()
		{
			ManualTimer.TickRaised -= new EventHandler(this, ManualTimer.OnTick);
			this.Disposed.Invoke(this, new EventArgs());
		}

		private void OnTick(object sender, EventArgs e)
		{
			this.Tick.Invoke(this, new EventArgs());
		}

		public static void RaiseTick()
		{
			ManualTimer.TickRaised.Invoke(null, new EventArgs());
		}

		public void Start()
		{
			this.Started.Invoke(this, new EventArgs());
		}

		public void Stop()
		{
			this.Stopped.Invoke(this, new EventArgs());
		}

		public event EventHandler Disposed;

		public event EventHandler Started;

		public event EventHandler Stopped;

		public event EventHandler Tick;

		public static event EventHandler TickRaised;
	}
}