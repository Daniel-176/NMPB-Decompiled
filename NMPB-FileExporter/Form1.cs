using NMPB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace NMPB_FileExporter
{
	public class Form1 : Form
	{
		private Mutex _mainMutex = new Mutex(false, Assembly.GetEntryAssembly().Location.Replace(Path.DirectorySeparatorChar, '\u005F'));

		private NMPB.Bot Bot;

		private IContainer components;

		private Button button1;

		private CheckBox checkBox1;

		private ProgressBar progressBar1;

		private Button button2;

		private FolderBrowserDialog folderBrowserDialog1;

		private OpenFileDialog openFileDialog1;

		private BackgroundWorker backgroundWorker1;

		public Form1()
		{
			this.Bot = new NMPB.Bot("");
			this.InitializeComponent();
		}

		private void AddFiles(string[] files)
		{
			List<string> strs = new List<string>();
			string[] strArrays = files;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str = strArrays[i];
				try
				{
					this.Bot.AddFile(str, null, false);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					strs.Add(string.Format("'{0}': {1}", Path.GetFileName(str), exception.Message));
				}
			}
			if (!strs.Any<string>())
			{
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			int count = strs.Count;
			stringBuilder.AppendLine(string.Format("{0} error{1} occured:", count, (count == 1 ? "" : "s")));
			int num = Math.Min(10, count);
			for (int j = 0; j < num; j++)
			{
				stringBuilder.AppendLine(strs[j]);
			}
			if (count > num)
			{
				stringBuilder.AppendLine("...");
			}
			MessageBox.Show(stringBuilder.ToString());
		}

		private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
		{
			string str;
			string argument = (string)e.Argument;
			this._mainMutex.WaitOne();
			try
			{
				ReadOnlyCollection<string> names = this.Bot.get_Names();
				int count = names.Count;
				int length = 255 - (argument.Length + 1 + 4 + 2 + count.ToString().Length);
				if (length < 0 || this.checkBox1.Checked && length < names.Count.ToString().Length + 2)
				{
					throw new Exception("Filepath too long");
				}
				for (int i = 0; i < names.Count; i++)
				{
					string fullMidiPath = this.Bot.GetFullMidiPath(i);
					if (File.Exists(fullMidiPath))
					{
						string str1 = Form1.escapeFileName(names[i]);
						if (str1.Length > length)
						{
							str1 = str1.Substring(0, length);
						}
						if (str1.EndsWith(".mid"))
						{
							str1 = str1.Substring(0, str1.Length - 4);
						}
						string str2 = Path.Combine(argument, string.Format((this.checkBox1.Checked ? "{1}. {0}" : "{0}"), str1, i));
						int num = 0;
						while (true)
						{
							string str3 = this.GenName(str2, num);
							str = str3;
							if (!File.Exists(str3))
							{
								break;
							}
							num++;
						}
						File.Copy(fullMidiPath, str);
						this.backgroundWorker1.ReportProgress(100 * i / names.Count);
					}
				}
			}
			finally
			{
				this._mainMutex.ReleaseMutex();
			}
		}

		private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			this.progressBar1.Value = e.ProgressPercentage;
		}

		private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			this.progressBar1.Value = 0;
			MessageBox.Show("Export completed.");
		}

		private void button1_Click(object sender, EventArgs e)
		{
			if (this.folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				this.backgroundWorker1.RunWorkerAsync(this.folderBrowserDialog1.SelectedPath);
			}
		}

		private void button2_Click(object sender, EventArgs e)
		{
			this.openFileDialog1.ShowDialog();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		private static string escapeFileName(string str)
		{
			return ((IEnumerable<char>)Path.GetInvalidFileNameChars()).Aggregate<char, string>(str, (string current, char c) => current.Replace(c.ToString(CultureInfo.InvariantCulture), string.Empty));
		}

		private void Form1_DragDrop(object sender, DragEventArgs e)
		{
			string[] data = (string[])e.Data.GetData(DataFormats.FileDrop);
			this.AddFiles(data);
		}

		private void Form1_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				e.Effect = DragDropEffects.Copy;
			}
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			this.AllowDrop = true;
			base.DragEnter += new DragEventHandler(this.Form1_DragEnter);
			base.DragDrop += new DragEventHandler(this.Form1_DragDrop);
		}

		private string GenName(string path, int copy)
		{
			return string.Format("{0}{1}.mid", path, (copy > 0 ? string.Concat("(", copy, ")") : ""));
		}

		private void InitializeComponent()
		{
			this.button1 = new Button();
			this.checkBox1 = new CheckBox();
			this.progressBar1 = new ProgressBar();
			this.button2 = new Button();
			this.folderBrowserDialog1 = new FolderBrowserDialog();
			this.openFileDialog1 = new OpenFileDialog();
			this.backgroundWorker1 = new BackgroundWorker();
			base.SuspendLayout();
			this.button1.Location = new Point(12, 12);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(111, 25);
			this.button1.TabIndex = 0;
			this.button1.Text = "Export";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new EventHandler(this.button1_Click);
			this.checkBox1.AutoSize = true;
			this.checkBox1.Checked = true;
			this.checkBox1.CheckState = CheckState.Checked;
			this.checkBox1.Location = new Point(24, 43);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(85, 17);
			this.checkBox1.TabIndex = 1;
			this.checkBox1.Text = "Add Indexes";
			this.checkBox1.UseVisualStyleBackColor = true;
			this.progressBar1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			this.progressBar1.Location = new Point(12, 101);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(109, 30);
			this.progressBar1.TabIndex = 2;
			this.button2.Location = new Point(12, 66);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(111, 25);
			this.button2.TabIndex = 3;
			this.button2.Text = "Import";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new EventHandler(this.button2_Click);
			this.openFileDialog1.FileName = "openFileDialog1";
			this.openFileDialog1.Multiselect = true;
			this.openFileDialog1.FileOk += new CancelEventHandler(this.openFileDialog1_FileOk);
			this.backgroundWorker1.WorkerReportsProgress = true;
			this.backgroundWorker1.WorkerSupportsCancellation = true;
			this.backgroundWorker1.DoWork += new DoWorkEventHandler(this.backgroundWorker1_DoWork);
			this.backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
			this.backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(133, 143);
			base.Controls.Add(this.button2);
			base.Controls.Add(this.progressBar1);
			base.Controls.Add(this.checkBox1);
			base.Controls.Add(this.button1);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			base.Name = "Form1";
			this.Text = "NMPB File Exporter";
			base.Load += new EventHandler(this.Form1_Load);
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
		{
			if (e.Cancel)
			{
				return;
			}
			this.AddFiles(this.openFileDialog1.FileNames);
		}
	}
}