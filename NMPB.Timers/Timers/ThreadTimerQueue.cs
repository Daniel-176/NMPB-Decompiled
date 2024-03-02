using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace NMPB.Timers
{
	internal class ThreadTimerQueue
	{
		private Stopwatch watch = Stopwatch.StartNew();

		private Thread loop;

		private List<ThreadTimerQueue.Tick> tickQueue = new List<ThreadTimerQueue.Tick>();

		private static ThreadTimerQueue instance;

		public static ThreadTimerQueue Instance
		{
			get
			{
				if (ThreadTimerQueue.instance == null)
				{
					ThreadTimerQueue.instance = new ThreadTimerQueue();
				}
				return ThreadTimerQueue.instance;
			}
		}

		private ThreadTimerQueue()
		{
		}

		public void Add(ThreadTimer timer)
		{
			ThreadTimerQueue threadTimerQueue = this;
			Monitor.Enter(threadTimerQueue);
			try
			{
				ThreadTimerQueue.Tick tick = new ThreadTimerQueue.Tick()
				{
					Timer = timer,
					Time = this.watch.get_Elapsed()
				};
				this.tickQueue.Add(tick);
				this.tickQueue.Sort();
				if (this.loop == null)
				{
					this.loop = new Thread(new ThreadStart(this, ThreadTimerQueue.TimerLoop));
					this.loop.Start();
				}
				Monitor.PulseAll(this);
			}
			finally
			{
				Monitor.Exit(threadTimerQueue);
			}
		}

		private static TimeSpan Min(TimeSpan x0, TimeSpan x1)
		{
			if (x0 > x1)
			{
				return x1;
			}
			return x0;
		}

		public void Remove(ThreadTimer timer)
		{
			ThreadTimerQueue threadTimerQueue = this;
			Monitor.Enter(threadTimerQueue);
			try
			{
				int num = 0;
				while (num < this.tickQueue.get_Count() && (object)this.tickQueue.get_Item(num).Timer != (object)timer)
				{
					num++;
				}
				if (num < this.tickQueue.get_Count())
				{
					this.tickQueue.RemoveAt(num);
				}
				Monitor.PulseAll(this);
			}
			finally
			{
				Monitor.Exit(threadTimerQueue);
			}
		}

		private void TimerLoop()
		{
			ThreadTimerQueue threadTimerQueue = this;
			Monitor.Enter(threadTimerQueue);
			try
			{
				TimeSpan timeSpan = TimeSpan.FromMilliseconds(500);
				for (int i = 0; i < 3; i++)
				{
					TimeSpan timeSpan1 = timeSpan;
					if (this.tickQueue.get_Count() > 0)
					{
						timeSpan1 = ThreadTimerQueue.Min(this.tickQueue.get_Item(0).Time - this.watch.get_Elapsed(), timeSpan1);
						i = 0;
					}
					if (timeSpan1 > TimeSpan.Zero)
					{
						Monitor.Wait(this, timeSpan1);
					}
					if (this.tickQueue.get_Count() > 0)
					{
						ThreadTimerQueue.Tick item = this.tickQueue.get_Item(0);
						TimerMode mode = item.Timer.Mode;
						Monitor.Exit(this);
						item.Timer.DoTick();
						Monitor.Enter(this);
						if (mode != TimerMode.Periodic)
						{
							this.tickQueue.RemoveAt(0);
						}
						else
						{
							item.Time += item.Timer.PeriodTimeSpan;
							this.tickQueue.Sort();
						}
					}
				}
				this.loop = null;
			}
			finally
			{
				Monitor.Exit(threadTimerQueue);
			}
		}

		private class Tick : IComparable
		{
			public ThreadTimer Timer;

			public TimeSpan Time;

			public Tick()
			{
			}

			public int CompareTo(object obj)
			{
				ThreadTimerQueue.Tick tick = obj as ThreadTimerQueue.Tick;
				if (tick == null)
				{
					return -1;
				}
				return this.Time.CompareTo(tick.Time);
			}
		}
	}
}