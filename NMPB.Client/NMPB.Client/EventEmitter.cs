using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NMPB.Client
{
	public class EventEmitter
	{
		private readonly Dictionary<string, Action<object[]>> _events = new Dictionary<string, Action<object[]>>();

		public EventEmitter()
		{
		}

		public void Emit(string evnt, params object[] arguments)
		{
			if (!this._events.ContainsKey(evnt))
			{
				return;
			}
			this._events[evnt](arguments);
		}

		public void Off(string evnt, Action<object[]> fn)
		{
			if (!this._events.ContainsKey(evnt))
			{
				return;
			}
			Dictionary<string, Action<object[]>> strs = this._events;
			string str = evnt;
			strs[str] = (Action<object[]>)Delegate.Remove(strs[str], fn);
		}

		public void On(string evnt, Action<object[]> fn)
		{
			if (!this._events.ContainsKey(evnt))
			{
				this._events.Add(evnt, new Action<object[]>((object[] o) => {
				}));
			}
			Dictionary<string, Action<object[]>> strs = this._events;
			string str = evnt;
			strs[str] = (Action<object[]>)Delegate.Combine(strs[str], fn);
		}
	}
}