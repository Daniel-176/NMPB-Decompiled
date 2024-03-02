using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace NMPB.Timers
{
	public sealed class WinTimer : IComponent, IDisposable, ITimer
	{
		private const int TIMERR_NOERROR = 0;

		private int timerID;

		private volatile TimerMode mode;

		private volatile int period;

		private volatile int resolution;

		private WinTimer.TimeProc timeProcPeriodic;

		private WinTimer.TimeProc timeProcOneShot;

		private WinTimer.EventRaiser tickRaiser;

		private bool running;

		private volatile bool disposed;

		private ISynchronizeInvoke synchronizingObject;

		private ISite site;

		public bool IsRunning
		{
			get
			{
				return this.running;
			}
		}

		public TimerMode Mode
		{
			get
			{
				if (this.disposed)
				{
					throw new ObjectDisposedException("Timer");
				}
				return (TimerMode)this.mode;
			}
			set
			{
				if (this.disposed)
				{
					throw new ObjectDisposedException("Timer");
				}
				this.mode = value;
				if (this.IsRunning)
				{
					this.Stop();
					this.Start();
				}
			}
		}

		public long Period
		{
			get
			{
				if (this.disposed)
				{
					throw new ObjectDisposedException("Timer");
				}
				return (long)this.period;
			}
			set
			{
				if (this.disposed)
				{
					throw new ObjectDisposedException("Timer");
				}
				if (value < (long)TimerFactory.Capabilities.periodMin || value > (long)TimerFactory.Capabilities.periodMax)
				{
					throw new ArgumentOutOfRangeException("Period", (object)value, "Multimedia Timer period out of range.");
				}
				this.period = (int)value;
				if (this.IsRunning)
				{
					this.Stop();
					this.Start();
				}
			}
		}

		public int Resolution
		{
			get
			{
				if (this.disposed)
				{
					throw new ObjectDisposedException("Timer");
				}
				return this.resolution;
			}
			set
			{
				if (this.disposed)
				{
					throw new ObjectDisposedException("Timer");
				}
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException("Resolution", (object)value, "Multimedia timer resolution out of range.");
				}
				this.resolution = value;
				if (this.IsRunning)
				{
					this.Stop();
					this.Start();
				}
			}
		}

		public ISite Site
		{
			get
			{
				return this.site;
			}
			set
			{
				this.site = value;
			}
		}

		public ISynchronizeInvoke SynchronizingObject
		{
			get
			{
				if (this.disposed)
				{
					throw new ObjectDisposedException("Timer");
				}
				return this.synchronizingObject;
			}
			set
			{
				if (this.disposed)
				{
					throw new ObjectDisposedException("Timer");
				}
				this.synchronizingObject = value;
			}
		}

		public WinTimer(IContainer container)
		{
			container.Add(this);
			this.Initialize();
		}

		public WinTimer()
		{
			this.Initialize();
		}

		public void Dispose()
		{
			if (this.disposed)
			{
				return;
			}
			if (this.IsRunning)
			{
				this.Stop();
			}
			this.disposed = true;
			this.OnDisposed(EventArgs.Empty);
		}

		protected override void Finalize()
		{
			try
			{
				if (this.IsRunning)
				{
					WinTimer.timeKillEvent(this.timerID);
				}
			}
			finally
			{
				this.Finalize();
			}
		}

		public static TimerCaps GetTimerCaps()
		{
			TimerCaps timerCap = new TimerCaps();
			WinTimer.timeGetDevCaps(ref timerCap, Marshal.SizeOf(timerCap));
			return timerCap;
		}

		private void Initialize()
		{
			this.mode = TimerMode.Periodic;
			this.period = TimerFactory.Capabilities.periodMin;
			this.resolution = 1;
			this.running = false;
			this.timeProcPeriodic = new WinTimer.TimeProc(this.TimerPeriodicEventCallback);
			this.timeProcOneShot = new WinTimer.TimeProc(this.TimerOneShotEventCallback);
			this.tickRaiser = new WinTimer.EventRaiser(this.OnTick);
		}

		private void OnDisposed(EventArgs e)
		{
			EventHandler eventHandler = this.Disposed;
			if (eventHandler != null)
			{
				eventHandler.Invoke(this, e);
			}
		}

		private void OnStarted(EventArgs e)
		{
			EventHandler eventHandler = this.Started;
			if (eventHandler != null)
			{
				eventHandler.Invoke(this, e);
			}
		}

		private void OnStopped(EventArgs e)
		{
			EventHandler eventHandler = this.Stopped;
			if (eventHandler != null)
			{
				eventHandler.Invoke(this, e);
			}
		}

		private void OnTick(EventArgs e)
		{
			EventHandler eventHandler = this.Tick;
			if (eventHandler != null)
			{
				eventHandler.Invoke(this, e);
			}
		}

		public void Start()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException("Timer");
			}
			if (this.IsRunning)
			{
				return;
			}
			if (this.Mode != TimerMode.Periodic)
			{
				this.timerID = WinTimer.timeSetEvent((int)this.Period, this.Resolution, this.timeProcOneShot, 0, (int)this.Mode);
			}
			else
			{
				this.timerID = WinTimer.timeSetEvent((int)this.Period, this.Resolution, this.timeProcPeriodic, 0, (int)this.Mode);
			}
			if (this.timerID == 0)
			{
				throw new TimerStartException("Unable to start multimedia Timer.");
			}
			this.running = true;
			if (this.SynchronizingObject == null || !this.SynchronizingObject.get_InvokeRequired())
			{
				this.OnStarted(EventArgs.Empty);
				return;
			}
			ISynchronizeInvoke synchronizingObject = this.SynchronizingObject;
			WinTimer.EventRaiser eventRaiser = new WinTimer.EventRaiser(this.OnStarted);
			object[] empty = new object[] { EventArgs.Empty };
			synchronizingObject.BeginInvoke((Delegate)eventRaiser, empty);
		}

		public void Stop()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException("Timer");
			}
			if (!this.running)
			{
				return;
			}
			WinTimer.timeKillEvent(this.timerID);
			this.running = false;
			if (this.SynchronizingObject == null || !this.SynchronizingObject.get_InvokeRequired())
			{
				this.OnStopped(EventArgs.Empty);
				return;
			}
			ISynchronizeInvoke synchronizingObject = this.SynchronizingObject;
			WinTimer.EventRaiser eventRaiser = new WinTimer.EventRaiser(this.OnStopped);
			object[] empty = new object[] { EventArgs.Empty };
			synchronizingObject.BeginInvoke((Delegate)eventRaiser, empty);
		}

		[DllImport("winmm.dll", CharSet=1, ExactSpelling=false)]
		internal static extern int timeGetDevCaps(ref TimerCaps caps, int sizeOfTimerCaps);

		[DllImport("winmm.dll", CharSet=1, ExactSpelling=false)]
		private static extern int timeKillEvent(int id);

		private void TimerOneShotEventCallback(int id, int msg, int user, int param1, int param2)
		{
			if (this.synchronizingObject == null)
			{
				this.OnTick(EventArgs.Empty);
				this.Stop();
				return;
			}
			ISynchronizeInvoke synchronizeInvoke = this.synchronizingObject;
			WinTimer.EventRaiser eventRaiser = this.tickRaiser;
			object[] empty = new object[] { EventArgs.Empty };
			synchronizeInvoke.BeginInvoke((Delegate)eventRaiser, empty);
			this.Stop();
		}

		private void TimerPeriodicEventCallback(int id, int msg, int user, int param1, int param2)
		{
			if (this.synchronizingObject == null)
			{
				this.OnTick(EventArgs.Empty);
				return;
			}
			ISynchronizeInvoke synchronizeInvoke = this.synchronizingObject;
			WinTimer.EventRaiser eventRaiser = this.tickRaiser;
			object[] empty = new object[] { EventArgs.Empty };
			synchronizeInvoke.BeginInvoke((Delegate)eventRaiser, empty);
		}

		[DllImport("winmm.dll", CharSet=1, ExactSpelling=false)]
		private static extern int timeSetEvent(int delay, int resolution, WinTimer.TimeProc proc, int user, int mode);

		public event EventHandler Disposed;

		public event EventHandler Started;

		public event EventHandler Stopped;

		public event EventHandler Tick;

		private delegate void EventRaiser(EventArgs e);

		private delegate void TimeProc(int id, int msg, int user, int param1, int param2);
	}
}