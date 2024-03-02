using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;

namespace NMPB_GUI.ReflectionControls
{
	internal class ColorWrapper : IControlWrapper, IDisposable
	{
		private Button _control;

		public System.Windows.Forms.Control Control
		{
			get
			{
				return this._control;
			}
		}

		public string Name
		{
			get;
			set;
		}

		public object Value
		{
			get
			{
				return this._control.BackColor;
			}
			set
			{
				this._control.BackColor = (Color)value;
			}
		}

		public ColorWrapper(string name, Button button)
		{
			this.Name = name;
			this._control = button;
		}

		public void Dispose()
		{
			this._control.Dispose();
		}

		public event EventHandler Changed;
	}
}