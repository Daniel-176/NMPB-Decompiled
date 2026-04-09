using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;

namespace NMPB.Client
{
	public class Player : NMPB.Client.Client
	{
		public NMPB.Client.NoteQuota NoteQuota;

		public double PosX;

		public double PosY;

		private System.Timers.Timer _sendMouseTimer;

		public Player.CursorDelegate Cursor;

		public bool ObtainCrown;

		public QuotaLimitations QuotaLimitation;

		private double _lastX;

		private double _lastY;

		public bool ConnectedToRoom
		{
			get;
			private set;
		}

		public Player(Uri uri = null, string useragent = null) : base(uri, useragent)
		{
			this.BotUserUpdated = (object argument0, UserBaseEventArgs argument1) => {
			};
			this.NoteBufferReceived = (object argument2, UserNoteBufferEventArgs argument3) => {
			};
			this.ChatReceived = (object argument4, ChatMessageEventArgs argument5) => {
			};
			this.Cursor = (ref double argument6, ref double argument7) => {
			};
			this.QuotaLimitation = QuotaLimitations.Easy;
			this.InitClient();
		}

		public Player(string room, ChannelSettings settings, string useragent = null) : base(null, useragent)
		{
			this.BotUserUpdated = (object argument0, UserBaseEventArgs argument1) => {
			};
			this.NoteBufferReceived = (object argument2, UserNoteBufferEventArgs argument3) => {
			};
			this.ChatReceived = (object argument4, ChatMessageEventArgs argument5) => {
			};
			this.Cursor = (ref double argument6, ref double argument7) => {
			};
			this.QuotaLimitation = QuotaLimitations.Easy;
			this.InitClient();
			base.SetChannel(Uri.UnescapeDataString(room), settings ?? new ChannelSettings(false, true, true, false, null));
		}

		private static int Clamp(int val, int a, int b)
		{
			if (val < a)
			{
				return a;
			}
			if (val <= b)
			{
				return val;
			}
			return b;
		}

		public new void Dispose()
		{
			base.Dispose();
			this._sendMouseTimer.Dispose();
		}

		private void GetCrown(object sender, ChannelEventArgs args)
		{
			if (!this.ObtainCrown)
			{
				return;
			}
			if (args == null || args.Channel == null || args.Channel.Crown == null)
			{
				return;
			}
			if (this.ParticipantId == null || args.Channel.Crown.ParticipantId == this.ParticipantId)
			{
				return;
			}
			this.SendObject(new { m = "chown", id = this.ParticipantId });
			int time = (int)(args.Channel.Crown.Time + (long)15000 - base.GetSTime());
			if (time > 0)
			{
				DelayedTask delayedTask = new DelayedTask(() => {
					this.SendObject(new { m = "chown", id = this.ParticipantId });
					base.SetChannelSettings(this.DesiredChannelSettings);
				}, time);
			}
		}

		private void InitClient()
		{
			this.NoteQuota = new NMPB.Client.NoteQuota(true);
			base.OnDynamic("a", new Action<object>(this.ReceiveChat));
			base.OnDynamic("n", new Action<object>(this.ReceiveNoteBuffer));
			base.OnDynamic("nq", (object msg) => this.NoteQuota.SetParams(msg));
			base.OnDynamic("hi", (object msg) => this.ConnectedToRoom = false);
			base.Disconnected += new EventHandler((object sender, EventArgs args) => this.ConnectedToRoom = false);
			base.UserUpdated += new EventHandler<UserBaseEventArgs>(this.UpdateBotUser);
			base.UserEntered += new EventHandler<UserBaseEventArgs>(this.UpdateBotUser);
			base.ChannelUpdated += new EventHandler<ChannelEventArgs>(this.GetCrown);
			base.ChannelUpdated += new EventHandler<ChannelEventArgs>((object sender, ChannelEventArgs args) => this.ConnectedToRoom = true);
			this._sendMouseTimer = new System.Timers.Timer()
			{
				Interval = 50
			};
			this._sendMouseTimer.Elapsed += new ElapsedEventHandler(this.SendMouse);
			this._sendMouseTimer.Start();
		}

