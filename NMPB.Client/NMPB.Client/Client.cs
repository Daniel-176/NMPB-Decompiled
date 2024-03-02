using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NMPB.Client.Properties;
using SuperSocket.ClientEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using WebSocket4Net;

namespace NMPB.Client
{
	public class Client : EventEmitter, IDisposable
	{
		private WebSocket _ws;

		private readonly Uri _uri;

		public ChannelInfo Channel;

		protected ChannelSettings DesiredChannelSettings;

		private string _desiredChannelId;

		public readonly List<UserBase> Users;

		public UserBase BotUser;

		private readonly System.Timers.Timer _noteFlushInterval;

		private readonly System.Timers.Timer _pingInterval;

		public string ParticipantId;

		private DateTime? _connectionTime;

		private int _connectionAttempts;

		private JArray _noteBuffer;

		private long _noteBufferTime;

		public long ServerTimeOffset;

		private bool _canConnect;

		private const int AllowedMessageLength = 8192;

		private readonly string _useragent;

		private bool _channelReceived;

		private readonly DateTime _st = new DateTime(1970, 1, 1);

		private bool _disposed;

		public Client(Uri uri = null, string useragent = null)
		{
			this.Users = new List<UserBase>();
			this.BotUser = new UserBase("No", "No", "Anonymous", "#ffff00");
			this._uri = uri ?? new Uri((string.IsNullOrWhiteSpace(Settings.Default.ServerUrl) ? "ws://www.multiplayerpiano.com:443" : Settings.Default.ServerUrl));
			this._useragent = useragent ?? "NMPB.Client";
			this.ReinitWs();
			this.BindEventListeners();
			this._pingInterval = new System.Timers.Timer()
			{
				Interval = 20000
			};
			this._pingInterval.Elapsed += new ElapsedEventHandler((object o, ElapsedEventArgs eventArgs) => this.Send(string.Format("[{{\"m\": \"t\", \"e\":\"{0}\"}}]", this.GetTime())));
			this._noteFlushInterval = new System.Timers.Timer()
			{
				Interval = 200
			};
			this._noteFlushInterval.Elapsed += new ElapsedEventHandler(this.OnNoteFlushIntervalElapsed);
		}

		private void BindEventListeners()
		{
			this.OnDynamic("hi", (object msg) => {
				this._channelReceived = false;
				this.BotUser = (UserBase)msg.u.ToObject<UserBase>();
				this.Connected(this, new ConnectedEventArgs((string)msg.v ?? "", (string)msg.motd ?? "", this.BotUser));
				if (msg.t != (dynamic)null)
				{
					this.ReceiveServerTime((long)msg.t);
				}
				if (this._desiredChannelId != null)
				{
					this.SetChannelSure();
				}
			});
			this.OnDynamic("t", (object msg) => {
				if (msg.t != (dynamic)null)
				{
					this.ReceiveServerTime((long)msg.t);
				}
			});
			this.OnDynamic("ch", (object msg) => {
				dynamic obj = msg.ch == (dynamic)null;
				if (!obj)
				{
					if ((obj | !(msg.ch is JObject)) == 0)
					{
						this._channelReceived = true;
						this._desiredChannelId = (string)msg.ch._id;
						this.Channel = (ChannelInfo)msg.ch.ToObject<ChannelInfo>();
						if (msg.p != (dynamic)null)
						{
							this.ParticipantId = (string)msg.p;
						}
						if (this.DesiredChannelSettings != null && this.IsOwner() && !this.DesiredChannelSettings.Equals(this.Channel.Settings))
						{
							this._channelReceived = false;
							this.SetChannelSure();
						}
						dynamic obj1 = msg.ppl.ToObject<List<UserBase>>();
						this.SetParticipants(msg.ppl);
						this.ChannelUpdated(this, new ChannelEventArgs(this.Channel, this.ParticipantId, obj1, this.Users));
						return;
					}
				}
			});
			this.OnDynamic("p", (object msg) => this.ParticipantUpdate(msg));
			this.OnDynamic("m", (object msg) => {
				string str = (string)msg.id;
				if (this.Users.Any<UserBase>((UserBase user) => user.Id == str))
				{
					this.ParticipantUpdate(msg);
				}
			});
			this.OnDynamic("bye", (object msg) => this.RemoveParticipant((string)msg.p));
		}

		private void Connect()
		{
			if (this._disposed || !this._canConnect || this.IsConnected())
			{
				return;
			}
			this.ReinitWs();
			lock (this._ws)
			{
				this._ws.Open();
			}
		}

		private int CountParticipants()
		{
			return this.Users.Count;
		}

