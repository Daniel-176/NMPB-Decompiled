using NMPB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace NMPB_GUI
{
	public class Commands : Form
	{
		public static string InitialCommands;

		private string _currentGroup;

		private readonly Form1 _form;

		private IContainer components;

		static Commands()
		{
			Commands.InitialCommands = "%%Text:| Title Song Name Playing| Help H| Search Find S F| List| Color| About| Info| Gsun|%%Upload:| Upload U Download D|%%Sequencer:| Auto A| Load L Play P| Stop| Restart| Random R| Set Pos| Sustain| PlatNext Pn| PlayPrev Pp|%%Turns:| Turn T| Voteskip VS| Afk | Skipme| Queue Q| Mysustain Mysus|%%No Turns| Backwards b| Mistakes| Tempo| Inverse Mirror";
		}

		public Commands()
		{
			this.InitializeComponent();
		}

		public Commands(Form1 form)
		{
			this.InitializeComponent();
			int num = 10;
			int num1 = num;
			base.Height = num;
			base.Width = num1;
			this._form = form;
			this.AddRootSet(Commands.InitialCommands);
		}

		private void AddCheckBox(string name, int l, int t)
		{
			MyCheckBox myCheckBox = new MyCheckBox()
			{
				Text = name,
				Left = l,
				Top = t,
				Height = 20,
				Group = this._currentGroup
			};
			string lower = name.ToLower();
			myCheckBox.Checked = this._form.Bot.AvalibleCommandsSet.Any<KeyValuePair<string, HashSet<string>>>((KeyValuePair<string, HashSet<string>> pair) => pair.Value.Contains<string>(lower, StringComparer.OrdinalIgnoreCase));
			myCheckBox.CheckedChanged += new EventHandler(this.OnChange);
			base.Controls.Add(myCheckBox);
		}

		private void AddLabel(string s, int top)
		{
			this._currentGroup = s.Trim(new char[] { '%', ' ', ':' });
			Label label = new Label()
			{
				Text = s,
				Left = 6,
				Top = top,
				Height = 15
			};
			base.Controls.Add(label);
		}

		private void AddRootSet(string set)
		{
			char[] chrArray = new char[] { '|' };
			int num = 10;
			string[] strArrays = set.Split(chrArray, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str = strArrays[i];
				if (!str.StartsWith("%%"))
				{
					this.AddSet(str, num);
					num += 20;
				}
				else
				{
					this.AddLabel(str.Substring(2), num);
					num += 15;
				}
			}
			base.Height = Math.Max(base.Height, num + 40);
		}

		private void AddSet(string set, int top)
		{
			char[] chrArray = new char[] { ' ' };
			int num = 10;
			string[] strArrays = set.Split(chrArray, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				this.AddCheckBox(strArrays[i], num, top);
				num += 120;
			}
			base.Width = Math.Max(base.Width, num);
		}

		public static void ApplyCommands(Bot bot)
		{
			string initialCommands = Commands.InitialCommands;
			char[] chrArray = new char[] { '|' };
			string str = "";
			string[] strArrays = initialCommands.Split(chrArray, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str1 = strArrays[i];
				if (!str1.StartsWith("%%"))
				{
					string[] strArrays1 = str1.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
					for (int j = 0; j < (int)strArrays1.Length; j++)
					{
						string str2 = strArrays1[j];
						bot.AvalibleCommandsSet[str].Add(str2.ToLower());
					}
				}
				else
				{
					str = str1.Trim(new char[] { ' ', '%', ':' });
				}
			}
		}

		private void Commands_Load(object sender, EventArgs e)
		{
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
			base.SuspendLayout();
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(348, 100);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			base.Name = "Commands";
			base.StartPosition = FormStartPosition.CenterParent;
			this.Text = "Commands";
			base.Load += new EventHandler(this.Commands_Load);
			base.ResumeLayout(false);
		}

		private void OnChange(object sender, EventArgs e)
		{
			MyCheckBox myCheckBox = sender as MyCheckBox;
			if (myCheckBox == null)
			{
				return;
			}
			string lower = myCheckBox.Text.ToLower();
			if (myCheckBox.Checked)
			{
				this._form.Bot.AvalibleCommandsSet[myCheckBox.Group].Add(lower);
				return;
			}
			this._form.Bot.AvalibleCommandsSet[myCheckBox.Group].Remove(lower);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			this._form.SaveConfig();
			base.OnClosing(e);
		}
	}
}