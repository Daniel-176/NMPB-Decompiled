using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NMPB.Timers
{
	internal sealed class ThreadTimer : ITimer
	{
		private readonly ThreadTimerQueue _queue;

		private TimerMode _mode;

		private TimeSpan _period;

		private TimeSpan _resolution;

		private readonly static object[] EmptyArgs;

		private readonly ThreadTimer.EventRaiser _tickRaiser;

		private ISynchronizeInvoke _synchronizingObject;

		private bool _disposed;

		public bool IsRunning
		{
			get;
			private set;
		}

		public TimerMode Mode
		{
			get
			{
				if (this._disposed)
				{
					throw new ObjectDisposedException("Timer");
				}
				return this._mode;
			}
			set
			{
				if (this._disposed)
				{
					throw new ObjectDisposedException("Timer");
				}
				this._mode = value;
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
				if (this._disposed)
				{
					throw new ObjectDisposedException("Timer");
				}
				return (long)this._period.get_TotalMilliseconds();
			}
			set
			{
				if (this._disposed)
				{
					throw new ObjectDisposedException("Timer");
				}
				bool isRunning = this.IsRunning;
				if (isRunning)
				{
					this.Stop();
				}
				this._period = TimeSpan.FromMilliseconds((double)value);
				if (isRunning)
				{
					this.Start();
				}
			}
		}

		public TimeSpan PeriodTimeSpan
		{
			get
			{
				return this._period;
			}
		}

		public int Resolution
		{
			get
			{
				return (int)this._resolution.get_TotalMilliseconds();
			}
			set
			{
				this._resolution = TimeSpan.FromMilliseconds((double)value);
			}
		}

		public ISite Site
		{
			get;
			set;
		}

		public ISynchronizeInvoke SynchronizingObject
		{
			get
			{
				if (this._disposed)
				{
					throw new ObjectDisposedException("Timer");
				}
				return this._synchronizingObject;
			}
			set
			{
				if (this._disposed)
				{
					throw new ObjectDisposedException("Timer");
				}
				this._synchronizingObject = value;
			}
		}

		static ThreadTimer()
		{
			ThreadTimer.EmptyArgs = new object[] { EventArgs.Empty };
		}

		public ThreadTimer() : this(ThreadTimerQueue.Instance)
		{
			if (!Stopwatch.IsHighResolution)
			{
				throw new NotImplementedException("Stopwatch is not IsHighResolution");
			}
			this.IsRunning = false;
			this._mode = TimerMode.Periodic;
			this._resolution = TimeSpan.FromMilliseconds(1);
			this._period = this._resolution;
			this._tickRaiser = new ThreadTimer.EventRaiser(this.OnTick);
		}

		private ThreadTimer(ThreadTimerQueue queue)
		{
			this._queue = queue;
		}

		public void Dispose()
		{
			this.Stop();
			this._disposed = true;
			this.OnDisposed(EventArgs.Empty);
		}

		internal void DoTick()
		{
			if (this.SynchronizingObject == null || !this.SynchronizingObject.get_InvokeRequired())
			{
				this.OnTick(EventArgs.Empty);
				return;
			}
			this.SynchronizingObject.BeginInvoke(this._tickRaiser, ThreadTimer.EmptyArgs);
		}

		private void OnDisposed(EventArgs e)
		{
			EventHandler eventHandler = this.Disposed;
			if (eventHandler == null)
			{
				return;
			}
			eventHandler.Invoke(this, e);
		}

		private void OnStarted(EventArgs e)
		{
			EventHandler eventHandler = this.Started;
			if (eventHandler == null)
			{
				return;
			}
			eventHandler.Invoke(this, e);
		}

		private void OnStopped(EventArgs e)
		{
			EventHandler eventHandler = this.Stopped;
			if (eventHandler == null)
			{
				return;
			}
			eventHandler.Invoke(this, e);
		}

		private void OnTick(EventArgs e)
		{
			EventHandler eventHandler = this.Tick;
			if (eventHandler == null)
			{
				return;
			}
			eventHandler.Invoke(this, e);
		}

		public void Start()
		{
			if (this._disposed)
			{
				throw new ObjectDisposedException("Timer");
			}
			if (this.IsRunning)
			{
				return;
			}
			if (this.Mode != TimerMode.Periodic)
			{
				throw new NotImplementedException();
			}
			this._queue.Add(this);
			this.IsRunning = true;
			if (this.SynchronizingObject == null || !this.SynchronizingObject.get_InvokeRequired())
			{
				this.OnStarted(EventArgs.Empty);
				return;
			}
			ISynchronizeInvoke synchronizingObject = this.SynchronizingObject;
			ThreadTimer.EventRaiser eventRaiser = new ThreadTimer.EventRaiser(this.OnStarted);
			object[] empty = new object[] { EventArgs.Empty };
			synchronizingObject.BeginInvoke((Delegate)eventRaiser, empty);
		}

		public void Stop()
		{
			if (this._disposed)
			{
				throw new ObjectDisposedException("Timer");
			}
			if (!this.IsRunning)
			{
				return;
			}
			this._queue.Remove(this);
			this.IsRunning = false;
			if (this.SynchronizingObject == null || !this.SynchronizingObject.get_InvokeRequired())
			{
				this.OnStopped(EventArgs.Empty);
				return;
			}
			ISynchronizeInvoke synchronizingObject = this.SynchronizingObject;
			ThreadTimer.EventRaiser eventRaiser = new ThreadTimer.EventRaiser(this.OnStopped);
			object[] empty = new object[] { EventArgs.Empty };
			synchronizingObject.BeginInvoke((Delegate)eventRaiser, empty);
		}

		public event EventHandler Disposed;

		public event EventHandler Started;

		public event EventHandler Stopped;

		public event EventHandler Tick;

		private delegate void EventRaiser(EventArgs e);
	}
}