		public void Dispose()
		{
			this._disposed = true;
			this.Stop();
			this._pingInterval.Dispose();
			this._noteFlushInterval.Dispose();
		}

		public UserBase FindParticipantById(string id)
		{
			return this.Users.FirstOrDefault<UserBase>((UserBase user) => user.Id == id);
		}

		private ChannelSettings GetChannelSetting()
		{
			if (this.Channel != null && this.Channel.Settings != null)
			{
				return this.Channel.Settings;
			}
			return (ChannelSettings)((dynamic)this.OfflineChannelSettings());
		}

		public UserBase GetOwnParticipant()
		{
			return this.FindParticipantById(this.ParticipantId);
		}

		public long GetSTime()
		{
			return this.GetTime() + this.ServerTimeOffset;
		}

		public long GetTime()
		{
			TimeSpan universalTime = DateTime.Now.ToUniversalTime() - this._st;
			return (long)(universalTime.TotalMilliseconds + 0.5);
		}

		public bool IsConnected()
		{
			if (this._disposed || this._ws == null)
			{
				return false;
			}
			return this._ws.State == WebSocketState.Open;
		}

		public bool IsConnecting()
		{
			if (this._ws == null)
			{
				return false;
			}
			return this._ws.State == WebSocketState.Connecting;
		}

		public bool IsOwner()
		{
			if (this.Channel == null || this.Channel.Crown == null)
			{
				return false;
			}
			return this.Channel.Crown.ParticipantId == this.ParticipantId;
		}

		[Conditional("DEBUG")]
		private void LogData(string msg)
		{
			this.DataDebug(this, new DebugMessageEventArgs(Regex.Replace(msg, "\\s+", " ")));
		}

		private dynamic OfflineChannelSettings()
		{
			return JObject.FromObject(new { lobby = true, visible = false, chat = false, crownsolo = false });
		}

		public UserBase OfflineParticipant()
		{
			return new UserBase("", "", "", "#777");
		}

