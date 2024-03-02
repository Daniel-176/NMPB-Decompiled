using System;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace NMPB_GUI.ReflectionControls
{
	public interface IControlWrapper : IDisposable
	{
		System.Windows.Forms.Control Control
		{
			get;
		}

		string Name
		{
			get;
			set;
		}

		object Value
		{
			get;
			set;
		}

		event EventHandler Changed;
	}
}