using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NMPB.Timers
{
	public class NixTimer : ITimer
	{
		private Thread _threadTimer;

		private long _ignoreEventIfLateBy = 9223372036854775807L;

		private long _timerIntervalInMicroSec;

		private bool _stopTimer = true;

		public bool Enabled
		{
			get
			{
				if (this._threadTimer == null)
				{
					return false;
				}
				return this._threadTimer.get_IsAlive();
			}
			set
			{
				if (value)
				{
					this.Start();
					return;
				}
				this.Stop();
			}
		}

		public long IgnoreEventIfLateBy
		{
			get
			{
				return Interlocked.Read(ref this._ignoreEventIfLateBy);
			}
			set
			{
				Interlocked.Exchange(ref this._ignoreEventIfLateBy, (value <= (long)0 ? 9223372036854775807L : value));
			}
		}

		public long Period
		{
			get
			{
				return Interlocked.Read(ref this._timerIntervalInMicroSec) / (long)1000;
			}
			set
			{
				Interlocked.Exchange(ref this._timerIntervalInMicroSec, value * (long)1000);
			}
		}

		public NixTimer()
		{
			this._ignoreEventIfLateBy = 9223372036854775807L;
			this._stopTimer = true;
			EventHandler u003cu003e9_80 = NixTimer.<>c.<>9__8_0;
			if (u003cu003e9_80 == null)
			{
				u003cu003e9_80 = new EventHandler(NixTimer.<>c.<>9, (object argument0, EventArgs argument1) => {
				});
				NixTimer.<>c.<>9__8_0 = u003cu003e9_80;
			}
			this.Started = u003cu003e9_80;
			EventHandler u003cu003e9_81 = NixTimer.<>c.<>9__8_1;
			if (u003cu003e9_81 == null)
			{
				u003cu003e9_81 = new EventHandler(NixTimer.<>c.<>9, (object argument2, EventArgs argument3) => {
				});
				NixTimer.<>c.<>9__8_1 = u003cu003e9_81;
			}
			this.Stopped = u003cu003e9_81;
			EventHandler u003cu003e9_82 = NixTimer.<>c.<>9__8_2;
			if (u003cu003e9_82 == null)
			{
				u003cu003e9_82 = new EventHandler(NixTimer.<>c.<>9, (object argument4, EventArgs argument5) => {
				});
				NixTimer.<>c.<>9__8_2 = u003cu003e9_82;
			}
			this.Disposed = u003cu003e9_82;
			base();
		}

		public NixTimer(long timerIntervalInMicroseconds)
		{
			this._ignoreEventIfLateBy = 9223372036854775807L;
			this._stopTimer = true;
			EventHandler u003cu003e9_90 = NixTimer.<>c.<>9__9_0;
			if (u003cu003e9_90 == null)
			{
				u003cu003e9_90 = new EventHandler(NixTimer.<>c.<>9, (object argument0, EventArgs argument1) => {
				});
				NixTimer.<>c.<>9__9_0 = u003cu003e9_90;
			}
			this.Started = u003cu003e9_90;
			EventHandler u003cu003e9_91 = NixTimer.<>c.<>9__9_1;
			if (u003cu003e9_91 == null)
			{
				u003cu003e9_91 = new EventHandler(NixTimer.<>c.<>9, (object argument2, EventArgs argument3) => {
				});
				NixTimer.<>c.<>9__9_1 = u003cu003e9_91;
			}
			this.Stopped = u003cu003e9_91;
			EventHandler u003cu003e9_92 = NixTimer.<>c.<>9__9_2;
			if (u003cu003e9_92 == null)
			{
				u003cu003e9_92 = new EventHandler(NixTimer.<>c.<>9, (object argument4, EventArgs argument5) => {
				});
				NixTimer.<>c.<>9__9_2 = u003cu003e9_92;
			}
			this.Disposed = u003cu003e9_92;
			base();
			this.Period = timerIntervalInMicroseconds;
		}

		public void Abort()
		{
			this._stopTimer = true;
			if (this.Enabled)
			{
				this._threadTimer.Abort();
			}
		}

		public void Dispose()
		{
			this.Stop();
			this.Disposed.Invoke(this, EventArgs.Empty);
		}

		private void NotificationTimer(ref long timerIntervalInMicroSec, ref long ignoreEventIfLateBy, ref bool stopTimer)
		{
			int num = 0;
			long num1 = (long)0;
			MicroStopwatch microStopwatch = new MicroStopwatch();
			microStopwatch.Start();
			while (!stopTimer)
			{
				long elapsedMicroseconds = microStopwatch.ElapsedMicroseconds - num1;
				long num2 = Interlocked.Read(ref timerIntervalInMicroSec);
				long num3 = Interlocked.Read(ref ignoreEventIfLateBy);
				num1 += num2;
				num++;
				long num4 = (long)0;
				while (true)
				{
					long elapsedMicroseconds1 = microStopwatch.ElapsedMicroseconds;
					num4 = elapsedMicroseconds1;
					if (elapsedMicroseconds1 >= num1)
					{
						break;
					}
					Nanosleep.Sleep1Us();
				}
				long num5 = num4 - num1;
				if (num5 >= num3)
				{
					continue;
				}
				MicroTimerEventArgs microTimerEventArg = new MicroTimerEventArgs(num, num4, num5, elapsedMicroseconds);
				EventHandler eventHandler = this.Tick;
				if (eventHandler != null)
				{
					eventHandler.Invoke(this, microTimerEventArg);
				}
				else
				{
				}
			}
			microStopwatch.Stop();
			this.Stopped.Invoke(this, EventArgs.Empty);
		}

		public void Start()
		{
			if (this.Enabled || this.Period <= (long)0)
			{
				return;
			}
			this._stopTimer = false;
			this._threadTimer = new Thread(new ThreadStart(this, () => this.NotificationTimer(ref this._timerIntervalInMicroSec, ref this._ignoreEventIfLateBy, ref this._stopTimer)));
			this._threadTimer.set_Priority(4);
			this._threadTimer.Start();
			this.Started.Invoke(this, EventArgs.Empty);
		}

		public void Stop()
		{
			this._stopTimer = true;
		}

		public void StopAndWait()
		{
			this.StopAndWait(-1);
		}

		public bool StopAndWait(int timeoutInMilliSec)
		{
			this._stopTimer = true;
			if (!this.Enabled || this._threadTimer.get_ManagedThreadId() == Thread.get_CurrentThread().get_ManagedThreadId())
			{
				return true;
			}
			return this._threadTimer.Join(timeoutInMilliSec);
		}

		public event EventHandler Disposed;

		public event EventHandler Started;

		public event EventHandler Stopped;

		public event EventHandler Tick;

		public delegate void MicroTimerElapsedEventHandler(object sender, MicroTimerEventArgs timerEventArgs);
	}
}