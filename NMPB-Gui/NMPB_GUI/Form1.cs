using Newtonsoft.Json;
using NMPB;
using NMPB.Client;
using NMPB_GUI.ReflectionControls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace NMPB_GUI
{
	public class Form1 : Form
	{
		public NMPB.Bot Bot;

		public ReflectionManager<NMPB.Bot> Manager;

		private ReflectionForm _reflectionForm;

		private string _configFile;

		private bool loaded;

		private bool ready;

		private string _showedName = "NMPB";

		private readonly Type[] _simpleTypes = new Type[] { typeof(bool), typeof(int), typeof(double), typeof(string), typeof(Color) };

		private IContainer components;

		private Button bConnect;

		private OpenFileDialog openFileDialog1;

		private TextBox textBox1;

		private TextBox textBox3;

		private Button bClearChat;

		private CheckBox cReceiveChat;

		private Button bToTray;

		private NotifyIcon notifyIcon1;

		private CheckBox cSendChat;

		private Label label1;

		private ComboBox comboBox1;

		private Button bDisconnect;

		private CheckBox cTurns;

		private ComboBox comboBox2;

		private Label label2;

		private CheckBox cVisible;

		private CheckBox cSolo;

		private NumericUpDown fontSizeUpDown1;

		private CheckBox cChat;

		private CheckBox cbAllowOtherBots1;

		private Button buttonGTFO;

		private Button bReflection;

		private RichTextBox richTextBox1;

		private CheckBox cbScroll;

		private CheckBox topMostCheckBox;

		private Button colorButton;

		private ColorDialog colorDialog1;

		public string room
		{
			get
			{
				return this.textBox1.Text;
			}
			set
			{
				this.textBox1.Text = value;
			}
		}

		public string ShowedName
		{
			get
			{
				return this._showedName;
			}
			set
			{
				if (string.IsNullOrWhiteSpace(value))
				{
					this._showedName = "NMPB";
					return;
				}
				if (value.Length > 33)
				{
					value = value.Substring(0, 33);
				}
				this._showedName = string.Format("NMPB ({0})", value);
			}
		}

		public Form1(string configFile)
		{
			this.InitializeComponent();
			this.Bot = new NMPB.Bot("");
			Commands.ApplyCommands(this.Bot);
			this._configFile = configFile;
		}

		private void AddFiles(string[] files)
		{
			string[] strArrays = files;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str = strArrays[i];
				try
				{
					if (Directory.Exists(str))
					{
						this.AddFiles(Directory.GetFiles(str, "*.*", SearchOption.AllDirectories).Where<string>((string s) => {
							string extension = Path.GetExtension(s);
							if (extension == null)
							{
								return false;
							}
							if (extension.Equals(".mid", StringComparison.InvariantCultureIgnoreCase))
							{
								return true;
							}
							return extension.Equals(".midi", StringComparison.InvariantCultureIgnoreCase);
						}).ToArray<string>());
					}
					string str1 = Path.GetExtension(str);
					if (str1 != null)
					{
						if (str1.Equals(".mid", StringComparison.InvariantCultureIgnoreCase) || str1.Equals(".midi", StringComparison.InvariantCultureIgnoreCase))
						{
							this.Bot.AddFile(str, null, false);
						}
						else if (str1.Equals(".xml", StringComparison.InvariantCultureIgnoreCase))
						{
							this.ResaveLocalization(str);
						}
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					this.Bot.Say(string.Format(this.Bot.L.ReadingFileException, string.Concat(exception.Message, " ", str)));
				}
			}
		}

		private void AddReflectionChechbox(string name, CheckBox cb)
		{
			this.Manager.RegisterWrapper(name, new CheckBoxWrapper(name, cb));
		}

		public void AppendColoredText(RichTextBox box, string text, Color color)
		{
			box.SelectionStart = box.TextLength;
			box.SelectionLength = 0;
			box.SelectionColor = color;
			box.AppendText(text);
			box.SelectionColor = box.ForeColor;
		}

		private void bClearChat_Click(object sender, EventArgs e)
		{
			this.richTextBox1.Clear();
		}

		private void bConnect_Click(object sender, EventArgs e)
		{
			this.room = Player.PrepareRoomName(this.room);
			if (NMPB.Bot.IsRoomNameForbidden(this.room))
			{
				this.room = string.Concat("NMPB ", this.room);
			}
			this.Bot.Connect(this.room);
			this.notifyIcon1.Text = (this.room.Length < 64 ? this.room : this.room.Substring(0, 63));
			this.Text = string.Concat("NMPB - ", this.room);
			if (this._reflectionForm != null && !this._reflectionForm.IsDisposed)
			{
				this._reflectionForm.SetTitle(this.room);
			}
		}

		private void bDisconnectClick(object sender, EventArgs e)
		{
			this.Bot.Disconnect();
		}

		private void bReflection_Click(object sender, EventArgs e)
		{
			this._reflectionForm = new ReflectionForm(this.Bot, this.room);
			this._reflectionForm.Show();
		}

		private void bToTray_Click(object sender, EventArgs e)
		{
			this.notifyIcon1.Visible = true;
			base.Hide();
		}

		private void buttonGTFO_Click(object sender, EventArgs e)
		{
			this.Bot.SendGTFO();
		}

		private void colorButton_Click(object sender, EventArgs e)
		{
			this.colorDialog1.Color = this.colorButton.BackColor;
			this.colorDialog1.ShowDialog();
			this.colorButton.BackColor = this.colorDialog1.Color;
			this.Bot.RoomColor = this.colorDialog1.Color;
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.Bot.AvalibleCommandsType = (AvalibleCommandsType)this.comboBox1.SelectedIndex;
			if (this.ready && this.Bot.AvalibleCommandsType == AvalibleCommandsType.Custom)
			{
				(new Commands(this)).Show();
			}
		}

		private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.SaveConfig();
			try
			{
				this.Bot.L = Localization.Load(Path.Combine(this.Bot.RootDirectory, "localization", string.Concat(this.comboBox2.SelectedItem, ".xml")));
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				this.Bot.Say(string.Format("Unable to load localization. {0}", exception.Message));
			}
		}

		private void commandsButton_Click(object sender, EventArgs e)
		{
			(new Commands(this)).Show();
		}

		private void CreateFileIfNotExists(string name)
		{
			if (!File.Exists(name))
			{
				File.Create(name).Close();
			}
		}

		private void cReceiveChat_CheckedChanged(object sender, EventArgs e)
		{
			this.loaded = this.cReceiveChat.Checked;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void FindLocalozation()
		{
			this.comboBox2.Items.Clear();
			string rootDirectory = this.Bot.RootDirectory;
			char directorySeparatorChar = Path.DirectorySeparatorChar;
			foreach (string list in Directory.GetFiles(Path.Combine(rootDirectory, string.Concat("localization", directorySeparatorChar.ToString())), "*.xml").Select<string, string>(new Func<string, string>(Path.GetFileNameWithoutExtension)).Where<string>((string filename) => filename != null).ToList<string>())
			{
				this.comboBox2.Items.Add(list);
			}
		}

		private void Form1_DragDrop(object sender, DragEventArgs e)
		{
			string[] data = (string[])e.Data.GetData(DataFormats.FileDrop);
			DelayedThread delayedThread = new DelayedThread(() => this.AddFiles(data), 0);
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
			this.comboBox1.SelectedItem = "All";
			this.LoadConfig(this._configFile);
			this.LoadConfig();
			this.colorDialog1.CustomColors = new int[] { 15530733 };
			this.colorButton.BackColor = this.Bot.RoomColor;
			if (this.Bot.WorkingWirectory != Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
			{
				this.richTextBox1.AppendText(string.Format(this.Bot.L.WorkingDirectory, this.Bot.WorkingWirectory));
			}
			this.Bot.ChatLogged += new EventHandler<TextMessageEventArgs>(this.OnChatMessage);
			this.notifyIcon1.Icon = new System.Drawing.Icon(base.Icon, 16, 16);
			this.loaded = true;
			this.ready = true;
			this.AllowDrop = true;
			base.DragEnter += new DragEventHandler(this.Form1_DragEnter);
			base.DragDrop += new DragEventHandler(this.Form1_DragDrop);
			this.richTextBox1.LanguageOption = RichTextBoxLanguageOptions.DualFont;
			this.Manager = new ReflectionManager<NMPB.Bot>(this.Bot, false);
			this.AddReflectionChechbox("RoomIsVisible", this.cVisible);
			this.AddReflectionChechbox("RoomHasChat", this.cChat);
			this.AddReflectionChechbox("RoomSoloPlay", this.cSolo);
			this.AddReflectionChechbox("SendChat", this.cSendChat);
			this.AddReflectionChechbox("Turns", this.cTurns);
			this.AddReflectionChechbox("AllowOtherBots", this.cbAllowOtherBots1);
			this.Manager.RegisterWrapper("RoomColor", new ColorWrapper("RoomColor", this.colorButton));
			this.SaveDefaultConfig("default-settings.txt");
		}

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(Form1));
			this.bConnect = new Button();
			this.openFileDialog1 = new OpenFileDialog();
			this.textBox1 = new TextBox();
			this.textBox3 = new TextBox();
			this.bClearChat = new Button();
			this.cReceiveChat = new CheckBox();
			this.bToTray = new Button();
			this.notifyIcon1 = new NotifyIcon(this.components);
			this.cSendChat = new CheckBox();
			this.label1 = new Label();
			this.comboBox1 = new ComboBox();
			this.bDisconnect = new Button();
			this.cTurns = new CheckBox();
			this.comboBox2 = new ComboBox();
			this.label2 = new Label();
			this.cVisible = new CheckBox();
			this.cSolo = new CheckBox();
			this.fontSizeUpDown1 = new NumericUpDown();
			this.cChat = new CheckBox();
			this.cbAllowOtherBots1 = new CheckBox();
			this.buttonGTFO = new Button();
			this.bReflection = new Button();
			this.richTextBox1 = new RichTextBox();
			this.cbScroll = new CheckBox();
			this.topMostCheckBox = new CheckBox();
			this.colorButton = new Button();
			this.colorDialog1 = new ColorDialog();
			((ISupportInitialize)this.fontSizeUpDown1).BeginInit();
			base.SuspendLayout();
			this.bConnect.Location = new Point(263, 11);
			this.bConnect.Name = "bConnect";
			this.bConnect.Size = new System.Drawing.Size(89, 23);
			this.bConnect.TabIndex = 0;
			this.bConnect.Text = "Connect";
			this.bConnect.UseVisualStyleBackColor = true;
			this.bConnect.Click += new EventHandler(this.bConnect_Click);
			this.openFileDialog1.FileName = "openFileDialog1";
			this.openFileDialog1.Filter = "MIDI|*.mid";
			this.textBox1.Location = new Point(12, 13);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(245, 20);
			this.textBox1.TabIndex = 4;
			this.textBox1.Text = "NMPB Room";
			this.textBox1.KeyPress += new KeyPressEventHandler(this.textBox1_KeyPress);
			this.textBox3.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			this.textBox3.Location = new Point(12, 402);
			this.textBox3.Name = "textBox3";
			this.textBox3.Size = new System.Drawing.Size(1029, 20);
			this.textBox3.TabIndex = 8;
			this.textBox3.KeyPress += new KeyPressEventHandler(this.textBox3_KeyPress);
			this.bClearChat.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			this.bClearChat.Location = new Point(966, 37);
			this.bClearChat.Name = "bClearChat";
			this.bClearChat.Size = new System.Drawing.Size(75, 23);
			this.bClearChat.TabIndex = 9;
			this.bClearChat.Text = "Clear";
			this.bClearChat.UseVisualStyleBackColor = true;
			this.bClearChat.Click += new EventHandler(this.bClearChat_Click);
			this.cReceiveChat.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			this.cReceiveChat.AutoSize = true;
			this.cReceiveChat.Checked = true;
			this.cReceiveChat.CheckState = CheckState.Checked;
			this.cReceiveChat.Location = new Point(870, 27);
			this.cReceiveChat.Name = "cReceiveChat";
			this.cReceiveChat.Size = new System.Drawing.Size(90, 17);
			this.cReceiveChat.TabIndex = 10;
			this.cReceiveChat.Text = "Receive chat";
			this.cReceiveChat.UseVisualStyleBackColor = true;
			this.cReceiveChat.CheckedChanged += new EventHandler(this.cReceiveChat_CheckedChanged);
			this.bToTray.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			this.bToTray.Location = new Point(966, 11);
			this.bToTray.Name = "bToTray";
			this.bToTray.Size = new System.Drawing.Size(75, 22);
			this.bToTray.TabIndex = 12;
			this.bToTray.Text = "To Tray";
			this.bToTray.UseVisualStyleBackColor = true;
			this.bToTray.Click += new EventHandler(this.bToTray_Click);
			this.notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
			this.notifyIcon1.BalloonTipText = "NMPB is here.";
			this.notifyIcon1.Text = "Not Connected";
			this.notifyIcon1.MouseClick += new MouseEventHandler(this.notifyIcon1_MouseClick);
			this.cSendChat.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			this.cSendChat.AutoSize = true;
			this.cSendChat.Checked = true;
			this.cSendChat.CheckState = CheckState.Checked;
			this.cSendChat.Location = new Point(870, 10);
			this.cSendChat.Name = "cSendChat";
			this.cSendChat.Size = new System.Drawing.Size(75, 17);
			this.cSendChat.TabIndex = 13;
			this.cSendChat.Text = "Send chat";
			this.cSendChat.UseVisualStyleBackColor = true;
			this.label1.AutoSize = true;
			this.label1.Location = new Point(355, 43);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(62, 13);
			this.label1.TabIndex = 14;
			this.label1.Text = "Commands:";
			this.comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
			this.comboBox1.FormattingEnabled = true;
			this.comboBox1.Items.AddRange(new object[] { "No", "Title", "Text", "Text and Upload", "All", "Custom" });
			this.comboBox1.Location = new Point(423, 38);
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.Size = new System.Drawing.Size(108, 21);
			this.comboBox1.TabIndex = 15;
			this.comboBox1.SelectedIndexChanged += new EventHandler(this.comboBox1_SelectedIndexChanged);
			this.bDisconnect.Location = new Point(263, 38);
			this.bDisconnect.Name = "bDisconnect";
			this.bDisconnect.Size = new System.Drawing.Size(89, 23);
			this.bDisconnect.TabIndex = 16;
			this.bDisconnect.Text = "Disconnect";
			this.bDisconnect.UseVisualStyleBackColor = true;
			this.bDisconnect.Click += new EventHandler(this.bDisconnectClick);
			this.cTurns.AutoSize = true;
			this.cTurns.Location = new Point(185, 41);
			this.cTurns.Name = "cTurns";
			this.cTurns.Size = new System.Drawing.Size(53, 17);
			this.cTurns.TabIndex = 17;
			this.cTurns.Text = "Turns";
			this.cTurns.UseVisualStyleBackColor = true;
			this.comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
			this.comboBox2.FormattingEnabled = true;
			this.comboBox2.Location = new Point(423, 11);
			this.comboBox2.Name = "comboBox2";
			this.comboBox2.Size = new System.Drawing.Size(108, 21);
			this.comboBox2.TabIndex = 19;
			this.comboBox2.SelectedIndexChanged += new EventHandler(this.comboBox2_SelectedIndexChanged);
			this.label2.AutoSize = true;
			this.label2.Location = new Point(359, 14);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(58, 13);
			this.label2.TabIndex = 20;
			this.label2.Text = "Language:";
			this.cVisible.AutoSize = true;
			this.cVisible.Checked = true;
			this.cVisible.CheckState = CheckState.Checked;
			this.cVisible.Location = new Point(12, 41);
			this.cVisible.Name = "cVisible";
			this.cVisible.Size = new System.Drawing.Size(56, 17);
			this.cVisible.TabIndex = 21;
			this.cVisible.Text = "Visible";
			this.cVisible.UseVisualStyleBackColor = true;
			this.cSolo.AutoSize = true;
			this.cSolo.Location = new Point(132, 41);
			this.cSolo.Name = "cSolo";
			this.cSolo.Size = new System.Drawing.Size(47, 17);
			this.cSolo.TabIndex = 23;
			this.cSolo.Text = "Solo";
			this.cSolo.UseVisualStyleBackColor = true;
			this.fontSizeUpDown1.Location = new Point(618, 13);
			this.fontSizeUpDown1.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
			this.fontSizeUpDown1.Name = "fontSizeUpDown1";
			this.fontSizeUpDown1.Size = new System.Drawing.Size(54, 20);
			this.fontSizeUpDown1.TabIndex = 24;
			this.fontSizeUpDown1.Value = new decimal(new int[] { 10, 0, 0, 0 });
			this.fontSizeUpDown1.Visible = false;
			this.fontSizeUpDown1.ValueChanged += new EventHandler(this.numericUpDown1_ValueChanged_1);
			this.cChat.AutoSize = true;
			this.cChat.Checked = true;
			this.cChat.CheckState = CheckState.Checked;
			this.cChat.Location = new Point(78, 41);
			this.cChat.Name = "cChat";
			this.cChat.Size = new System.Drawing.Size(48, 17);
			this.cChat.TabIndex = 25;
			this.cChat.Text = "Chat";
			this.cChat.UseVisualStyleBackColor = true;
			this.cbAllowOtherBots1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			this.cbAllowOtherBots1.AutoSize = true;
			this.cbAllowOtherBots1.Location = new Point(763, 16);
			this.cbAllowOtherBots1.Name = "cbAllowOtherBots1";
			this.cbAllowOtherBots1.Size = new System.Drawing.Size(101, 17);
			this.cbAllowOtherBots1.TabIndex = 26;
			this.cbAllowOtherBots1.Text = "Allow other bots";
			this.cbAllowOtherBots1.UseVisualStyleBackColor = true;
			this.buttonGTFO.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			this.buttonGTFO.Location = new Point(763, 37);
			this.buttonGTFO.Name = "buttonGTFO";
			this.buttonGTFO.Size = new System.Drawing.Size(101, 23);
			this.buttonGTFO.TabIndex = 27;
			this.buttonGTFO.Text = "Kick bots";
			this.buttonGTFO.UseVisualStyleBackColor = true;
			this.buttonGTFO.Click += new EventHandler(this.buttonGTFO_Click);
			this.bReflection.Location = new Point(537, 10);
			this.bReflection.Name = "bReflection";
			this.bReflection.Size = new System.Drawing.Size(75, 23);
			this.bReflection.TabIndex = 28;
			this.bReflection.Text = "Reflection";
			this.bReflection.UseVisualStyleBackColor = true;
			this.bReflection.Click += new EventHandler(this.bReflection_Click);
			this.richTextBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			this.richTextBox1.Font = new System.Drawing.Font("Consolas", 9.75f, FontStyle.Regular, GraphicsUnit.Point, 204);
			this.richTextBox1.Location = new Point(12, 68);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.ReadOnly = true;
			this.richTextBox1.Size = new System.Drawing.Size(1029, 328);
			this.richTextBox1.TabIndex = 30;
			this.richTextBox1.Text = "";
			this.richTextBox1.LinkClicked += new LinkClickedEventHandler(this.richTextBox1_LinkClicked);
			this.richTextBox1.TextChanged += new EventHandler(this.richTextBox1_TextChanged);
			this.richTextBox1.KeyPress += new KeyPressEventHandler(this.richTextBox1_KeyPress);
			this.cbScroll.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			this.cbScroll.AutoSize = true;
			this.cbScroll.Checked = true;
			this.cbScroll.CheckState = CheckState.Checked;
			this.cbScroll.Location = new Point(870, 44);
			this.cbScroll.Name = "cbScroll";
			this.cbScroll.Size = new System.Drawing.Size(76, 17);
			this.cbScroll.TabIndex = 31;
			this.cbScroll.Text = "Scroll chat";
			this.cbScroll.UseVisualStyleBackColor = true;
			this.topMostCheckBox.AutoSize = true;
			this.topMostCheckBox.Location = new Point(537, 41);
			this.topMostCheckBox.Name = "topMostCheckBox";
			this.topMostCheckBox.Size = new System.Drawing.Size(92, 17);
			this.topMostCheckBox.TabIndex = 32;
			this.topMostCheckBox.Text = "Always on top";
			this.topMostCheckBox.UseVisualStyleBackColor = true;
			this.topMostCheckBox.CheckedChanged += new EventHandler(this.topMostCheckBox_CheckedChanged);
			this.colorButton.BackColor = Color.FromArgb(255, 255, 128);
			this.colorButton.Location = new Point(237, 39);
			this.colorButton.Name = "colorButton";
			this.colorButton.Size = new System.Drawing.Size(20, 20);
			this.colorButton.TabIndex = 34;
			this.colorButton.UseVisualStyleBackColor = false;
			this.colorButton.Click += new EventHandler(this.colorButton_Click);
			this.colorDialog1.FullOpen = true;
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(1053, 430);
			base.Controls.Add(this.colorButton);
			base.Controls.Add(this.topMostCheckBox);
			base.Controls.Add(this.cbScroll);
			base.Controls.Add(this.richTextBox1);
			base.Controls.Add(this.bReflection);
			base.Controls.Add(this.buttonGTFO);
			base.Controls.Add(this.cbAllowOtherBots1);
			base.Controls.Add(this.cChat);
			base.Controls.Add(this.fontSizeUpDown1);
			base.Controls.Add(this.cSolo);
			base.Controls.Add(this.cVisible);
			base.Controls.Add(this.label2);
			base.Controls.Add(this.comboBox2);
			base.Controls.Add(this.cTurns);
			base.Controls.Add(this.bDisconnect);
			base.Controls.Add(this.comboBox1);
			base.Controls.Add(this.label1);
			base.Controls.Add(this.cSendChat);
			base.Controls.Add(this.bToTray);
			base.Controls.Add(this.cReceiveChat);
			base.Controls.Add(this.bClearChat);
			base.Controls.Add(this.textBox3);
			base.Controls.Add(this.textBox1);
			base.Controls.Add(this.bConnect);
			//base.Icon = (System.Drawing.Icon)componentResourceManager.GetObject("$this.Icon");
			base.Name = "Form1";
			this.Text = "NMPB";
			base.Load += new EventHandler(this.Form1_Load);
			((ISupportInitialize)this.fontSizeUpDown1).EndInit();
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		private void LoadConfig()
		{
			int num;
			this.FindLocalozation();
			this.CreateFileIfNotExists(Path.Combine(this.Bot.RootDirectory, "config.txt"));
			string str = null;
			string str1 = null;
			using (TextReader streamReader = new StreamReader(Path.Combine(this.Bot.RootDirectory, "config.txt")))
			{
				try
				{
					str = streamReader.ReadLine();
					str1 = streamReader.ReadLine();
				}
				catch (Exception exception)
				{
				}
			}
			if (str1 == "true")
			{
				this.fontSizeUpDown1.Visible = true;
			}
			if (str == null)
			{
				int num1 = this.comboBox2.Items.IndexOf("Default");
				ComboBox comboBox = this.comboBox2;
				if (num1 >= 0)
				{
					num = num1;
				}
				else
				{
					num = (this.comboBox2.Items.Count > 0 ? 0 : -1);
				}
				comboBox.SelectedIndex = num;
			}
			else
			{
				int num2 = this.comboBox2.Items.IndexOf(str);
				if (num2 >= 0)
				{
					this.comboBox2.SelectedIndex = num2;
					return;
				}
			}
		}

		private void LoadConfig(string configFile)
		{
			if (configFile == null)
			{
				return;
			}
			dynamic jSON = Program.GetJSON(configFile);
			if (jSON == (dynamic)null)
			{
				return;
			}
			string str = (string)jSON.gui.workingDirectory;
			if (Directory.Exists(str))
			{
				this.Bot.Dispose();
				this.Bot = new NMPB.Bot(str);
				Commands.ApplyCommands(this.Bot);
				this.Bot.AvalibleCommandsType = AvalibleCommandsType.All;
			}
			if (jSON.bot != (dynamic)null)
			{
				foreach (dynamic obj in (IEnumerable)jSON.bot)
				{
					MemberInfo memberInfo = typeof(NMPB.Bot).GetMember((string)obj.Name).FirstOrDefault<MemberInfo>();
					if (memberInfo == null)
					{
						continue;
					}
					if (memberInfo.MemberType == MemberTypes.Field)
					{
						FieldInfo fieldInfo = (FieldInfo)memberInfo;
						fieldInfo.SetValue(this.Bot, typeof(Convert).ChangeType(obj.Value, fieldInfo.FieldType));
					}
					if (memberInfo.MemberType != MemberTypes.Property)
					{
						continue;
					}
					PropertyInfo propertyInfo = (PropertyInfo)memberInfo;
					propertyInfo.SetValue(this.Bot, typeof(Convert).ChangeType(obj.Value, propertyInfo.PropertyType), (dynamic)null);
				}
			}
			if (jSON.commandList != (dynamic)null)
			{
				this.Bot.AvalibleCommandsSet = (Dictionary<string, HashSet<string>>)typeof(JsonConvert).DeserializeObject<Dictionary<string, HashSet<string>>>(jSON.commandList.ToString());
				List<string> list = this.Bot.AvalibleCommandsSet.Keys.ToList<string>();
				for (int i = 0; i < list.Count; i++)
				{
					this.Bot.AvalibleCommandsSet[list[i]] = new HashSet<string>(
						from s in this.Bot.AvalibleCommandsSet[list[i]]
						select s.ToLower());
				}
			}
			if (jSON.gui != (dynamic)null)
			{
				if (jSON.gui.room != (dynamic)null)
				{
					this.room = (string)jSON.gui.room;
				}
				if (jSON.gui.language != (dynamic)null)
				{
					int num = this.comboBox2.Items.IndexOf((string)jSON.gui.language);
					if (num >= 0)
					{
						this.comboBox2.SelectedIndex = num;
					}
				}
				if (jSON.gui.commands != (dynamic)null)
				{
					int num1 = this.comboBox1.Items.IndexOf((string)jSON.gui.commands);
					if (num1 >= 0)
					{
						this.comboBox1.SelectedIndex = num1;
					}
				}
				if (jSON.gui.autoConnect == true)
				{
					this.bConnect_Click(null, null);
				}
				if (jSON.gui.minimized == true)
				{
					DelayedThread delayedThread = new DelayedThread(() => base.Invoke(new Action(() => this.bToTray_Click(null, null))), 2000);
				}
				if (jSON.gui.alwaysOnTop == true)
				{
					this.topMostCheckBox.Checked = true;
				}
				if (jSON.gui.receiveChat != (dynamic)null)
				{
					this.cReceiveChat.Checked = (bool)jSON.gui.receiveChat;
				}
				if (jSON.gui.scrollChat != (dynamic)null)
				{
					this.cbScroll.Checked = (bool)jSON.gui.scrollChat;
				}
				try
				{
					Point point = (Point)jSON.gui.windowPos;
					if (point.X >= 0 && point.Y >= 0)
					{
						base.Location = point;
					}
				}
				catch (Exception exception)
				{
				}
				try
				{
					System.Drawing.Size size = (System.Drawing.Size)jSON.gui.windowSize;
					if (size.Width >= 0 && size.Height >= 0)
					{
						base.Size = size;
					}
				}
				catch (Exception exception1)
				{
				}
			}
		}

		private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
		{
			base.Show();
			this.notifyIcon1.Visible = false;
		}

		private void numericUpDown1_ValueChanged_1(object sender, EventArgs e)
		{
			this.richTextBox1.Font = new System.Drawing.Font(this.richTextBox1.Font.Name, (float)((float)this.fontSizeUpDown1.Value), this.richTextBox1.Font.Style, this.richTextBox1.Font.Unit);
		}

		private void OnChatMessage(object sender, TextMessageEventArgs e)
		{
			if (!this.loaded)
			{
				return;
			}
			base.BeginInvoke(new Action(() => this.ParseChat(e.Message)));
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			this.Bot.Dispose();
			Thread.Sleep(200);
			base.OnClosing(e);
		}

		private void ParseChat(string message)
		{
			if (this.ParseSystemInfo(message))
			{
				return;
			}
			string[] strArrays = message.Split(new char[] { '|' }, 4);
			if ((int)strArrays.Length < 4)
			{
				this.richTextBox1.AppendText(message);
				this.richTextBox1.AppendText(Environment.NewLine);
				return;
			}
			string str = strArrays[0];
			string str1 = strArrays[2];
			string str2 = strArrays[3].Trim();
			Color black = Color.Black;
			try
			{
				black = ColorTranslator.FromHtml(str1);
			}
			catch (Exception exception)
			{
			}
			string str3 = str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault<string>();
			this.AppendColoredText(this.richTextBox1, string.Format("{0} | {1}", str3, str2), black);
			this.richTextBox1.AppendText(Environment.NewLine);
		}

		private bool ParseSystemInfo(string message)
		{
			Match match = Regex.Match(message, "\\d{4}\\.\\d\\d\\.\\d\\d (\\d\\d:\\d\\d:\\d\\d) \\| (SYSTEM INFO: (.*))");
			if (!match.Success)
			{
				return false;
			}
			string value = match.Groups[1].Value;
			string str = match.Groups[2].Value;
			match = Regex.Match(match.Groups[3].Value, "((?:User entred\\.|User left\\.) Name: .*), Color: (#[0-9a-f]{6}), Auid: [0-9a-f]+\\.$");
			if (!match.Success)
			{
				this.richTextBox1.AppendText(string.Format("{0} | {1}", value, str));
				this.richTextBox1.AppendText(Environment.NewLine);
				return true;
			}
			Color black = Color.Black;
			try
			{
				black = ColorTranslator.FromHtml(match.Groups[2].Value);
			}
			catch (Exception exception)
			{
			}
			this.AppendColoredText(this.richTextBox1, string.Format("{0} | SYSTEM INFO: {1}", value, match.Groups[1].Value), black);
			this.richTextBox1.AppendText(Environment.NewLine);
			return true;
		}

		private void ResaveLocalization(string file)
		{
			Localization.Load(file).Save(file);
			base.Invoke(new Action(() => this.richTextBox1.AppendText(string.Concat("Localization resaved.", Environment.NewLine))));
		}

		private void richTextBox1_KeyPress(object sender, KeyPressEventArgs e)
		{
			e.Handled = true;
		}

		private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
		{
			Process.Start(e.LinkText);
		}

		private void richTextBox1_TextChanged(object sender, EventArgs e)
		{
			if (!this.cbScroll.Checked)
			{
				return;
			}
			this.richTextBox1.SelectionStart = this.richTextBox1.Text.Length;
			this.richTextBox1.ScrollToCaret();
		}

		public void SaveConfig()
		{
			this.CreateFileIfNotExists(Path.Combine(this.Bot.RootDirectory, "config.txt"));
			using (TextWriter streamWriter = new StreamWriter(Path.Combine(this.Bot.RootDirectory, "config.txt"), false))
			{
				streamWriter.WriteLine(this.comboBox2.SelectedItem);
				streamWriter.Close();
			}
		}

		public void SaveDefaultConfig(string filename)
		{
			Dictionary<string, object> dictionary = typeof(NMPB.Bot).GetFields(BindingFlags.Instance | BindingFlags.Public).Where<FieldInfo>((FieldInfo info) => {
				if (info.IsInitOnly)
				{
					return false;
				}
				return this._simpleTypes.Contains<Type>(info.FieldType);
			}).ToDictionary<FieldInfo, string, object>((FieldInfo fieldInfo) => fieldInfo.Name, (FieldInfo fieldInfo) => fieldInfo.GetValue(this.Bot));
			foreach (PropertyInfo propertyInfo in typeof(NMPB.Bot).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where<PropertyInfo>((PropertyInfo info) => {
				if (!info.CanWrite)
				{
					return false;
				}
				return this._simpleTypes.Contains<Type>(info.PropertyType);
			}))
			{
				dictionary.Add(propertyInfo.Name, propertyInfo.GetValue(this.Bot, null));
			}
			Dictionary<string, object> strs = new Dictionary<string, object>()
			{
				{ "workingDirectory", "" },
				{ "room", this.room },
				{ "language", "Default" },
				{ "commands", "All" },
				{ "autoRestart", false },
				{ "autoConnect", false },
				{ "minimized", false },
				{ "alwaysOnTop", false },
				{ "receiveChat", true },
				{ "scrollChat", true },
				{ "windowPos", new Point(-1, -1) },
				{ "windowSize", new System.Drawing.Size(-1, -1) }
			};
			File.WriteAllText(Path.Combine(this.Bot.RootDirectory, filename), JsonConvert.SerializeObject(new { gui = strs, bot = dictionary, commandList = this.Bot.AvalibleCommandsSet }, 1));
		}

		private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == '\r')
			{
				this.bConnect_Click(sender, null);
				e.Handled = true;
			}
		}

		private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == '\r')
			{
				this.richTextBox1.AppendText(string.Concat(this.textBox3.Text, Environment.NewLine));
				lock (this.Bot.Client.Users)
				{
					this.Bot.ProcessChat(this.textBox3.Text, "Console", "Console", this.Bot.BotAuid, true);
				}
				this.textBox3.Text = "";
				e.Handled = true;
			}
		}

		private void topMostCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			base.TopMost = this.topMostCheckBox.Checked;
		}
	}
}