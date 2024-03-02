using System;
using System.Runtime.CompilerServices;

namespace NMPB.Timers
{
	public interface ITimer
	{
		long Period
		{
			get;
			set;
		}

		void Dispose();

		void Start();

		void Stop();

		event EventHandler Disposed;

		event EventHandler Started;

		event EventHandler Stopped;

		event EventHandler Tick;
	}
}