		public void PlayNote(int code, int volume = 64, long? time = null)
		{
			if (!this.SpendQuota())
			{
				return;
			}
			base.StartNote(NoteConverter.Notes[Player.Clamp(code, 0, 127)], (double)volume / 128, time);
		}

		public static string PrepareRoomName(string name)
		{
			name = Uri.UnescapeDataString(name);
			Match match = (new Regex("multiplayerpiano.com\\/(.+)", RegexOptions.IgnoreCase)).Match(name);
			if (match.Success)
			{
				name = match.Groups[1].Value;
			}
			return name;
		}

		private void ReceiveChat(dynamic msg)
		{
			if (msg.p == null)
			{
				return;
			}
			string str = (string)msg.a ?? "";
			string str1 = (string)msg.p.name ?? "";
			string str2 = (string)msg.p.color ?? "#ffffff";
			string str3 = (string)msg.p._id ?? "no";
			this.ChatReceived(this, new ChatMessageEventArgs(str1, str, str2, str3));
		}

		private void ReceiveNoteBuffer(dynamic msg)
		{
			string participantId = (string)msg.p;
			UserBase user = this.FindParticipantById(participantId);
			if (user == null)
			{
				return;
			}
			long time = (long)((msg.t != null) ? msg.t : 0);
			List<Note> notes = new List<Note>();
			if (msg.n != null)
			{
				foreach (dynamic noteObj in msg.n)
				{
					Note note = new Note
					{
						Value = (string)noteObj.n ?? "a0",
						Velocity = (noteObj.v != null) ? (double)noteObj.v : 0.5,
						Delay = (noteObj.d != null) ? (long)noteObj.d : 0,
						Stop = (noteObj.s != null) ? (int)noteObj.s : 0
					};
					notes.Add(note);
				}
			}
			this.NoteBufferReceived(this, new UserNoteBufferEventArgs(user, time, notes));
		}

		public void Say(string message)
		{
			this.SendObject(new { m = "a", message = message });
		}

		private void SendMouse(object sender, EventArgs args)
		{
			this.Cursor(ref this.PosX, ref this.PosY);
			if (Math.Abs(this._lastX - this.PosX) < 0.01 && Math.Abs(this._lastY - this.PosY) < 0.01)
			{
				return;
			}
			this._lastX = this.PosX;
			this._lastY = this.PosY;
			this.SendObject(new { m = "m", x = this.PosX.ToString("N2", CultureInfo.InvariantCulture), y = this.PosY.ToString("N2", CultureInfo.InvariantCulture) });
		}

		public void SendObject(object content)
		{
			base.SendArray(new JArray(JObject.FromObject(content)));
		}

		private bool SpendQuota()
		{
			if (this.QuotaLimitation == QuotaLimitations.Easy && !this.NoteQuota.SafeSpend(1) || this.QuotaLimitation == QuotaLimitations.Aggressive && !this.NoteQuota.Spend(1))
			{
				return false;
			}
			if (this.QuotaLimitation == QuotaLimitations.None)
			{
				this.NoteQuota.SafeSpend(1);
			}
			return true;
		}

		public void StopNote(int code)
		{
			if (!this.SpendQuota())
			{
				return;
			}
			base.StopNote(NoteConverter.Notes[Player.Clamp(code, 0, 127)]);
		}

		private void UpdateBotUser(object sender, UserBaseEventArgs e)
		{
			if (e.User.Auid != this.BotUser.Auid)
			{
				return;
			}
			this.BotUser = e.User;
			this.BotUserUpdated(this, new UserBaseEventArgs(this.BotUser));
		}

		public event EventHandler<UserBaseEventArgs> BotUserUpdated;

		public event EventHandler<ChatMessageEventArgs> ChatReceived;

		public event EventHandler<UserNoteBufferEventArgs> NoteBufferReceived;

		public delegate void CursorDelegate(ref double x, ref double y);
	}
}