		public void OnDynamic(string evnt, Action<dynamic> callback)
		{
			base.On(evnt, (object[] objects) => {
				if (NMPB.Client.Client.<>o__91.<>p__0 == null)
				{
					NMPB.Client.Client.<>o__91.<>p__0 = CallSite<Action<CallSite, Action<object>, object>>.Create(Binder.Invoke(CSharpBinderFlags.ResultDiscarded, typeof(NMPB.Client.Client), (IEnumerable<CSharpArgumentInfo>)(new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) })));
				}
				NMPB.Client.Client.<>o__91.<>p__0.Target(NMPB.Client.Client.<>o__91.<>p__0, callback, objects[0]);
			});
		}

		private void OnNoteFlushIntervalElapsed(object o, ElapsedEventArgs eventArgs)
		{
			if (this._disposed)
			{
				return;
			}
			lock (this._noteBuffer)
			{
				if (this._noteBufferTime > (long)0 && this._noteBuffer.Count > 0)
				{
					this.SplitAndSendNotes(this._noteBuffer);
					this._noteBufferTime = (long)0;
					this._noteBuffer.Clear();
				}
			}
		}

		private void OnWsClosed(object sender, EventArgs e)
		{
			lock (this._ws)
			{
				if (!this._disposed)
				{
					this._pingInterval.Stop();
					this._noteFlushInterval.Stop();
					this.Disconnected(this, new EventArgs());
					if (this._canConnect)
					{
						if (!this._connectionTime.HasValue)
						{
							this._connectionAttempts++;
						}
						else
						{
							this._connectionTime = null;
							this._connectionAttempts = 0;
						}
						int[] numArray = new int[] { 50, 2950, 7000, 10000 };
						int length = this._connectionAttempts;
						if (length >= (int)numArray.Length)
						{
							length = (int)numArray.Length - 1;
						}
						int num = numArray[length];
						DelayedTask delayedTask = new DelayedTask(new Action(this.Connect), num);
					}
				}
			}
		}

		private void OnWsError(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
		{
			this.ConnectionError(this, new NMPB.Client.ErrorEventArgs(e.Exception));
		}

		private void OnWsMessageReceived(object sender, MessageReceivedEventArgs args)
		{
			foreach (dynamic obj in (IEnumerable)JArray.Parse(args.Message))
			{
				dynamic obj1 = obj != (dynamic)null;
				if ((!obj1 ? obj1 == null : (obj1 & obj.m != (dynamic)null) == 0))
				{
					continue;
				}
				lock (this.Users)
				{
					this.Emit((string)obj.m, obj);
				}
			}
		}

		private void OnWsOpened(object sender, EventArgs e)
		{
			this._connectionTime = new DateTime?(DateTime.Now);
			this.Send("[{\"m\": \"hi\"}]");
			this._pingInterval.Start();
			this._noteBuffer = new JArray();
			this._noteBufferTime = (long)0;
			this._noteFlushInterval.Start();
		}

		private void ParticipantUpdate(dynamic update)
		{
			if (update.id == (dynamic)null)
			{
				return;
			}
			lock (this.Users)
			{
				UserBase userBase = this.Users.FirstOrDefault<UserBase>((UserBase user) => {
					string id = user.Id;
					if (NMPB.Client.Client.<>o__81.<>p__4 == null)
					{
						NMPB.Client.Client.<>o__81.<>p__4 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.ConvertExplicit, typeof(string), typeof(NMPB.Client.Client)));
					}
					!0 target = NMPB.Client.Client.<>o__81.<>p__4.Target;
					CallSite<Func<CallSite, object, string>> u003cu003ep_4 = NMPB.Client.Client.<>o__81.<>p__4;
					if (NMPB.Client.Client.<>o__81.<>p__3 == null)
					{
						NMPB.Client.Client.<>o__81.<>p__3 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "id", typeof(NMPB.Client.Client), (IEnumerable<CSharpArgumentInfo>)(new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) })));
					}
					return id == target(u003cu003ep_4, NMPB.Client.Client.<>o__81.<>p__3.Target(NMPB.Client.Client.<>o__81.<>p__3, update));
				});
				if (userBase != null)
				{
					if (update.x != (dynamic)null)
					{
						userBase.X = (double)((double)update.x);
					}
					if (update.y != (dynamic)null)
					{
						userBase.Y = (double)((double)update.y);
					}
					dynamic obj = update.x != (dynamic)null;
					if (!obj)
					{
						if ((obj | update.y != (dynamic)null) == 0)
						{
							goto Label1;
						}
					}
					this.UserMouseMoved(this, new UserBaseEventArgs(userBase));
					if (update.name != (dynamic)null)
					{
						userBase.Name = (string)update.name;
						this.UserNameReceived(this, new UserBaseEventArgs(userBase));
					}
					if (update.color != (dynamic)null)
					{
						userBase.Color = (string)update.color;
						this.UserColorReceived(this, new UserBaseEventArgs(userBase));
					}
				}
				else
				{
					UserBase userBase1 = (UserBase)update.ToObject<UserBase>();
					if (userBase1 != null)
					{
						if (update.x != (dynamic)null)
						{
							userBase1.X = (double)((double)update.x);
						}
						if (update.y != (dynamic)null)
						{
							userBase1.Y = (double)((double)update.y);
						}
						this.Users.Add(userBase1);
						this.UserEntered(this, new UserBaseEventArgs(userBase1));
						userBase = userBase1;
					}
					else
					{
						return;
					}
				}
				this.UserUpdated(this, new UserBaseEventArgs(userBase));
			}
		}

		public bool PreventsPlaying()
		{
			if (!this.IsConnected() || this.IsOwner())
			{
				return false;
			}
			return this.GetChannelSetting().Crownsolo;
		}

		private void ReceiveServerTime(long t)
		{
			this.ServerTimeOffset = t - this.GetTime();
		}

		private void ReinitWs()
		{
			this._ws = new WebSocket(this._uri.ToString(), "", null, null, this._useragent, this._uri.Host, WebSocketVersion.None, null, SslProtocols.None, 0)
			{
				ReceiveBufferSize = 8192
			};
			this._ws.Closed += new EventHandler(this.OnWsClosed);
			this._ws.Opened += new EventHandler(this.OnWsOpened);
			this._ws.MessageReceived += new EventHandler<MessageReceivedEventArgs>(this.OnWsMessageReceived);
			this._ws.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs>(this.OnWsError);
			this._ws.EnableAutoSendPing = false;
			this._ws.NoDelay = true;
		}

		private void RemoveParticipant(string id)
		{
			UserBase userBase;
			lock (this.Users)
			{
				userBase = this.Users.FirstOrDefault<UserBase>((UserBase user) => user.Id == id);
				if (userBase != null)
				{
					this.Users.Remove(userBase);
				}
				else
				{
					return;
				}
			}
			this.UserLeft(this, new UserBaseEventArgs(userBase));
		}

		private void Send(string raw)
		{
			lock (this._ws)
			{
				if (this.IsConnected())
				{
					this._ws.Send(raw);
				}
			}
		}

		public void SendArray(JArray jArray)
		{
			this.Send(JsonConvert.SerializeObject(jArray));
		}

		[Conditional("DEBUG")]
		private void SendDebug(string msg)
		{
			this.TextDebug(this, new DebugMessageEventArgs(msg));
		}

		public void SetChannel(string id = null, ChannelSettings set = null)
		{
			this._desiredChannelId = id ?? this._desiredChannelId;
			this.DesiredChannelSettings = set ?? this.DesiredChannelSettings;
			dynamic obj = JObject.FromObject(new { m = "ch", _id = this._desiredChannelId });
			dynamic obj1 = obj;
			object desiredChannelSettings = this.DesiredChannelSettings;
			if (desiredChannelSettings == null)
			{
				desiredChannelSettings = new object();
			}
			obj1.set = JObject.FromObject(desiredChannelSettings);
			this.SendArray(new JArray(obj));
		}

		public void SetChannelSettings(ChannelSettings set)
		{
			this.DesiredChannelSettings = set ?? this.DesiredChannelSettings;
			this.SendArray(new JArray(JObject.FromObject(new { m = "chset", @set = set })));
		}

		private void SetChannelSure()
		{
			this.SetChannel(null, null);
			DelayedTask delayedTask = new DelayedTask(() => {
				if (!this._channelReceived)
				{
					this.SetChannelSure();
				}
			}, 2000);
		}

		private void SetParticipants(IEnumerable<JToken> newParticipants)
		{
			if (newParticipants == null)
			{
				return;
			}
			lock (this.Users)
			{
				List<UserBase> list = (
					from user1 in this.Users
					where newParticipants.All<JToken>((JToken user2) => user1.Id != (string)user2["id"])
					select user1).ToList<UserBase>();
				for (int i = 0; i < list.Count; i++)
				{
					this.RemoveParticipant(list[i].Id);
				}
				foreach (JToken newParticipant in newParticipants)
				{
					this.ParticipantUpdate(newParticipant);
				}
			}
		}

		private void SplitAndSendNotes(JArray array)
		{
			if (array == null)
			{
				return;
			}
			string str = JsonConvert.SerializeObject(new JArray(JObject.FromObject(new { m = "n", t = this._noteBufferTime + this.ServerTimeOffset, n = array })));
			if (str.Length < 8192)
			{
				this.Send(str);
				return;
			}
			int count = array.Count;
			this.SplitAndSendNotes(JArray.FromObject(array.Take<JToken>(count / 2)));
			this.SplitAndSendNotes(JArray.FromObject(array.Skip<JToken>(count / 2)));
		}

		public void Start()
		{
			this._canConnect = true;
			this.Connect();
		}

		public void StartNote(string note, double vel = 0.5, long? startTime = null)
		{
			if (!this.IsConnected())
			{
				return;
			}
			long? nullable = startTime;
			long num = (nullable.HasValue ? nullable.GetValueOrDefault() : this.GetTime());
			string str = vel.ToString("N3", CultureInfo.InvariantCulture);
			dynamic obj = JObject.FromObject(new { n = note, v = str });
			lock (this._noteBuffer)
			{
				if (this._noteBufferTime > (long)0)
				{
					obj.d = num - this._noteBufferTime;
				}
				else
				{
					this._noteBufferTime = num;
				}
				this._noteBuffer.Add(obj);
			}
		}

		public void Stop()
		{
			this._canConnect = false;
			lock (this._ws)
			{
				if (this.IsConnected())
				{
					this._ws.Close();
				}
			}
		}

        public void StopNote(string note)
        {
            if (!this.IsConnected())
            {
                return;
            }
            dynamic obj = JObject.FromObject(new { n = note, s = 1 });
            lock (this._noteBuffer)
            {
                if (this._noteBufferTime > (long)0)
                {
                    obj.d = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - this._noteBufferTime;
                }
                else
                {
                    this._noteBufferTime = num;
                }
                this._noteBuffer.Add(obj);
            }
        }

		public event EventHandler<ChannelEventArgs> ChannelUpdated;

		public event EventHandler<ConnectedEventArgs> Connected;

		public event EventHandler<NMPB.Client.ErrorEventArgs> ConnectionError;

		public event EventHandler<DebugMessageEventArgs> DataDebug;

		public event EventHandler Disconnected;

		public event EventHandler<DebugMessageEventArgs> TextDebug;

		public event EventHandler<UserBaseEventArgs> UserColorReceived;

		public event EventHandler<UserBaseEventArgs> UserEntered;

		public event EventHandler<UserBaseEventArgs> UserLeft;

		public event EventHandler<UserBaseEventArgs> UserMouseMoved;

		public event EventHandler<UserBaseEventArgs> UserNameReceived;

		public event EventHandler<UserBaseEventArgs> UserUpdated;
	}
}