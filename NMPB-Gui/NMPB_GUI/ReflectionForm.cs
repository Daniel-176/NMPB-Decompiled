using NMPB;
using NMPB_GUI.ReflectionControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace NMPB_GUI
{
	public class ReflectionForm : Form
	{
		private readonly ReflectionManager<Bot> _manager;

		private IContainer components;

		private FlowLayoutPanel flowLayoutPanel1;

		public ReflectionForm(Bot bot, string title)
		{
			this.InitializeComponent();
			this.SetTitle(title);
			this._manager = new ReflectionManager<Bot>(bot, true);
			List<IControlWrapper> list = (
				from pair in this._manager.Controls.ToList<KeyValuePair<string, IControlWrapper>>()
				select pair.Value).ToList<IControlWrapper>();
			list = (
				from wrapper in list
				orderby wrapper.ToString(), wrapper.Control.Enabled, wrapper.Name
				select wrapper).ToList<IControlWrapper>();
			for (int i = 0; i < list.Count; i++)
			{
				IControlWrapper item = list[i];
				if (item.Control is NumericUpDown)
				{
					if (i > 0 && !(list[i - 1].Control is NumericUpDown) && list[i - 1].Control.Enabled)
					{
						this.flowLayoutPanel1.SetFlowBreak(list[i - 1].Control, true);
					}
					Label label = new Label()
					{
						Width = 130,
						TextAlign = ContentAlignment.MiddleRight,
						Text = string.Concat(ReflectionManager<Bot>.AddSpacesToSentence(item.Control.Name, true), ":"),
						Padding = new System.Windows.Forms.Padding(0, 0, 0, 0)
					};
					this.flowLayoutPanel1.Controls.Add(label);
					this.flowLayoutPanel1.SetFlowBreak(item.Control, true);
				}
				if (item.Control is CheckBox)
				{
					item.Control.Width = 130;
				}
				this.flowLayoutPanel1.Controls.Add(item.Control);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(ReflectionForm));
			this.flowLayoutPanel1 = new FlowLayoutPanel();
			base.SuspendLayout();
			this.flowLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			this.flowLayoutPanel1.AutoScroll = true;
			this.flowLayoutPanel1.BackColor = SystemColors.Control;
			this.flowLayoutPanel1.Location = new Point(12, 12);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(438, 521);
			this.flowLayoutPanel1.TabIndex = 0;
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(462, 545);
			base.Controls.Add(this.flowLayoutPanel1);
			base.Icon = (System.Drawing.Icon)componentResourceManager.GetObject("$this.Icon");
			base.Name = "ReflectionForm";
			this.Text = "Reflection";
			base.Load += new EventHandler(this.ReflectionForm_Load);
			base.ResumeLayout(false);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			this._manager.Dispose();
			base.OnClosing(e);
		}

		private void ReflectionForm_Load(object sender, EventArgs e)
		{
		}

		public void SetTitle(string text)
		{
			this.Text = string.Concat("Reflection - ", text);
		}
	}
}