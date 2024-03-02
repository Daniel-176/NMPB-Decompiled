using NMPB.Client;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace NMPB.RemoteControl
{
	public class Form1 : Form
	{
		private Player Client;

		private RSAParameters RSAKeys;

		public readonly string RootDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

		private IContainer components;

		private Label label1;

		private Label label2;

		private TextBox tRoom;

		private TextBox tCommand;

		private Button bConnect;

		private Button bSend;

		public Form1()
		{
			this.InitializeComponent();
		}

		private void bConnect_Click(object sender, EventArgs e)
		{
			this.Connect();
		}

		private void bSend_Click(object sender, EventArgs e)
		{
			this.Send();
		}

		private void Connect()
		{
			this.tRoom.Text = Player.PrepareRoomName(this.tRoom.Text);
			this.Client.SetChannel(this.tRoom.Text, null);
			this.Client.Start();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		public static string Encrypt(string message, string signature)
		{
			byte[] bytes = (new UTF8Encoding()).GetBytes(message);
			byte[] numArray = Convert.FromBase64String(signature);
			for (int i = 0; i < (int)bytes.Length; i++)
			{
				ref byte numPointer = ref bytes[i];
				numPointer = (byte)(numPointer ^ numArray[i % (int)numArray.Length]);
			}
			return Convert.ToBase64String(bytes);
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			this.Client = new Player(null, "NMPB.RemoteControl");
			using (RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider())
			{
				try
				{
					try
					{
						rSACryptoServiceProvider.FromXmlString(File.ReadAllText(this.GetPath("PrivateKeys.xml")));
						this.RSAKeys = rSACryptoServiceProvider.ExportParameters(true);
						if (!File.Exists(this.GetPath("AdminKeys.xml")))
						{
							File.WriteAllText(this.GetPath("AdminKeys.xml"), rSACryptoServiceProvider.ToXmlString(false));
						}
					}
					catch (Exception exception)
					{
						this.GenerateKeys();
					}
				}
				finally
				{
					rSACryptoServiceProvider.PersistKeyInCsp = false;
				}
			}
		}

		private void GenerateKeys()
		{
			using (RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider(1024))
			{
				try
				{
					try
					{
						File.WriteAllText(this.GetPath("PrivateKeys.xml"), rSACryptoServiceProvider.ToXmlString(true));
						File.WriteAllText(this.GetPath("AdminKeys.xml"), rSACryptoServiceProvider.ToXmlString(false));
						this.RSAKeys = rSACryptoServiceProvider.ExportParameters(true);
					}
					catch (Exception exception)
					{
						MessageBox.Show(exception.Message);
					}
				}
				finally
				{
					rSACryptoServiceProvider.PersistKeyInCsp = false;
				}
			}
		}

		public string GetPath(string path)
		{
			return Path.Combine(this.RootDirectory, path);
		}

		private void InitializeComponent()
		{
			this.label1 = new Label();
			this.label2 = new Label();
			this.tRoom = new TextBox();
			this.tCommand = new TextBox();
			this.bConnect = new Button();
			this.bSend = new Button();
			base.SuspendLayout();
			this.label1.AutoSize = true;
			this.label1.Location = new Point(31, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(38, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Room:";
			this.label2.AutoSize = true;
			this.label2.Location = new Point(12, 35);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(57, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "Command:";
			this.tRoom.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			this.tRoom.Location = new Point(75, 6);
			this.tRoom.Name = "tRoom";
			this.tRoom.Size = new System.Drawing.Size(350, 20);
			this.tRoom.TabIndex = 2;
			this.tRoom.KeyPress += new KeyPressEventHandler(this.tRoom_KeyPress);
			this.tCommand.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			this.tCommand.Location = new Point(75, 32);
			this.tCommand.Name = "tCommand";
			this.tCommand.Size = new System.Drawing.Size(350, 20);
			this.tCommand.TabIndex = 3;
			this.tCommand.KeyPress += new KeyPressEventHandler(this.tCommand_KeyPress);
			this.bConnect.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			this.bConnect.Location = new Point(431, 4);
			this.bConnect.Name = "bConnect";
			this.bConnect.Size = new System.Drawing.Size(75, 23);
			this.bConnect.TabIndex = 4;
			this.bConnect.Text = "Connect";
			this.bConnect.UseVisualStyleBackColor = true;
			this.bConnect.Click += new EventHandler(this.bConnect_Click);
			this.bSend.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			this.bSend.Location = new Point(431, 30);
			this.bSend.Name = "bSend";
			this.bSend.Size = new System.Drawing.Size(75, 23);
			this.bSend.TabIndex = 5;
			this.bSend.Text = "Send";
			this.bSend.UseVisualStyleBackColor = true;
			this.bSend.Click += new EventHandler(this.bSend_Click);
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(520, 60);
			base.Controls.Add(this.bSend);
			base.Controls.Add(this.bConnect);
			base.Controls.Add(this.tCommand);
			base.Controls.Add(this.tRoom);
			base.Controls.Add(this.label2);
			base.Controls.Add(this.label1);
			base.Name = "Form1";
			this.Text = "NMPB.RemoteControl";
			base.Load += new EventHandler(this.Form1_Load);
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		private void Send()
		{
			if (!this.Client.ConnectedToRoom)
			{
				this.Connect();
			}
			string text = this.tCommand.Text;
			Thread thread = new Thread(() => this.SendMessage("/admin", text))
			{
				IsBackground = true
			};
			thread.Start();
			this.tCommand.Text = "";
		}

		private void SendMessage(string opened, string closed)
		{
			int num = 0;
			while (!this.Client.ConnectedToRoom)
			{
				num++;
				Thread.Sleep(100);
				if (num % 25 != 0)
				{
					continue;
				}
				this.Client.SetChannel(null, null);
				this.Client.Start();
			}
			long sTime = this.Client.GetSTime();
			object[] auid = new object[] { sTime, this.Client.BotUser.Auid, "ALL", closed };
			string str = string.Format("{0} {1} {2} {3}", auid);
			string str1 = Form1.SignData(str, this.RSAKeys);
			str = Form1.Encrypt(str, str1);
			this.Client.Say(string.Format("{2} {0} {1}", str, str1, opened));
		}

		public static string SignData(string message, RSAParameters privateKey)
		{
			byte[] numArray;
			using (RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider())
			{
				try
				{
					byte[] bytes = (new UTF8Encoding()).GetBytes(message);
					rSACryptoServiceProvider.ImportParameters(privateKey);
					numArray = rSACryptoServiceProvider.SignData(bytes, CryptoConfig.MapNameToOID("SHA512"));
				}
				finally
				{
					rSACryptoServiceProvider.PersistKeyInCsp = false;
				}
			}
			return Convert.ToBase64String(numArray);
		}

		private void tCommand_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == '\r')
			{
				this.Send();
				e.Handled = true;
			}
		}

		private void tRoom_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == '\r')
			{
				this.Connect();
				e.Handled = true;
			}
		}
	}
}