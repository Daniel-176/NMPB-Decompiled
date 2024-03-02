using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;

namespace NMPB_GUI.ReflectionControls
{
	internal class NumericUpDownWrapper : IControlWrapper, IDisposable
	{
		private bool _preventChangedFire;

		private readonly NumericUpDown _control;

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
				return (int)this._control.Value;
			}
			set
			{
				this._preventChangedFire = true;
				if ((int)value < this._control.Minimum)
				{
					this._control.Minimum = new decimal(-1, -1, -1, true, 0);
				}
				this._control.Value = (int)value;
				this._preventChangedFire = false;
			}
		}

		public NumericUpDownWrapper(string name, NumericUpDown control)
		{
			this.Name = name;
			this._control = control;
			this._control.ValueChanged += new EventHandler(this.OnChanged);
		}

		public NumericUpDownWrapper(string name, string text, bool enabled, int defaultValue)
		{
			this.Name = name;
			this._control = new NumericUpDown()
			{
				Text = text,
				Name = name,
				Minimum = decimal.One,
				Maximum = new decimal(-1, -1, -1, false, 0)
			};
			if (defaultValue >= this._control.Minimum)
			{
				this._control.Value = defaultValue;
			}
			this._control.Enabled = enabled;
			this._control.ValueChanged += new EventHandler(this.OnChanged);
		}

		public void Dispose()
		{
			this.Control.Dispose();
		}

		private void OnChanged(object sender, EventArgs e)
		{
			if (this._preventChangedFire)
			{
				return;
			}
			EventHandler eventHandler = this.Changed;
			if (eventHandler != null)
			{
				eventHandler(this, e);
			}
		}

		public event EventHandler Changed;
	}
}