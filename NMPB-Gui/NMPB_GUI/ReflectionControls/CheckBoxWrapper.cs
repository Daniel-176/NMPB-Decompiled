using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;

namespace NMPB_GUI.ReflectionControls
{
	internal class CheckBoxWrapper : IControlWrapper, IDisposable
	{
		private bool _preventChangedFire;

		private readonly CheckBox _cb;

		public System.Windows.Forms.Control Control
		{
			get
			{
				return this._cb;
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
				return this._cb.Checked;
			}
			set
			{
				this._preventChangedFire = true;
				this._cb.Checked = (bool)value;
				this._preventChangedFire = false;
			}
		}

		public CheckBoxWrapper(string name, CheckBox cb)
		{
			this.Name = name;
			this._cb = cb;
			cb.CheckedChanged += new EventHandler(this.OnChanged);
		}

		public CheckBoxWrapper(string name, string text, bool enabled, bool check)
		{
			this.Name = name;
			this._cb = new CheckBox()
			{
				Text = text,
				Name = name,
				Checked = check,
				Enabled = enabled
			};
			this._cb.CheckedChanged += new EventHandler(this.OnChanged);
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