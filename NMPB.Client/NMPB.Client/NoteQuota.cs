using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;

namespace NMPB.Client
{
	public class NoteQuota : IDisposable
	{
		public int Allowance = 400;

		public int Max = 1200;

		public int HistLen = 3;

		private dynamic _paramsNormal = new { allowance = 400, max = 1200 };

		private readonly Timer _timer;

		private readonly LinkedList<int> _history;

		public int Points
		{
			get;
			private set;
		}

		public NoteQuota(bool timer = true)
		{
			this.Points = this.Max;
			this._history = new LinkedList<int>(new int[] { this.Max });
			if (!timer)
			{
				return;
			}
			this._timer = new Timer(2000);
			this._timer.Elapsed += new ElapsedEventHandler(this.Tick);
			this._timer.Start();
		}

		public bool CanSafeSpend(int needed)
		{
			bool flag;
			lock (this)
			{
				flag = (this.Points >= needed ? true : false);
			}
			return flag;
		}

		public bool CanSpend(int needed)
		{
			bool flag;
			lock (this)
			{
				if (this._history.Sum() <= 0)
				{
					needed *= this.Allowance;
				}
				flag = (this.Points >= needed ? true : false);
			}
			return flag;
		}

		public void Dispose()
		{
			if (this._timer == null)
			{
				return;
			}
			this._timer.Stop();
			this._timer.Dispose();
		}

		private void ResetPoints()
		{
			lock (this)
			{
				this.Points = this.Max;
				this._history.Clear();
				for (int i = 0; i < this.HistLen; i++)
				{
					this._history.AddFirst(this.Points);
				}
			}
		}

		public bool SafeSpend(int needed)
		{
			bool flag;
			lock (this)
			{
				if (this.Points >= needed)
				{
					this.Points = this.Points - needed;
					flag = true;
				}
				else
				{
					flag = false;
				}
			}
			return flag;
		}

		public bool SetParams(dynamic param)
		{
			bool flag;
			lock (this)
			{
				param = param ?? this._paramsNormal;
				int num = (int)(param.allowance ?? this.Allowance);
				int num1 = (int)(param.max ?? this.Max);
				int num2 = (int)(param.histLen ?? this.HistLen);
				if (num != this.Allowance || num1 != this.Max || num2 != this.HistLen)
				{
					this.Allowance = num;
					this.Max = num1;
					this.HistLen = num2;
					this.ResetPoints();
					flag = true;
				}
				else
				{
					flag = false;
				}
			}
			return flag;
		}

		public bool Spend(int needed)
		{
			bool flag;
			lock (this)
			{
				if (this._history.Sum() <= 0)
				{
					needed *= this.Allowance;
				}
				if (this.Points >= needed)
				{
					this.Points = this.Points - needed;
					flag = true;
				}
				else
				{
					flag = false;
				}
			}
			return flag;
		}

		public void Tick(object sender, ElapsedEventArgs elapsedEventArgs)
		{
			lock (this)
			{
				this._history.AddFirst(this.Points);
				while (this._history.Count > this.HistLen)
				{
					this._history.RemoveLast();
				}
				if (this.Points < this.Max)
				{
					this.Points = this.Points + this.Allowance;
					if (this.Points > this.Max)
					{
						this.Points = this.Max;
					}
				}
			}
		}
	}
}