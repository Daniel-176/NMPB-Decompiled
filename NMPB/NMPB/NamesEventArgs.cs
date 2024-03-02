using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NMPB
{
	public class NamesEventArgs : EventArgs
	{
		private readonly List<string> _names;

		public ReadOnlyCollection<string> Names
		{
			get
			{
				return this._names.AsReadOnly();
			}
		}

		public NamesEventArgs(List<string> names)
		{
			this._names = names;
		}
	}
}