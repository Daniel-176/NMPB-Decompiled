using CsQuery;
using CsQuery.ExtensionMethods.Internal;
using NMPB.Client;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace NMPB
{
	public class Bot : IDisposable
	{
		public Player Client;

		private bool _closing;

		private bool _loaded;

		private Sequence _sequence;

		private Sequencer _sequencer;

		public bool Trackbar;

		private readonly Random _rnd;

		private BotStatus _status;

		private DateTime _startTime;

		private string _room;

		private string _nowPlayingName;

		private TimeSpan _nowPlayingLength;

		private readonly object _loadingLocker;

		public bool AllowOtherBots;

		public int CustomNoteDelay;

		public bool LogarthmicVelocity;

		private bool _hushNotes;

		private bool _tooManylocker;

		private bool _preserveQuotExceeded;

		public bool Inversed;

		public bool OctaveMode;

		public bool EchoMode;

		public bool OctaveJumpMode;

		private readonly LimitedValue _octaveCount;

		private readonly LimitedValue _echoCount;

		private readonly LimitedValue _echoDelay;

		private readonly LimitedValue _echoFade;

		private readonly LimitedValue _transpose;

		public bool ModifiersAutoReset;

		private readonly long[] _noteTimes;

		private Mistakes _mistakes;

		private Dictionary<string, List<string>> _helpDictionary;

		private Localization _l;

		private NMPB.Sustain _sustain;

		private bool _roomIsVisible;

		private bool _roomHasChat;

		private bool _roomSoloPlay;

		private Color _roomColor;

		private RSAParameters? _adminKeys;

		public bool Sustain;

		public bool PreserveQuota;

		private const int PreventDoublesMinTime = 10;

		public bool SendChat;

		public NMPB.AvalibleCommandsType AvalibleCommandsType;

		public Dictionary<string, HashSet<string>> AvalibleCommandsSet;

		private List<string> _names;

		private List<string> _hashes;

		public string DownloaderProxy;

		public readonly string RootDirectory;

		public readonly string WorkingWirectory;

		private List<string[]> _indirectList;

		private const string NoName = "No Name";

		private const string Deleted = "[DELETED]";

		private readonly string MIDIFolder;

		private readonly CultureInfo EnUs;

		private string _chat;

		private Sequence _dlSequence;

		private const int MaxDownloadTimeDefault = 6000;

		public int MaxDownloadTime;

		private const int MaxRequestTimeoutDefault = 3000;

		public int MaxRequestTimeout;

		private Mutex _mainMutex;

		private Mutex _deleteMutex;

		private FileSystemWatcher _fsWatcher;

		private string[] _imposters;

		private readonly object _downloadLocker;

		private readonly string[] _cirno;

		private readonly string[] _sans;

		private readonly object _chatlocker;

		private const int TurnTimeDefault = 30;

		public int TurnTime;

		private const int LongSongTimeDefault = 180;

		public int LongSongTime;

		private const double CircleRadiusDefault = 5;

		public double CircleRadius;

		private const int MaxUserListSizeDefault = 500;

		public int MaxUserListSize;

		private DateTime _lastHi;

		public List<User> Users;

		public HashSet<string> Banned;

		public User Master;

		public User OldMaster;

		public List<User> Queue;

		public bool WelcomeNewUsers;

		private User _lastGreetedUser;

		private NMPB.TurnState _turnState;

		private bool _turns;

		public bool ShowAllColors;

		private DelayedThread _songDelayedThread;

		private DelayedThread _longSongDelayedThread;

		public const int Min = 8;

		public const int Max = 88;

		private IEnumerable<NamedColor> _customColors;

		public string BotAuid
		{
			get
			{
				return this.Client.BotUser.Auid;
			}
		}

		public string BotColor
		{
			get
			{
				return this.Client.BotUser.Color;
			}
		}

		public string BotName
		{
			get
			{
				return this.Client.BotUser.Name;
			}
		}

		public NMPB.TurnState CurrnetTurnState
		{
			get
			{
				return this._turnState;
			}
		}

		public int EchoCount
		{
			get
			{
				return this._echoCount.Value;
			}
			set
			{
				this._echoCount.Value = value;
			}
		}

		public int EchoDelay
		{
			get
			{
				return this._echoDelay.Value;
			}
			set
			{
				this._echoDelay.Value = value;
			}
		}

		public int EchoFade
		{
			get
			{
				return this._echoFade.Value;
			}
			set
			{
				this._echoFade.Value = value;
			}
		}

		public ReadOnlyCollection<string> Hashes
		{
			get
			{
				return new ReadOnlyCollection<string>(this._hashes);
			}
		}

		private string HashesTxt
		{
			get
			{
				return Path.Combine(this.WorkingWirectory, "hashes.txt");
			}
		}

		private string ImpostersFile
		{
			get
			{
				return Path.Combine(this.WorkingWirectory, "imposters.txt");
			}
		}

		public Localization L
		{
			get
			{
				return this._l;
			}
			set
			{
				this._l = value;
				this.LoadHelp();
				this.LoadColors();
			}
		}

		public ReadOnlyCollection<string> Names
		{
			get
			{
				return new ReadOnlyCollection<string>(this._names);
			}
		}

		private string NamesTxt
		{
			get
			{
				return Path.Combine(this.WorkingWirectory, "names.txt");
			}
		}

		public int NowPlayingIndex
		{
			get;
			private set;
		}

		public TimeSpan NowPlayingLength
		{
			get
			{
				return this._nowPlayingLength;
			}
		}

		public string NowPlayingName
		{
			get
			{
				return this._nowPlayingName;
			}
		}

		public int OctaveCount
		{
			get
			{
				return this._octaveCount.Value;
			}
			set
			{
				this._octaveCount.Value = value;
			}
		}

		public bool Playing
		{
			get
			{
				if (this._sequencer == null)
				{
					return false;
				}
				return this._sequencer.get_Playing();
			}
		}

		public int RemainingNotes
		{
			get
			{
				return this.Client.NoteQuota.get_Points();
			}
		}

		public string Room
		{
			get
			{
				return this._room;
			}
		}

		public Color RoomColor
		{
			get
			{
				return this._roomColor;
			}
			set
			{
				this._roomColor = value;
				bool? nullable = null;
				bool? nullable1 = nullable;
				nullable = null;
				bool? nullable2 = nullable;
				nullable = null;
				this.SetChannel(nullable1, nullable2, nullable, null);
			}
		}

		public bool RoomHasChat
		{
			get
			{
				return this._roomHasChat;
			}
			set
			{
				if (this._roomHasChat != value)
				{
					this._roomHasChat = value;
					bool? nullable = null;
					bool? nullable1 = nullable;
					nullable = null;
					bool? nullable2 = nullable;
					nullable = null;
					this.SetChannel(nullable1, nullable2, nullable, null);
				}
			}
		}

		public bool RoomIsVisible
		{
			get
			{
				return this._roomIsVisible;
			}
			set
			{
				if (this._roomIsVisible != value)
				{
					this._roomIsVisible = value;
					bool? nullable = null;
					bool? nullable1 = nullable;
					nullable = null;
					bool? nullable2 = nullable;
					nullable = null;
					this.SetChannel(nullable1, nullable2, nullable, null);
				}
			}
		}

		public bool RoomSoloPlay
		{
			get
			{
				return this._roomSoloPlay;
			}
			set
			{
				if (this._roomSoloPlay != value)
				{
					this._roomSoloPlay = value;
					bool? nullable = null;
					bool? nullable1 = nullable;
					nullable = null;
					bool? nullable2 = nullable;
					nullable = null;
					this.SetChannel(nullable1, nullable2, nullable, null);
				}
			}
		}

		public int Transpose
		{
			get
			{
				return this._transpose.Value;
			}
			set
			{
				this._transpose.Value = value;
			}
		}

		public bool Turns
		{
			get
			{
				return this._turns;
			}
			set
			{
				if (!this._turns & value)
				{
					this._turns = true;
					this.Say(this.L.TurnsEnabled);
					this.Stop();
					this.RestartTurns();
					return;
				}
				if (!value)
				{
					this._turns = false;
					this.Say(this.L.TurnsDisabled);
					this.StopTurns();
				}
			}
		}

		internal NMPB.TurnState TurnState
		{
			get
			{
				return this._turnState;
			}
			set
			{
				this._turnState = value;
				this.TurnsStateChanged(this, new TurnsEventArgs(this._turnState, this.Master));
			}
		}

		public Bot(string root = "")
		{
			// 
			// Current member / type: System.Void NMPB.Bot::.ctor(System.String)
			// File path: C:\Users\Daniel176\Downloads\NMPB v1.2 bin\NMPB.dll
			// 
			// Product version: 2024.1.131.0
			// Exception in: System.Void .ctor(System.String)
			// 
			// Object reference not set to an instance of an object.
			//    at ..( , Int32 , Statement& , Int32& ) in C:\DeveloperTooling_JD_Agent1\_work\15\s\OpenSource\Cecil.Decompiler\Steps\CodePatterns\ObjectInitialisationPattern.cs:line 78
			//    at ..( , Int32& , Statement& , Int32& ) in C:\DeveloperTooling_JD_Agent1\_work\15\s\OpenSource\Cecil.Decompiler\Steps\CodePatterns\BaseInitialisationPattern.cs:line 33
			//    at ..( ) in C:\DeveloperTooling_JD_Agent1\_work\15\s\OpenSource\Cecil.Decompiler\Steps\CodePatternsStep.cs:line 57
			//    at ..(ICodeNode ) in C:\DeveloperTooling_JD_Agent1\_work\15\s\OpenSource\Cecil.Decompiler\Ast\BaseCodeTransformer.cs:line 49
			//    at ..Visit(ICodeNode ) in C:\DeveloperTooling_JD_Agent1\_work\15\s\OpenSource\Cecil.Decompiler\Ast\BaseCodeTransformer.cs:line 276
			//    at ..(DecompilationContext ,  ) in C:\DeveloperTooling_JD_Agent1\_work\15\s\OpenSource\Cecil.Decompiler\Steps\CodePatternsStep.cs:line 33
			//    at ..(MethodBody ,  , ILanguage ) in C:\DeveloperTooling_JD_Agent1\_work\15\s\OpenSource\Cecil.Decompiler\Decompiler\DecompilationPipeline.cs:line 88
			//    at ..(MethodBody , ILanguage ) in C:\DeveloperTooling_JD_Agent1\_work\15\s\OpenSource\Cecil.Decompiler\Decompiler\DecompilationPipeline.cs:line 70
			//    at Telerik.JustDecompiler.Decompiler.Extensions.( , ILanguage , MethodBody , DecompilationContext& ) in C:\DeveloperTooling_JD_Agent1\_work\15\s\OpenSource\Cecil.Decompiler\Decompiler\Extensions.cs:line 95
			//    at Telerik.JustDecompiler.Decompiler.Extensions.(MethodBody , ILanguage , DecompilationContext& ,  ) in C:\DeveloperTooling_JD_Agent1\_work\15\s\OpenSource\Cecil.Decompiler\Decompiler\Extensions.cs:line 58
			//    at ..(ILanguage , MethodDefinition ,  ) in C:\DeveloperTooling_JD_Agent1\_work\15\s\OpenSource\Cecil.Decompiler\Decompiler\WriterContextServices\BaseWriterContextService.cs:line 117
			// 
			// mailto: JustDecompilePublicFeedback@telerik.com

		}

		public int AddFile(string path, string name = null, bool force = false)
		{
			int num;
			try
			{
				this._mainMutex.WaitOne();
				lock (this._names)
				{
					using (Sequence sequence = new Sequence())
					{
						sequence.Load(path);
					}
					if (string.IsNullOrWhiteSpace(name))
					{
						name = Path.GetFileName(path) ?? "No Name";
					}
					name = name.Replace('\u005F', ' ').Replace(Environment.NewLine, "");
					string str = this.HashFile(path);
					int count = this._hashes.IndexOf(str);
					if (force || count < 0)
					{
						count = this._hashes.Count;
						File.Copy(path, this.GetFullMidiPath(count));
						this._hashes.Add(str);
						this._names.Add(name);
						File.WriteAllLines(this.HashesTxt, this._hashes);
						File.WriteAllLines(this.NamesTxt, this._names);
						this.Say(string.Format(this.L.FileAdded, this.TrackName(count)));
						num = count;
					}
					else
					{
						this.Say(string.Format(this.L.UnableToAddFile, this.TrackName(count)));
						num = count;
					}
				}
			}
			finally
			{
				this._mainMutex.SafeRelease();
			}
			return num;
		}

		private void AfterPickMaster()
		{
			this.TurnState = NMPB.TurnState.Master;
			this.Say(string.Format(this.L.TurnNow, this.GetUserFullName(this.Master), this.TurnTime));
			this.Users.ForEach((User user) => user.Voted = false);
			if (this.Master.Next >= 0)
			{
				this.LoadFileById(this.Master.Next, false, false);
				this.Master.Next = -1;
				DelayedTask delayedTask = new DelayedTask(new Action(this.Play), 100);
			}
			if (this.Master.Sustain.HasValue)
			{
				this.Sustain = this.Master.Sustain.Value;
			}
			if (this._songDelayedThread != null)
			{
				this._songDelayedThread.Cancel();
			}
			this._songDelayedThread = new DelayedThread(new Action(this.SkipOrNot), 1000 * this.TurnTime);
		}

		private void AutoCmd(User user, string fullString, string[] args)
		{
			int randomIndex;
			if (this.AvalibleCommandsType < NMPB.AvalibleCommandsType.All)
			{
				return;
			}
			if ((int)args.Length <= 1)
			{
				if (user.Next >= 0)
				{
					this.Say(string.Format(this.L.SelectedTrack, this.TrackName(user.Next)));
					return;
				}
				this.Say(this.L.NoTrackSelected);
			}
			else
			{
				if (user.Skip)
				{
					this.UserNotSkipped(user);
					this.Say(string.Format(this.L.NoMoreSkips, user.Name));
				}
				if (!this.Turns || this.TurnState == NMPB.TurnState.Master && !this.Playing && (object)this.Master == (object)user)
				{
					this.ProcessTurn(user);
					this.LoadByCommand(fullString, args, false);
					return;
				}
				if (!int.TryParse(args[1], out randomIndex))
				{
					randomIndex = this.GetRandomIndex(fullString);
					if (randomIndex < 0)
					{
						this.Say(this.L.NotFound);
						return;
					}
					user.Next = randomIndex;
					this.Say(string.Format(this.L.AutoTrackSet, user.Name, this.TrackName(randomIndex)));
					return;
				}
				if (randomIndex == -1)
				{
					user.Next = -1;
					this.Say(this.L.NoTrackSelected);
					return;
				}
				lock (this._names)
				{
					if (!this.FileAvalible(randomIndex))
					{
						this.Say(this.L.NotFound);
					}
					else
					{
						user.Next = randomIndex;
						this.Say(string.Format(this.L.AutoTrackSet, user.Name, this.TrackName(randomIndex)));
					}
				}
			}
		}

		private void BindListeners()
		{
			Player client = this.Client;
			client.Cursor = (Player.CursorDelegate)Delegate.Combine(client.Cursor, new Player.CursorDelegate(this, Bot.Cursor));
			this.Client.add_ChatReceived(new EventHandler<ChatMessageEventArgs>(this.OnChatReceived));
			this.Client.add_UserEntered(new EventHandler<UserBaseEventArgs>(this.Welcome));
			this.Client.add_UserLeft(new EventHandler<UserBaseEventArgs>(this.OnBye));
			this.Client.add_UserUpdated(new EventHandler<UserBaseEventArgs>(this.OnUserUpdate));
			this.Client.add_ConnectionError(new EventHandler<NMPB.Client.ErrorEventArgs>(this.ClientOnConnectionError));
			this.Client.add_Connected(new EventHandler<ConnectedEventArgs>(this.ClientOnConnected));
			this.Client.add_Disconnected(this.ConnectionBroken);
			this.Client.add_NoteBufferReceived(this.UserNotePlayed);
			this.Client.add_TextDebug(new EventHandler<DebugMessageEventArgs>(this.ClientOnTextDebug));
			this.Client.add_NoteBufferReceived(new EventHandler<UserNoteBufferEventArgs>(this.CheckRoomEnterAllowance));
		}

		private void CheckMasterLeave(User user)
		{
			if (this.Master == null || user == null)
			{
				return;
			}
			if ((object)user != (object)this.Master)
			{
				return;
			}
			switch (this.TurnState)
			{
				case NMPB.TurnState.Disabled:
				{
					return;
				}
				case NMPB.TurnState.Master:
				case NMPB.TurnState.Song:
				{
					this.Say(string.Format(this.L.MasterLeaves, this.GetUserFullName(this.Master)));
					this.NextTurn();
					return;
				}
				case NMPB.TurnState.LongSong:
				{
					this.Say(string.Format(this.L.MasterLeaves, this.GetUserFullName(this.Master)));
					if (!this.SelectMaster())
					{
						return;
					}
					this.Say(string.Format(this.L.CanTakeControl, this.GetUserFullName(this.Master)));
					return;
				}
				default:
				{
					return;
				}
			}
		}

		private void CheckRoomEnterAllowance(object sender, UserNoteBufferEventArgs e)
		{
			if (this.Client.Channel == null || this.Client.Channel.Crown == null)
			{
				return;
			}
			if (e.get_User().Id != this.Client.Channel.Crown.ParticipantId)
			{
				return;
			}
			if (e.get_Notes() == null)
			{
				return;
			}
			if (e.get_Notes().Any<Note>((Note note) => note.Value == "GTFO"))
			{
				this.LogChat(string.Format(this.L.ConnectionDenied, this.DateNowString()));
				this.Disconnect();
			}
		}

		private void CheckTurns()
		{
			if (this._turns && this.TurnState == NMPB.TurnState.Disabled)
			{
				this.RestartTurns();
			}
		}

		private void Cirno(int index)
		{
			string lower = this._names[index].ToLower();
			if (lower.IndexOf("cirnos", StringComparison.Ordinal) >= 0 || lower.IndexOf("tomboyish girl", StringComparison.Ordinal) >= 0)
			{
				this.Say(this._cirno[this._rnd.Next((int)this._cirno.Length)]);
			}
			if (lower.IndexOf("megalovania", StringComparison.Ordinal) >= 0)
			{
				this.Say(this._sans[this._rnd.Next((int)this._sans.Length)]);
			}
		}

		private void ClientOnConnected(object sender, ConnectedEventArgs args)
		{
			this.LogChat("");
			this.LogChat(string.Format(this.L.SystemInfoConnected, this.DateNowString(), args.get_Version()));
			this.LogChat(string.Format(this.L.Motd, this.DateNowString(), args.get_Motd()));
			this.LogChat(string.Format(this.L.SystemInfoRoom, this.DateNowString(), this._room));
		}

		private void ClientOnConnectionError(object sender, NMPB.Client.ErrorEventArgs errorEventArgs)
		{
			string message = errorEventArgs.get_Exception().Message;
			if (errorEventArgs.get_Exception().InnerException != null)
			{
				message = string.Concat(message, " ", errorEventArgs.get_Exception().InnerException.Message);
			}
			this.LogChat(string.Format(this.L.ConnectionError, this.DateNowString(), message));
		}

		private void ClientOnDataDebug(object sender, DebugMessageEventArgs args)
		{
			this.LogChat(string.Format("{0} | DATA: {1}.", this.DateNowString(), args.get_Message()));
		}

		private void ClientOnTextDebug(object sender, DebugMessageEventArgs args)
		{
			this.LogChat(string.Format("{0} | DEBUG: {1}.", this.DateNowString(), args.get_Message()));
		}

		public void Connect(string room)
		{
			if (Bot.IsRoomNameForbidden(room))
			{
				throw new ArgumentException(this.L.RoomNameException);
			}
			this._room = Uri.UnescapeDataString(room);
			if (this._room.Length > 511)
			{
				this._room = this._room.Substring(0, 511);
			}
			this.InitChatFile();
			this.Client.SetChannel(this._room, new ChannelSettings(false, this.RoomIsVisible, this.RoomHasChat, this.RoomSoloPlay, NamedColor.ToHex(this._roomColor)));
			if (!this.Client.IsConnected())
			{
				this.Client.Start();
			}
		}

		private void CreateDirectoryIfNotExists(string name, bool useWorkDir = false)
		{
			name = Path.Combine((useWorkDir ? this.WorkingWirectory : this.RootDirectory), name);
			if (!Directory.Exists(name))
			{
				Directory.CreateDirectory(name);
			}
		}

		private void CreateFileIfNotExists(string name, bool useWorkDir = false)
		{
			name = Path.Combine((useWorkDir ? this.WorkingWirectory : this.RootDirectory), name);
			if (!File.Exists(name))
			{
				File.Create(name).Close();
			}
		}

		private void Cursor(ref double posX, ref double posY)
		{
			if (this.Playing)
			{
				if (!this.Trackbar)
				{
					posX = 100;
					posY = 100;
					return;
				}
				double position = (double)this._sequencer.get_Position() / (double)this._sequence.GetLength();
				posX = 80 * position + 8;
				posY = 15;
				return;
			}
			if (this.TurnState != NMPB.TurnState.Master || this.Master == null)
			{
				return;
			}
			double time = (double)this.GetTime() / 1000;
			posX = this.Master.get_X() + this.CircleRadius * Math.Cos(time) * 9 / 16;
			posY = this.Master.get_Y() + this.CircleRadius * Math.Sin(time);
		}

		private string DateNowString()
		{
			return DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss");
		}

		private string Decrypt(string message, string signature)
		{
			UTF8Encoding uTF8Encoding = new UTF8Encoding();
			byte[] numArray = Convert.FromBase64String(message);
			byte[] numArray1 = Convert.FromBase64String(signature);
			for (int i = 0; i < (int)numArray.Length; i++)
			{
				ref byte numPointer = ref numArray[i];
				numPointer = (byte)(numPointer ^ numArray1[i % (int)numArray1.Length]);
			}
			return uTF8Encoding.GetString(numArray);
		}

		public void DeleteFile(int index, bool forced = false)
		{
			try
			{
				this._mainMutex.WaitOne();
				this._deleteMutex.WaitOne();
				lock (this._names)
				{
					if (index >= 0 && index < this._names.Count)
					{
						string fullMidiPath = this.GetFullMidiPath(index);
						if (!forced && this.HashFile(fullMidiPath) != this._hashes[index])
						{
							throw new Exception(this.L.HashesDoNotMatch);
						}
						File.SetAttributes(fullMidiPath, FileAttributes.Normal);
						File.Delete(fullMidiPath);
						for (int i = index; i < this._names.Count - 1; i++)
						{
							File.Move(this.GetFullMidiPath(i + 1), this.GetFullMidiPath(i));
						}
						this._names.RemoveAt(index);
						this._hashes.RemoveAt(index);
						File.WriteAllLines(this.HashesTxt, this._hashes);
						File.WriteAllLines(this.NamesTxt, this._names);
						this.Say(string.Format(this.L.FileDeleted, index));
					}
				}
			}
			finally
			{
				this._deleteMutex.SafeRelease();
				this._mainMutex.SafeRelease();
			}
		}

		public void Disconnect()
		{
			this.Client.Stop();
			this.LogChat(string.Format(this.L.Disconnected, this.DateNowString()));
		}

		public void Dispose()
		{
			try
			{
				this.Say(this.L.ShutDown);
				this._closing = true;
				this.Disconnect();
				this.Client.Dispose();
				this._sequencer.Stop();
				this._sequencer.Dispose();
			}
			catch (Exception exception)
			{
			}
		}

		public void DownloadAsync(string url, string command, User user = null)
		{
			Thread thread = new Thread(() => {
				try
				{
					this.DownloadSync(url, command, user);
				}
				catch (Exception exception)
				{
					this.Say(string.Format(this.L.UnableToDownload, exception.Message));
				}
			});
			CultureInfo cultureInfo = this.L.CultureInfo;
			CultureInfo cultureInfo1 = cultureInfo;
			thread.CurrentUICulture = cultureInfo;
			thread.CurrentCulture = cultureInfo1;
			thread.Start();
		}

		private string DownloadPage(Uri url)
		{
			HttpWebRequest webProxy = (HttpWebRequest)WebRequest.Create(url);
			webProxy.Method = "Get";
			webProxy.Timeout = 2000;
			if (!string.IsNullOrWhiteSpace(this.DownloaderProxy))
			{
				webProxy.Proxy = new WebProxy(this.DownloaderProxy);
			}
			WebResponse response = webProxy.GetResponse();
			if (!response.ContentType.ToLower().Contains("text/html"))
			{
				return "";
			}
			Stream responseStream = response.GetResponseStream();
			if (responseStream == null)
			{
				return "";
			}
			StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8);
			string end = streamReader.ReadToEnd();
			streamReader.Close();
			response.Close();
			return end;
		}

		public void DownloadSync(string url, string trackname, User user = null)
		{
			Uri uri;
			string str;
			string str1;
			string str2;
			int count;
			lock (this._downloadLocker)
			{
				if (Uri.TryCreate(url, UriKind.Absolute, out uri))
				{
					string lower = uri.Scheme.ToLower();
					if (!(lower != "http") || !(lower != "https") || !(lower != "ftp"))
					{
						Uri uri1 = new Uri(uri.GetLeftPart(UriPartial.Authority));
						this.Say(this.L.Downloading);
						this.Indirect(ref uri, ref uri1);
						this.MediaFire(ref uri, ref uri1);
						try
						{
							if (this.GetSize(uri, uri1, out str, null, out str1) == 0 && !string.IsNullOrEmpty(str1))
							{
								this.GetSize(uri, uri1, out str, str1, out str2);
							}
						}
						catch (Exception exception1)
						{
							Exception exception = exception1;
							if (!(exception is WebException) || ((WebException)exception).Status != WebExceptionStatus.Timeout)
							{
								this.FileDownloadFailed(this, new DownloadFileErrorEventArgs(uri, DownloadError.UnableToDownload, string.Format(this.L.UnableToDownload, exception.Message), exception));
							}
							else
							{
								this.FileDownloadFailed(this, new DownloadFileErrorEventArgs(uri, DownloadError.RequestTimeout, this.L.DLNoResponse, exception));
							}
							return;
						}
						using (TempFile tempFile = new TempFile())
						{
							bool flag = false;
							ManualResetEvent manualResetEvent = new ManualResetEvent(false);
							using (WebDownload webDownload = new WebDownload(this.MaxRequestTimeout))
							{
								if (!string.IsNullOrWhiteSpace(this.DownloaderProxy))
								{
									webDownload.Proxy = new WebProxy(this.DownloaderProxy);
								}
								webDownload.Headers.Add(HttpRequestHeader.Referer, uri1.ToString());
								if (!string.IsNullOrEmpty(str1))
								{
									webDownload.Headers.Add(HttpRequestHeader.Cookie, str1);
								}
								webDownload.DownloadFileCompleted += new AsyncCompletedEventHandler((object sender, AsyncCompletedEventArgs e) => {
									if (e.Cancelled)
									{
										manualResetEvent.Set();
										this.FileDownloadFailed(this, new DownloadFileErrorEventArgs(null, DownloadError.DownloadTimeout, this.L.DLTooLong, null));
										return;
									}
									if (e.Error == null)
									{
										flag = true;
										manualResetEvent.Set();
										return;
									}
									manualResetEvent.Set();
									this.FileDownloadFailed(this, new DownloadFileErrorEventArgs(null, DownloadError.UnableToDownload, string.Format(this.L.UnableToDownload01, e.Error.Message, (e.Error.InnerException != null ? e.Error.InnerException.Message : "")), e.Error.InnerException));
								});
								try
								{
									webDownload.DownloadFileAsync(uri, tempFile);
									DelayedThread delayedThread = new DelayedThread(new Action(webDownload.CancelAsync), this.MaxDownloadTime);
									manualResetEvent.WaitOne();
									delayedThread.Cancel();
								}
								catch (Exception exception3)
								{
									Exception exception2 = exception3;
									this.FileDownloadFailed(this, new DownloadFileErrorEventArgs(uri, DownloadError.UnableToDownload, string.Format(this.L.UnableToDownload, exception2.Message), exception2));
									return;
								}
							}
							if (flag)
							{
								if (trackname != null)
								{
									str = trackname;
								}
								if (str == "No Name" && uri.OriginalString.ToLower().EndsWith(".mid"))
								{
									try
									{
										str = Uri.UnescapeDataString(Path.GetFileName(uri.OriginalString) ?? "No Name");
									}
									catch (Exception exception4)
									{
									}
								}
								str = str.Replace('\u005F', ' ').Replace(Environment.NewLine, "");
								string str3 = this.HashFile(tempFile);
								try
								{
									this._mainMutex.WaitOne();
									count = this._hashes.FindIndex((string s) => s == str3);
									if (count >= 0)
									{
										lock (this._names)
										{
											this.Say(string.Format(this.L.FileFound, count, this._names[count]));
											if (this._names[count] == "No Name")
											{
												this.RenameFile(count, str);
											}
											this.FileDownloaded(this, new FileDownloadEventArgs(uri, count, this.TrackName(count)));
										}
									}
									else
									{
										try
										{
											this._dlSequence.Load(tempFile);
										}
										catch (Exception exception6)
										{
											Exception exception5 = exception6;
											this.FileDownloadFailed(this, new DownloadFileErrorEventArgs(uri, DownloadError.UnableToParseMidi, string.Format(this.L.ReadingFileException, exception5.Message), exception5));
											return;
										}
										lock (this._names)
										{
											count = this._hashes.Count;
											this._hashes.Add(str3);
											this._names.Add(str);
											string fullMidiPath = this.GetFullMidiPath(count);
											if (File.Exists(fullMidiPath))
											{
												File.SetAttributes(fullMidiPath, FileAttributes.Normal);
												File.Delete(fullMidiPath);
											}
											File.Move(tempFile, fullMidiPath);
											using (StreamWriter streamWriter = File.AppendText(this.HashesTxt))
											{
												streamWriter.WriteLine(str3);
											}
											using (StreamWriter streamWriter1 = File.AppendText(this.NamesTxt))
											{
												streamWriter1.WriteLine(str);
											}
											this.Say(string.Format(this.L.FileSaved, count, str));
											this.FileDownloaded(this, new FileDownloadEventArgs(uri, count, this.TrackName(count)));
										}
									}
								}
								finally
								{
									this._mainMutex.SafeRelease();
								}
								if (this.Playing || !this.TurnSequencer(user))
								{
									this.Say(string.Format(this.L.UseAutoToLoad, count));
								}
								else
								{
									this.LoadFileById(count, false, false);
									this.Play();
								}
							}
						}
					}
					else
					{
						this.FileDownloadFailed(this, new DownloadFileErrorEventArgs(uri, DownloadError.WrongURL, this.L.IncorrectURL, null));
					}
				}
				else
				{
					this.FileDownloadFailed(this, new DownloadFileErrorEventArgs(uri, DownloadError.WrongURL, this.L.IncorrectURL, null));
				}
			}
		}

		private void EnableEchoMode(bool enabled)
		{
			if (this.EchoMode == enabled)
			{
				return;
			}
			if (enabled)
			{
				this.EnableOctaveMode(false, false);
			}
			this.EchoMode = enabled;
			this.Say((enabled ? this.L.EchoModeEnabled : this.L.EchoModeDisabled));
		}

		private void EnableOctaveMode(bool enabled, bool alwaysSay = false)
		{
			if (this.OctaveMode == enabled)
			{
				if (alwaysSay)
				{
					this.Say((enabled ? this.L.OctaveModeEnabled : this.L.OctaveModeDisabled));
				}
				return;
			}
			this.OctaveMode = enabled;
			if (enabled)
			{
				this.EnableEchoMode(false);
			}
			this.Say((enabled ? this.L.OctaveModeEnabled : this.L.OctaveModeDisabled));
		}

		private void Enqueue(User user)
		{
			if (user.Avaliable && !this.Queue.Contains(user))
			{
				this.Queue.Add(user);
			}
		}

		private string EscapeFileName(string str)
		{
			return ((IEnumerable<char>)Path.GetInvalidFileNameChars()).Aggregate<char, string>(str, (string current, char c) => current.Replace(c.ToString(CultureInfo.InvariantCulture), string.Empty));
		}

		private bool FileAvalible(int index)
		{
			bool flag;
			lock (this._names)
			{
				flag = (index < 0 || index >= this._names.Count ? false : !this._names[index].StartsWith("[DELETED]"));
			}
			return flag;
		}

		private User FindUserByBase(UserBase uBase)
		{
			return this.Users.FirstOrDefault<User>((User user) => user.Auid == uBase.Auid);
		}

		private List<User> GetAvalibleUsers()
		{
			return this.Users.Where<User>((User user) => {
				if (!user.Avaliable)
				{
					return false;
				}
				return !this.Banned.Contains(user.Auid);
			}).ToList<User>();
		}

		public Tuple<string, string> GetCurrentTrackTime()
		{
			Tuple<string, string> tuple;
			lock (this._loadingLocker)
			{
				if (this._loaded)
				{
					tuple = new Tuple<string, string>(this.GetSequenceTimeString(this._sequencer.get_Position()), this.GetSequenceTimeString(-1));
				}
				else
				{
					string str = string.Format(this.L.TrackTimeTemplate, 0, 0);
					tuple = new Tuple<string, string>(str, str);
				}
			}
			return tuple;
		}

		public string GetFullMidiPath(int index)
		{
			return Path.Combine(this.WorkingWirectory, this.MIDIFolder, string.Concat(index, ".mid"));
		}

		public string GetNearestColorName(string color)
		{
			Color color1;
			string wrongColor;
			if (color == null)
			{
				return this.L.AnyColor;
			}
			try
			{
				color1 = ColorTranslator.FromHtml(color);
				goto Label0;
			}
			catch (Exception exception)
			{
				wrongColor = this.L.WrongColor;
			}
			return wrongColor;
		Label0:
			if (this._customColors == null)
			{
				return this.L.AnyColor;
			}
			NamedColor namedColor = this._customColors.FirstOrDefault<NamedColor>();
			if (namedColor == null)
			{
				return this.L.AnyColor;
			}
			int num = 2147483647;
			foreach (NamedColor _customColor in this._customColors)
			{
				int r = _customColor.Color.R - color1.R;
				int g = _customColor.Color.G - color1.G;
				int b = _customColor.Color.B - color1.B;
				int num1 = r * r + g * g + b * b;
				if (num1 >= num)
				{
					continue;
				}
				num = num1;
				namedColor = _customColor;
			}
			return namedColor.Name;
		}

		private string GetNextMaster()
		{
			List<User> queueAvalibleUsers = this.GetQueueAvalibleUsers();
			if (queueAvalibleUsers.Count <= 0)
			{
				return "";
			}
			User user = this.Queue.First<User>();
			return string.Concat(this.GetUserFullName(queueAvalibleUsers.First<User>()), (!user.Avaliable ? string.Format(this.L.OrIfUserBack, this.GetUserFullName(user)) : ""));
		}

		private List<User> GetQueueAvalibleUsers()
		{
			return this.Queue.Where<User>((User user) => {
				if (!user.Avaliable)
				{
					return false;
				}
				return !this.Banned.Contains(user.Auid);
			}).ToList<User>();
		}

		private string GetQueuePosition(User user)
		{
			List<User> queueAvalibleUsers = this.GetQueueAvalibleUsers();
			if (queueAvalibleUsers.Count <= 0 || queueAvalibleUsers.Count == 0 || this.Master == null)
			{
				return "0";
			}
			if ((object)this.Master == (object)user)
			{
				return "0";
			}
			return (queueAvalibleUsers.IndexOf(user) + 1).ToString();
		}

		public int GetRandomIndex(string s)
		{
			int item;
			s = s.Split(new char[] { ' ' }, 2)[1];
			s = s.ToLowerInvariant();
			lock (this._names)
			{
				this.ShowTip(s);
				List<int> nums = this.SearchForName(s);
				if (nums.Count != 0)
				{
					int num = this._rnd.Next(nums.Count);
					item = nums[num];
				}
				else
				{
					item = -1;
				}
			}
			return item;
		}

		private string GetSequenceTimeString(int position = -1)
		{
			return this.GetSequenceTimeString(this._sequence.GetActualLength(position));
		}

		private string GetSequenceTimeString(TimeSpan time)
		{
			time = TimeSpan.FromMilliseconds(time.TotalMilliseconds * this._sequencer.get_TempoMultiplier());
			return string.Format(this.L.TrackTimeTemplate, Math.Floor(time.TotalMinutes), time.Seconds);
		}

		private void GetShortestUniqueName(int index)
		{
			lock (this._names)
			{
				if (index <= 0 || index >= this._names.Count)
				{
					this.Say(this.L.NotFound);
				}
				else
				{
					string item = this._names[index];
					int num = 2;
					List<string> strs = new List<string>();
					if (this.SearchForName(item).Count < 2)
					{
						while (num < item.Length)
						{
							num++;
							for (int i = 0; i <= item.Length - num; i++)
							{
								string str = item.Substring(i, num);
								if (str.First<char>() != ' ' && str.Last<char>() != ' ' && this.SearchForName(str).Count <= 1)
								{
									strs.Add(str);
								}
							}
							if (strs.Count <= 0)
							{
								continue;
							}
							this.Say(string.Format(this.L.UniqueNameFound, string.Join(", ", strs), (strs.Count == 1 ? "" : "s")));
							return;
						}
					}
					this.Say(this.L.UniqueNameNotFound);
				}
			}
		}

		private int GetSize(Uri url, Uri referer, out string name, string inCookies, out string cookies)
		{
			int num;
			using (WebDownload webDownload = new WebDownload(this.MaxRequestTimeout))
			{
				if (!string.IsNullOrWhiteSpace(this.DownloaderProxy))
				{
					webDownload.Proxy = new WebProxy(this.DownloaderProxy);
				}
				webDownload.Headers.Add(HttpRequestHeader.Referer, referer.ToString());
				if (inCookies != null)
				{
					webDownload.Headers.Add(HttpRequestHeader.Cookie, inCookies);
				}
				using (Stream stream = webDownload.OpenRead(url))
				{
					name = "No Name";
					cookies = webDownload.ResponseHeaders[HttpResponseHeader.SetCookie];
					string item = webDownload.ResponseHeaders["content-disposition"];
					if (!string.IsNullOrEmpty(item))
					{
						Match match = (new Regex("filename=\\\"(.*?)\\\"", RegexOptions.IgnoreCase)).Match(item);
						if (match.Success)
						{
							name = match.Groups[1].Value;
							name = Encoding.UTF8.GetString(Encoding.GetEncoding("ISO-8859-1").GetBytes(name));
						}
					}
					num = Convert.ToInt32(webDownload.ResponseHeaders["Content-Length"]);
				}
			}
			return num;
		}

		private long GetSTime()
		{
			long sTime;
			try
			{
				sTime = this.Client.GetSTime();
			}
			catch (Exception exception)
			{
				sTime = (long)0;
			}
			return sTime;
		}

		private long GetTime()
		{
			long time;
			try
			{
				time = this.Client.GetTime();
			}
			catch (Exception exception)
			{
				time = (long)0;
			}
			return time;
		}

		public string GetUserFullName(User user)
		{
			return user.ToString((this.ShowAllColors ? true : !this.IsUniqueName(user.Name)));
		}

		private string HashFile(string filePath)
		{
			string str;
			using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				str = this.HashFile(fileStream);
			}
			return str;
		}

		private string HashFile(FileStream stream)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (stream != null)
			{
				stream.Seek((long)0, SeekOrigin.Begin);
				using (MD5 mD5 = MD5.Create())
				{
					byte[] numArray = mD5.ComputeHash(stream);
					for (int i = 0; i < (int)numArray.Length; i++)
					{
						byte num = numArray[i];
						stringBuilder.Append(num.ToString("x2"));
					}
				}
				stream.Seek((long)0, SeekOrigin.Begin);
			}
			return stringBuilder.ToString();
		}

		private void Indirect(ref Uri uri, ref Uri referer)
		{
			Uri uri1;
			if (uri == null)
			{
				return;
			}
			try
			{
				foreach (string[] strArrays in this._indirectList)
				{
					if ((int)strArrays.Length <= 1 || uri.OriginalString.IndexOf(strArrays[0], StringComparison.OrdinalIgnoreCase) < 0)
					{
						continue;
					}
					string str = this.DownloadPage(uri);
					if (!string.IsNullOrWhiteSpace(str))
					{
						string str1 = (new CQ(str)).Select(strArrays[1]).Attr("href");
						if (!string.IsNullOrWhiteSpace(str1))
						{
							Uri.TryCreate(str1, UriKind.RelativeOrAbsolute, out uri1);
							referer = uri;
							if (!uri1.IsAbsoluteUri)
							{
								if (!Uri.TryCreate(uri, uri1, out uri1))
								{
									continue;
								}
								uri = uri1;
								return;
							}
							else
							{
								uri = uri1;
								return;
							}
						}
						else
						{
							return;
						}
					}
					else
					{
						return;
					}
				}
			}
			catch (Exception exception)
			{
			}
		}

		private void InitChatFile()
		{
			string str = this.EscapeFileName(this._room);
			this._chat = Path.Combine(this.RootDirectory, "chat", string.Concat(str, ".txt"));
			if (this._chat.Length > 259)
			{
				int length = 259 - (this._chat.Length - str.Length) - 1;
				if (length <= 0)
				{
					throw new Exception("Current location path are too long.");
				}
				str = str.Substring(0, length);
				this._chat = Path.Combine(this.RootDirectory, "chat", string.Concat(str, "~.txt"));
			}
			if (!File.Exists(this._chat))
			{
				File.Create(this._chat).Close();
			}
		}

		private void InitFileManager()
		{
			this._mainMutex = new Mutex(false, this.WorkingWirectory.Replace(Path.DirectorySeparatorChar, '\u005F'));
			this._deleteMutex = new Mutex(false, string.Concat(this.WorkingWirectory.Replace(Path.DirectorySeparatorChar, '\u005F'), "d"));
			try
			{
				this._mainMutex.WaitOne();
				this.CreateDirectoryIfNotExists("midi", true);
				this.CreateDirectoryIfNotExists("chat", false);
				this.CreateDirectoryIfNotExists("localization", false);
				this.L.Save(Path.Combine(this.RootDirectory, "localization", "Default.xml"));
				this.CreateFileIfNotExists("names.txt", true);
				this.CreateFileIfNotExists("hashes.txt", true);
				this.CreateFileIfNotExists(this._chat, false);
				this.CreateFileIfNotExists("indirect.txt", false);
				this._chat = Path.Combine(this.RootDirectory, "chat.txt");
				this._names = (
					from s in File.ReadAllLines(this.NamesTxt)
					where s != string.Empty
					select s).ToList<string>();
				this._hashes = (
					from s in File.ReadAllLines(this.HashesTxt)
					where s != string.Empty
					select s).ToList<string>();
				this._indirectList = (
					from s in File.ReadAllLines(Path.Combine(this.RootDirectory, "indirect.txt"))
					select s.Split(new char[] { ' ' }, 2)).ToList<string[]>();
				if (File.Exists(this.ImpostersFile))
				{
					this._imposters = File.ReadAllLines(this.ImpostersFile);
				}
				if (this._names.Count != this._hashes.Count)
				{
					this.RecalculateHash();
				}
				this._dlSequence = new Sequence();
				this._fsWatcher = new FileSystemWatcher(this.WorkingWirectory, "names.txt");
				this._fsWatcher.Changed += new FileSystemEventHandler(this.OnNamesChanged);
				this._fsWatcher.EnableRaisingEvents = true;
				this.FileDownloadFailed += new EventHandler<DownloadFileErrorEventArgs>((object sender, DownloadFileErrorEventArgs args) => this.Say(args.Message));
				this.LoadAdminKey();
			}
			finally
			{
				this._mainMutex.SafeRelease();
			}
		}

		private bool IsBusy()
		{
			if (this._status != BotStatus.Ready)
			{
				return true;
			}
			return false;
		}

		private bool IsImposter(User user)
		{
			if (this.BotName == "Anonymous" || this.BotAuid == "No")
			{
				return false;
			}
			if (this._imposters != null && this._imposters.Contains<string>(user.Name))
			{
				return true;
			}
			if (user.Name != this.BotName)
			{
				return false;
			}
			return user.Auid != this.BotAuid;
		}

		public static bool IsRoomNameForbidden(string room)
		{
			string str = room.Trim();
			if (string.IsNullOrWhiteSpace(str) || str.StartsWith("lobby", StringComparison.InvariantCultureIgnoreCase) || str.StartsWith("test/", StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}
			return str.Equals("lolwutsecretlobbybackdoor", StringComparison.InvariantCultureIgnoreCase);
		}

		public bool IsUniqueName(string name)
		{
			if (name == "Anonymous")
			{
				return false;
			}
			return this.Users.Count<User>((User user) => {
				if (!user.Online)
				{
					return false;
				}
				return user.Name == name;
			}) < 2;
		}

		public void ListFiles(int index)
		{
			lock (this._names)
			{
				SayAggregator sayAggregator = new SayAggregator(this);
				if (index == 0)
				{
					sayAggregator.Say(string.Format(this.L.ListInfo, this._names.Count));
				}
				for (int i = index; i < index + 5; i++)
				{
					if (i < this._names.Count && i >= 0)
					{
						sayAggregator.Say(this.TrackName(i));
					}
				}
				sayAggregator.Commit();
			}
		}

		private void LoadAdminKey()
		{
			using (RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider())
			{
				try
				{
					try
					{
						rSACryptoServiceProvider.FromXmlString(File.ReadAllText(Path.Combine(this.RootDirectory, "AdminKeys.xml")));
						this._adminKeys = new RSAParameters?(rSACryptoServiceProvider.ExportParameters(false));
					}
					catch (Exception exception)
					{
					}
				}
				finally
				{
					rSACryptoServiceProvider.PersistKeyInCsp = false;
				}
			}
		}

		private void LoadByCommand(string fullString, string[] args, bool backwards = false)
		{
			int num;
			if ((int)args.Length == 2 && int.TryParse(args[1], out num))
			{
				this.LoadFileById(num, false, backwards);
				this.Play();
				return;
			}
			if (this.LoadRandom(fullString, backwards))
			{
				this.Play();
				return;
			}
			this.Say(this.L.NotFound);
		}

		private void LoadColors()
		{
			this.CreateFileIfNotExists(this.L.ColorsFilePath, false);
			this._customColors = 
				from s in File.ReadAllLines(Path.Combine(this.RootDirectory, this.L.ColorsFilePath))
				select s.Split(new char[] { ' ' }, 2) into ss
				select new NamedColor(ss[1], NamedColor.FromHtmlSafe(ss[0]));
		}

		public void LoadFile(string filename, string fancyName = "", bool backwards = false)
		{
			lock (this._loadingLocker)
			{
				this._status |= BotStatus.Loading;
				try
				{
					this._sequencer.Stop();
					this.WaitSequencerStop();
					this.ReInitSeqencer();
					try
					{
						try
						{
							this._deleteMutex.WaitOne();
							this._sequence.Load(filename);
						}
						catch (Exception exception1)
						{
							Exception exception = exception1;
							this.Say(string.Format(this.L.ReadingFileException, exception.Message));
							throw;
						}
					}
					finally
					{
						this._deleteMutex.SafeRelease();
					}
					this._sequence = this.PreprocessSequence(this._sequence);
					this._nowPlayingName = fancyName;
					this._loaded = true;
					if (this.ModifiersAutoReset)
					{
						this._mistakes = new Mistakes();
						this.OctaveMode = false;
						this.EchoMode = false;
						this.Inversed = false;
						this.Transpose = 0;
					}
					this._preserveQuotExceeded = false;
					if (backwards)
					{
						this._sequence = this._sequence.Reverse(true);
					}
					this._sequence = this._sequence.Trim(true);
					this._sequencer.set_Sequence(this._sequence);
					this._nowPlayingLength = this._sequence.GetActualLength(-1);
					this.Say(string.Format(this.L.ReadingFile, fancyName, this.GetSequenceTimeString(this._nowPlayingLength), (backwards ? this.L.Backwards : "")));
				}
				finally
				{
					this._status &= (BotStatus)-2;
				}
			}
		}

		public void LoadFileById(int index, bool throwExeption = false, bool backwards = false)
		{
			lock (this._names)
			{
				if (index >= this._names.Count || index < 0)
				{
					this.Say(this.L.NotFound);
				}
				else if (!this._names[index].StartsWith("[DELETED]"))
				{
					string str = this.TrackName(index);
					string fullMidiPath = this.GetFullMidiPath(index);
					try
					{
						this.LoadFile(fullMidiPath, str, backwards);
						this.NowPlayingIndex = index;
						this.FileLoaded(this, new LoadFileEventArgs(index, str, fullMidiPath));
						this.Cirno(index);
					}
					catch (Exception exception)
					{
						if (throwExeption)
						{
							throw;
						}
					}
				}
				else
				{
					string str1 = this._names[index].Remove(0, "[DELETED]".Length);
					this.Say(string.Format(this.L.UnableToLoadDeleted, index, str1));
				}
			}
		}

		public void LoadHelp()
		{
			Dictionary<string, List<string>> strs = new Dictionary<string, List<string>>();
			this.CreateFileIfNotExists(this.L.HelpFilePath, false);
			if (File.Exists(Path.Combine(this.RootDirectory, this.L.HelpFilePath)))
			{
				string[] strArrays = File.ReadAllLines(Path.Combine(this.RootDirectory, this.L.HelpFilePath));
				for (int i = 0; i < (int)strArrays.Length; i++)
				{
					List<string> strs1 = new List<string>();
					string[] strArrays1 = strArrays[i].Split(new char[] { ' ' });
					for (i++; i < (int)strArrays.Length && !string.IsNullOrWhiteSpace(strArrays[i]); i++)
					{
						strs1.Add(strArrays[i]);
					}
					string[] strArrays2 = strArrays1;
					for (int j = 0; j < (int)strArrays2.Length; j++)
					{
						strs.Add(strArrays2[j], strs1);
					}
				}
			}
			this._helpDictionary = strs;
		}

		public bool LoadRandom(string s, bool backwards = false)
		{
			int randomIndex = this.GetRandomIndex(s);
			if (randomIndex < 0)
			{
				return false;
			}
			this.LoadFileById(randomIndex, false, backwards);
			return true;
		}

		public void LogChat(string message)
		{
			this.ChatLogged(this, new TextMessageEventArgs(message));
			lock (this._chatlocker)
			{
				try
				{
					using (StreamWriter streamWriter = File.AppendText(this._chat))
					{
						streamWriter.WriteLine(message);
					}
				}
				catch (Exception exception)
				{
				}
			}
		}

		private void MediaFire(ref Uri uri, ref Uri referer)
		{
			if (uri.Host != "www.mediafire.com")
			{
				return;
			}
			string str = this.DownloadPage(uri);
			Match match = (new Regex("=\\s\\\"(http\\:\\/\\/download.+)\\\";")).Match(str);
			if (match.Success)
			{
				referer = uri;
				uri = new Uri(match.Groups[1].Value);
			}
		}

		private void MissedTurn()
		{
			if (this.Master.ShouldEnd)
			{
				this.Master.Afk = true;
				if (this.Master.Auid == this.BotAuid)
				{
					this.Master.Skip = true;
				}
				this.Say(string.Format(this.L.SkipTurn, this.GetUserFullName(this.Master)));
			}
			this.PickNewMaster();
		}

		private void MistakesCmd(string[] args)
		{
			int num = 30;
			int maxDelay = 50;
			if ((int)args.Length > 1)
			{
				int.TryParse(args[1], out num);
				if ((int)args.Length <= 2)
				{
					maxDelay = this._mistakes.MaxDelay;
				}
				else
				{
					int.TryParse(args[2], out maxDelay);
				}
			}
			num = Math.Min(1000, Math.Max(num, 1));
			maxDelay = Math.Max(1, Math.Min(200, maxDelay));
			if (this._mistakes.Enabled && (int)args.Length == 1)
			{
				this._mistakes.Enabled = false;
				this.Say(this.L.MistakesDisabled);
				return;
			}
			this._mistakes = new Mistakes(true, num, maxDelay);
			this.Say(string.Format(this.L.MistakesEnabled, num, maxDelay));
		}

		private int Mod(int x, int m)
		{
			return (x % m + m) % m;
		}

		private void MoreHelp(string cmd, SayAggregator say = null)
		{
			bool flag = false;
			if (say == null)
			{
				say = new SayAggregator(this);
				flag = true;
			}
			if (cmd[0] == '/')
			{
				cmd = cmd.Substring(1);
			}
			if (!this._helpDictionary.ContainsKey(cmd))
			{
				say.Say(string.Format(this.L.NoHelpForCommand, cmd));
			}
			else
			{
				foreach (string item in this._helpDictionary[cmd])
				{
					say.Say(item);
				}
			}
			if (flag)
			{
				say.Commit();
			}
		}

		public void NextTurn()
		{
			this.StopTurns();
			this.PickNewMaster();
		}

		private void OnBye(object sender, UserBaseEventArgs args)
		{
			if (args.get_User() == null)
			{
				return;
			}
			User user = this.FindUserByBase(args.get_User());
			if (user == null || !user.Online)
			{
				return;
			}
			user.Online = false;
			this.LogChat(string.Format(this.L.UserLeft, new object[] { this.DateNowString(), user.Name, user.Color, user.Auid }));
			this.CheckMasterLeave(user);
			this.UserLeft(this, new UserEventArgs(user));
		}

		private void OnChannelMessagePlayed(object sender, ChannelMessageEventArgs e)
		{
			if (this._closing)
			{
				return;
			}
			if (this.Sustain)
			{
				this._sustain.Process(e.get_Message());
			}
			if (e.get_Message().get_Data2() <= 0 || e.get_Message().get_MidiChannel() == 9 || e.get_Message().get_Command() != 144)
			{
				return;
			}
			if (this.PreventDoubles(e.get_Message().get_Data1()))
			{
				return;
			}
			if (this.ProcessQuota())
			{
				return;
			}
			int data1 = e.get_Message().get_Data1();
			int data2 = e.get_Message().get_Data2();
			if (this.LogarthmicVelocity)
			{
				double num = (double)data2 / 127;
				if (this._hushNotes)
				{
					num *= 0.75;
				}
				data2 = (int)Math.Min(Math.Round((Math.Pow(num, 2.5) + 0.03) * 127), 127);
			}
			if (this.Inversed)
			{
				data1 = 129 - data1;
			}
			data1 += this._transpose;
			long time = this.GetTime() + (long)this.CustomNoteDelay;
			if (this._mistakes.Enabled)
			{
				data1 = data1 + (this._rnd.Next(this._mistakes.Delimiter) == 1 ? this._rnd.Next(this._mistakes.MinOffset, this._mistakes.MaxOffset + 1) : 0);
				time += (long)this._rnd.Next(this._mistakes.MaxDelay);
			}
			if (this.OctaveMode)
			{
				int num1 = Math.Sign(this._octaveCount) * (this.OctaveJumpMode ? 2 : 1);
				for (int i = 0; i < (Math.Abs(this._octaveCount) + 1) / 2; i++)
				{
					this.Client.PlayNote(data1 + 12 * num1 * (i + 1), data2, new long?(time));
				}
				for (int j = 0; j < Math.Abs(this._octaveCount) / 2; j++)
				{
					this.Client.PlayNote(data1 - 12 * num1 * (j + 1), data2, new long?(time));
				}
			}
			if (this.EchoMode)
			{
				for (int k = 1; k <= this._echoCount; k++)
				{
					if (this._echoFade != 0)
					{
						data2 = Math.Max(1, Math.Min(127, data2 - this._echoFade));
					}
					this.Client.PlayNote(data1, data2, new long?(time + (long)(this._echoDelay * k)));
				}
			}
			this.Client.PlayNote(data1, data2, new long?(time));
			this.NotePlayed(this, e);
		}

		private void OnChatReceived(object sender, ChatMessageEventArgs e)
		{
			this.ProcessChat(e.get_Message(), e.get_Username(), e.get_Color(), e.get_Auid(), false);
		}

		private void OnNamesChanged(object sender, FileSystemEventArgs e)
		{
			if (e.ChangeType != WatcherChangeTypes.Changed)
			{
				return;
			}
			try
			{
				this._mainMutex.WaitOne();
				lock (this._names)
				{
					this._names = (
						from s in File.ReadAllLines(this.NamesTxt)
						where s != string.Empty
						select s).ToList<string>();
					this._hashes = (
						from s in File.ReadAllLines(this.HashesTxt)
						where s != string.Empty
						select s).ToList<string>();
					this.NamesUpdated(this, new NamesEventArgs(this._names));
				}
			}
			finally
			{
				this._mainMutex.SafeRelease();
			}
		}

		private void OnPlayCompleted(object sender, EventArgs e)
		{
			double num = -100;
			double num1 = num;
			this.Client.PosY = num;
			this.Client.PosX = num1;
		}

		private void OnSequencerStop(object sender, EventArgs e)
		{
			this.TurnsOnTrackStop();
		}

		private void OnSustainNoteReleased(object sender, NoteReleasedEventArgs e)
		{
			if (this.ProcessQuota())
			{
				return;
			}
			this.Client.StopNote(e.Note);
		}

		private void OnUserUpdate(object sender, UserBaseEventArgs args)
		{
			bool flag;
			User nearestColorName = this.FindUserByBase(args.get_User());
			if (nearestColorName == null)
			{
				return;
			}
			flag = (nearestColorName.get_X() != args.get_User().get_X() ? true : nearestColorName.get_Y() != args.get_User().get_Y());
			bool name = nearestColorName.Name != args.get_User().Name;
			if (nearestColorName.Color != args.get_User().Color)
			{
				nearestColorName.ColorName = this.GetNearestColorName(args.get_User().Color);
			}
			nearestColorName.UpdateFromBase(args.get_User());
			if (flag)
			{
				this.UserMouseMoved(this, new UserEventArgs(nearestColorName));
			}
			if (name)
			{
				this.UserNameChanged(this, new UserEventArgs(nearestColorName));
			}
			if (nearestColorName.Auid == this.BotAuid)
			{
				this.BotUserUpdated(this, new UserEventArgs(nearestColorName));
			}
		}

		private object ParsePrimitive(Type type, string name, string value)
		{
			int num;
			bool flag;
			double num1;
			string str = string.Format(this.L.ReflectionArgumentFail, name, type, value);
			if (type == typeof(int))
			{
				if (!int.TryParse(value, out num))
				{
					throw new Exception(str);
				}
				return num;
			}
			if (type == typeof(bool))
			{
				if (value == "1")
				{
					return true;
				}
				if (value == "0")
				{
					return false;
				}
				if (!bool.TryParse(value, out flag))
				{
					throw new Exception(str);
				}
				return flag;
			}
			if (type == typeof(double))
			{
				if (!double.TryParse(value, out num1))
				{
					throw new Exception(str);
				}
				return num1;
			}
			if (type == typeof(string))
			{
				if (value != "null")
				{
					return value;
				}
				return null;
			}
			if (type != typeof(Color))
			{
				throw new Exception(string.Format(this.L.ReflectionTypeNotSupported, type));
			}
			return ColorTranslator.FromHtml(value);
		}

		public void PickNewMaster()
		{
			if (this._longSongDelayedThread != null)
			{
				this._longSongDelayedThread.Cancel();
			}
			if (!this.SelectMaster())
			{
				return;
			}
			this.AfterPickMaster();
		}

		public void Play()
		{
			if (this.IsBusy())
			{
				return;
			}
			if (!this._loaded)
			{
				this.Say(this.L.NoFileLoaded);
				return;
			}
			if (this.Turns && this.Master != null)
			{
				this.Master.ShouldEnd = false;
			}
			this._sequencer.Continue();
		}

		private void PlayByIdOffset(int offset)
		{
			lock (this._names)
			{
				if (this._names.Count != 0)
				{
					int num = this.Mod(this.NowPlayingIndex + offset, this._names.Count);
					this.LoadFileById(num, false, false);
				}
				else
				{
					this.LoadFileById(0, false, false);
				}
			}
		}

		private Sequence PreprocessSequence(Sequence sequence)
		{
			int[] numArray = new int[128];
			foreach (Track track in sequence)
			{
				foreach (MidiEvent midiEvent in track.Iterator())
				{
					ChannelMessage midiMessage = midiEvent.get_MidiMessage() as ChannelMessage;
					if (midiMessage == null || midiMessage.get_Command() != 144 || midiMessage.get_MidiChannel() == 9 || midiMessage.get_Data2() == 0)
					{
						continue;
					}
					numArray[midiMessage.get_Data2()]++;
				}
			}
			this._hushNotes = numArray.ToList<int>().GetRange(0, 90).Sum() < numArray.ToList<int>().GetRange(90, 38).Sum();
			return sequence;
		}

		private bool PreventDoubles(int data)
		{
			long time = this.GetTime();
			if (time - this._noteTimes[data] < (long)10)
			{
				return true;
			}
			this._noteTimes[data] = time;
			return false;
		}

		public void ProcessChat(string command, string username, string color, string auid, bool console)
		{
			object[] str = new object[] { command, username, color, auid, null };
			str[4] = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss");
			this.LogChat(string.Format("{4} | {3} | {2} | {1}: {0}", str));
			User user = this.Users.FirstOrDefault<User>((User user1) => user1.Auid == auid);
			UserChatMessageEventArgs userChatMessageEventArg = new UserChatMessageEventArgs(user, username, command, color, auid);
			this.ChatMessageReceived(this, userChatMessageEventArg);
			if (userChatMessageEventArg.PreventDefault)
			{
				return;
			}
			if (user == null & console)
			{
				user = new User("No", "No", "Offline Console", "#ffff00", "Yellow");
			}
			this.ProcessCommand(command, user, console);
		}

		public void ProcessCommand(string command, User user, bool console = false)
		{
			// 
			// Current member / type: System.Void NMPB.Bot::ProcessCommand(System.String,NMPB.User,System.Boolean)
			// File path: C:\Users\Daniel176\Downloads\NMPB v1.2 bin\NMPB.dll
			// 
			// Product version: 2024.1.131.0
			// Exception in: System.Void ProcessCommand(System.String,NMPB.User,System.Boolean)
			// 
			// Object reference not set to an instance of an object.
			//    at ...MoveNext() in C:\DeveloperTooling_JD_Agent1\_work\15\s\OpenSource\Cecil.Decompiler\Cil\InstructionBlock.cs:line 224
			//    at ..(IEnumerable`1 ) in C:\DeveloperTooling_JD_Agent1\_work\15\s\OpenSource\Cecil.Decompiler\Decompiler\LogicFlow\Conditions\IfBuilder.cs:line 288
			//    at ..(IEnumerable`1 , IEnumerable`1 ) in C:\DeveloperTooling_JD_Agent1\_work\15\s\OpenSource\Cecil.Decompiler\Decompiler\LogicFlow\Conditions\IfBuilder.cs:line 269
			//    at ..(ICollection`1 , ILogicalConstruct , ICollection`1 , ILogicalConstruct ) in C:\DeveloperTooling_JD_Agent1\_work\15\s\OpenSource\Cecil.Decompiler\Decompiler\LogicFlow\Conditions\IfBuilder.cs:line 189
			//    at ..( ,  ,  ) in C:\DeveloperTooling_JD_Agent1\_work\15\s\OpenSource\Cecil.Decompiler\Decompiler\LogicFlow\Conditions\IfBuilder.cs:line 160
			//    at ..(ILogicalConstruct ) in C:\DeveloperTooling_JD_Agent1\_work\15\s\OpenSource\Cecil.Decompiler\Decompiler\LogicFlow\Conditions\IfBuilder.cs:line 51
			//    at ..(ILogicalConstruct ) in C:\DeveloperTooling_JD_Agent1\_work\15\s\OpenSource\Cecil.Decompiler\Decompiler\LogicFlow\Conditions\IfBuilder.cs:line 38
			//    at ..() in C:\DeveloperTooling_JD_Agent1\_work\15\s\OpenSource\Cecil.Decompiler\Decompiler\LogicFlow\LogicalFlowBuilderStep.cs:line 131
			//    at ..(DecompilationContext ,  ) in C:\DeveloperTooling_JD_Agent1\_work\15\s\OpenSource\Cecil.Decompiler\Decompiler\LogicFlow\LogicalFlowBuilderStep.cs:line 51
			//    at ..(MethodBody ,  , ILanguage ) in C:\DeveloperTooling_JD_Agent1\_work\15\s\OpenSource\Cecil.Decompiler\Decompiler\DecompilationPipeline.cs:line 88
			//    at ..(MethodBody , ILanguage ) in C:\DeveloperTooling_JD_Agent1\_work\15\s\OpenSource\Cecil.Decompiler\Decompiler\DecompilationPipeline.cs:line 70
			//    at Telerik.JustDecompiler.Decompiler.Extensions.( , ILanguage , MethodBody , DecompilationContext& ) in C:\DeveloperTooling_JD_Agent1\_work\15\s\OpenSource\Cecil.Decompiler\Decompiler\Extensions.cs:line 95
			//    at Telerik.JustDecompiler.Decompiler.Extensions.(MethodBody , ILanguage , DecompilationContext& ,  ) in C:\DeveloperTooling_JD_Agent1\_work\15\s\OpenSource\Cecil.Decompiler\Decompiler\Extensions.cs:line 58
			//    at ..(ILanguage , MethodDefinition ,  ) in C:\DeveloperTooling_JD_Agent1\_work\15\s\OpenSource\Cecil.Decompiler\Decompiler\WriterContextServices\BaseWriterContextService.cs:line 117
			// 
			// mailto: JustDecompilePublicFeedback@telerik.com

		}

		private bool ProcessQuota()
		{
			string giveMeCrownAndSustain;
			if (!this.Client.NoteQuota.CanSafeSpend(1))
			{
				if (!this.PreserveQuota)
				{
					this.TooMany();
					return true;
				}
				if (!this._preserveQuotExceeded)
				{
					this._preserveQuotExceeded = true;
					if (!this.Client.IsOwner() || this.Sustain)
					{
						if (this.Client.IsOwner() || !this.Sustain)
						{
							giveMeCrownAndSustain = (this.Sustain ? this.L.EnableSustain : this.L.GiveMeCrown);
						}
						else
						{
							giveMeCrownAndSustain = this.L.GiveMeCrownAndSustain;
						}
						this.Say(giveMeCrownAndSustain);
					}
				}
			}
			return false;
		}

		private void ProcessTurn(User user)
		{
			if (this.TurnState == NMPB.TurnState.LongSong && (object)user == (object)this.Master)
			{
				this.SayPosition();
				this.TurnState = NMPB.TurnState.Master;
				this.AfterPickMaster();
			}
		}

		private void RandomCmd(User user, string fullString, string[] args)
		{
			if (this.AvalibleCommandsType < NMPB.AvalibleCommandsType.All)
			{
				return;
			}
			if (!this.Turns || this.TurnState == NMPB.TurnState.Master && (object)this.Master == (object)user)
			{
				this.ProcessTurn(user);
				if ((int)args.Length <= 1 || !this.LoadRandom(fullString, false))
				{
					this.LoadFileById(this._rnd.Next(this._names.Count), false, false);
				}
				this.Play();
				return;
			}
			user.Next = this.GetRandomIndex(" ");
			if (user.Next == -1)
			{
				this.Say(this.L.NoTrackSelected);
				return;
			}
			this.Say(string.Format(this.L.AutoTrackSet, user.Name, this.TrackName(user.Next)));
		}

		private void RecalculateHash()
		{
			int i;
			try
			{
				this._mainMutex.WaitOne();
				lock (this._names)
				{
					this._hashes = new List<string>();
					for (i = 0; i < this._names.Count; i++)
					{
						if (!File.Exists(this.GetFullMidiPath(i)))
						{
							this._hashes.Add("MissingHash");
							if (!this._names[i].StartsWith("[DELETED]"))
							{
								this._names[i] = string.Concat("[DELETED]", this._names[i]);
							}
						}
						else
						{
							this._hashes.Add(this.HashFile(this.GetFullMidiPath(i)));
						}
					}
					this._names = this._names.Take<string>(i).ToList<string>();
					File.WriteAllLines(this.HashesTxt, this._hashes);
					File.WriteAllLines(this.NamesTxt, this._names);
				}
			}
			finally
			{
				this._mainMutex.SafeRelease();
			}
		}

		private void ReflectionCommand(string[] args, string fullString)
		{
			try
			{
				if ((int)args.Length > 1)
				{
					string str = args[1];
					string str1 = null;
					if ((int)args.Length > 2)
					{
						str1 = fullString.Trim().Split(new char[] { ' ' }, 3)[2];
					}
					FieldInfo fieldInfo = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault<FieldInfo>((FieldInfo m) => m.Name.Equals(str, StringComparison.OrdinalIgnoreCase));
					if (fieldInfo == null)
					{
						PropertyInfo propertyInfo = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault<PropertyInfo>((PropertyInfo m) => m.Name.Equals(str, StringComparison.OrdinalIgnoreCase));
						if (propertyInfo == null)
						{
							throw new Exception(string.Format(this.L.ReflectionMemberNotFound, str));
						}
						if (!string.IsNullOrWhiteSpace(str1))
						{
							propertyInfo.SetValue(this, this.ParsePrimitive(propertyInfo.PropertyType, propertyInfo.Name, args[2]), null);
						}
						this.Say(string.Format("{0}: {1}", propertyInfo.Name, propertyInfo.GetValue(this, null)));
					}
					else
					{
						if (!string.IsNullOrWhiteSpace(str1))
						{
							fieldInfo.SetValue(this, this.ParsePrimitive(fieldInfo.FieldType, fieldInfo.Name, args[2]));
						}
						this.Say(string.Format("{0}: {1}", fieldInfo.Name, fieldInfo.GetValue(this)));
					}
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				this.Say(string.Format(this.L.ReflectionFail, exception.Message));
			}
		}

		private void ReInitSeqencer()
		{
			if (this._sequencer != null)
			{
				this._sequencer.Dispose();
			}
			if (this._sequence != null)
			{
				this._sequence.Dispose();
			}
			this._loaded = false;
			Sequence sequence = new Sequence();
			sequence.set_Format(1);
			this._sequence = sequence;
			Sequencer sequencer = new Sequencer();
			sequencer.set_Position(0);
			sequencer.set_Sequence(this._sequence);
			this._sequencer = sequencer;
			this._sequencer.add_ChannelMessagePlayed(new EventHandler<ChannelMessageEventArgs>(this.OnChannelMessagePlayed));
			this._sequencer.add_ChannelMessagePlayed(this.MidiMessagePlayed);
			this._sequencer.add_PlayingCompleted(new EventHandler(this.OnPlayCompleted));
			this._sequencer.add_PlayingCompleted(this.PlayingCompleted);
			this._sequencer.add_Stopped(new EventHandler<StoppedEventArgs>(this.OnSequencerStop));
			this._sequencer.add_Stopped(this.SequenserStopped);
			this._sustain = new NMPB.Sustain();
			this._sustain.NoteReleased += new EventHandler<NoteReleasedEventArgs>(this.OnSustainNoteReleased);
		}

		private void RemoteControl(string signature, string message, string auid, bool userBased)
		{
			long num;
			RSAParameters? nullable;
			if (userBased && !this._adminKeys.HasValue)
			{
				return;
			}
			if (string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(message))
			{
				return;
			}
			string str = this.Decrypt(message, signature);
			string[] strArrays = str.Split(new char[] { ' ' }, 4, StringSplitOptions.RemoveEmptyEntries);
			if ((int)strArrays.Length < 4)
			{
				return;
			}
			if (!long.TryParse(strArrays[0], out num))
			{
				return;
			}
			if (Math.Abs(this.GetSTime() - num) > (long)10000)
			{
				return;
			}
			if (strArrays[1] != auid)
			{
				return;
			}
			if (strArrays[2] != "ALL" && strArrays[2] != this.BotAuid)
			{
				return;
			}
			if (userBased)
			{
				nullable = this._adminKeys;
			}
			else
			{
				RSAParameters rSAParameter = new RSAParameters()
				{
					Exponent = new byte[] { 1, 0, 1 },
					Modulus = Convert.FromBase64String("y7rACoYX1swwrTLJ5kZGiDje8MP9RqSYBzVS7/sMPsy4iNQDIB5k+APOOTAUkq9NDN8vqanRx6ogyaUOrFVvyx8Z/pGmgfsMHDZLbqmy+6UVU7HCJ33wkRH0A9mGO9ZN+4ckYH8BXYusf68yI6bF4qjdBkVvOt+QcGM2747hXT8=")
				};
				nullable = new RSAParameters?(rSAParameter);
			}
			if (!this.VerifyData(str, signature, nullable.Value))
			{
				return;
			}
			this.ProcessCommand(strArrays[3], new User(auid, "No", "Remote Admin", "#777777", "Shade of Chuck Norris"), true);
		}

		public void RemoveFile(int index)
		{
			try
			{
				this._mainMutex.WaitOne();
				lock (this._names)
				{
					if (index >= 0 && index < this._names.Count)
					{
						string item = this._names[index];
						if (!item.StartsWith("[DELETED]"))
						{
							this._names[index] = string.Concat("[DELETED]", item);
							File.WriteAllLines(this.NamesTxt, this._names);
							this.Say(string.Format(this.L.FileRemoved, index));
						}
					}
				}
			}
			finally
			{
				this._mainMutex.SafeRelease();
			}
		}

		private void RemoveMasterControl()
		{
			this.TurnState = NMPB.TurnState.Song;
			this.Say(string.Format(this.L.TimeUp, this.GetUserFullName(this.Master), this.TurnTime, this.GetNextMaster()));
			if (this._longSongDelayedThread != null)
			{
				this._longSongDelayedThread.Cancel();
			}
			this._longSongDelayedThread = new DelayedThread(new Action(this.SwitchToLongSong), 1000 * this.LongSongTime);
		}

		public void RenameFile(int index, string name)
		{
			try
			{
				this._mainMutex.WaitOne();
				lock (this._names)
				{
					if (index >= 0 && index < this._names.Count)
					{
						name = name.Replace(Environment.NewLine, "");
						string item = this._names[index];
						this._names[index] = name;
						File.WriteAllLines(this.HashesTxt, this._hashes);
						File.WriteAllLines(this.NamesTxt, this._names);
						this.FileRenamed(this, new RenameEventArgs(index, item, name));
						this.Say(string.Format(this.L.FileRenamed, index, name, item));
					}
				}
			}
			finally
			{
				this._mainMutex.SafeRelease();
			}
		}

		public void Restart()
		{
			if (this.IsBusy())
			{
				return;
			}
			this._sequencer.Start();
		}

		public void RestartTurns()
		{
			this.Queue = this.GetAvalibleUsers();
			this.NextTurn();
		}

		public void RestoreFile(int index)
		{
			try
			{
				this._mainMutex.WaitOne();
				lock (this._names)
				{
					if (index >= 0 && index < this._names.Count)
					{
						if (File.Exists(this.GetFullMidiPath(index)))
						{
							string item = this._names[index];
							if (item.StartsWith("[DELETED]"))
							{
								this._names[index] = item.Remove(0, "[DELETED]".Length);
								File.WriteAllLines(this.NamesTxt, this._names);
								this.Say(string.Format(this.L.FileRestored, index));
							}
						}
						else
						{
							this.Say(string.Format(this.L.NotFound, index));
						}
					}
				}
			}
			finally
			{
				this._mainMutex.SafeRelease();
			}
		}

		public void Say(string message)
		{
			bool flag = (!this.SendChat ? true : !this.Client.IsConnected());
			this.Said(this, new SayMessageEventArgs(message, flag));
			if (flag || !this.Client.Channel.Settings.Chat)
			{
				DateTime now = DateTime.Now;
				this.LogChat(string.Format(string.Concat(now.ToString("yyyy.MM.dd HH:mm:ss"), "|||", this.L.ConsoleOnly), message));
				return;
			}
			string str = message.Trim();
			if (str.Length > 0 && str[0] == '/')
			{
				message = string.Concat(":", message);
			}
			this.Client.Say(message);
			Thread.Sleep(50);
		}

		public void SayPosition()
		{
			if (this._sequence.GetLength() > 0)
			{
				string currentPosition = this.L.CurrentPosition;
				double position = (double)(100 * this._sequencer.get_Position()) / (double)this._sequence.GetLength();
				this.Say(string.Format(currentPosition, position.ToString("N1", CultureInfo.InvariantCulture), this.GetSequenceTimeString(this._sequencer.get_Position()), this.GetSequenceTimeString(this._nowPlayingLength)));
			}
		}

		public void SayWhoIsMaster()
		{
			switch (this.TurnState)
			{
				case NMPB.TurnState.Disabled:
				{
					this.Say(this.L.TurnsDisabled);
					return;
				}
				case NMPB.TurnState.Master:
				case NMPB.TurnState.Song:
				{
					if (this.Master == null)
					{
						break;
					}
					this.Say(string.Format(this.L.CurrentTurnInfo, this.GetUserFullName(this.Master), this.GetNextMaster()));
					return;
				}
				case NMPB.TurnState.LongSong:
				{
					if (this.Master == null || this.OldMaster == null)
					{
						break;
					}
					this.Say(string.Format(this.L.LongTrackTurnInfo, this.GetUserFullName(this.OldMaster), this.GetUserFullName(this.Master)));
					break;
				}
				default:
				{
					return;
				}
			}
		}

		public void Search(string s, int index, string command)
		{
			int i;
			lock (this._names)
			{
				index = Math.Max(index - 1, 0);
				int num = index * 5;
				s = s.ToLowerInvariant();
				List<int> nums = this.SearchForName(s);
				SayAggregator sayAggregator = new SayAggregator(this);
				if (nums.Count == 0)
				{
					sayAggregator.Say(this.L.NotFound);
				}
				for (i = num; i < num + 5; i++)
				{
					if (i < nums.Count && i >= 0)
					{
						sayAggregator.Say(this.TrackName(nums[i]));
					}
				}
				int count = nums.Count - i;
				if (count == 1)
				{
					sayAggregator.Say(this.TrackName(nums[i]));
				}
				if (count > 1)
				{
					sayAggregator.Say(string.Format(this.L.SearchNext, new object[] { count, s, index + 2, command }));
				}
				sayAggregator.Commit();
				if (index == 0)
				{
					this.ShowTip(s);
				}
			}
		}

		private List<int> SearchForName(string s)
		{
			return this._names.Select((string v, int i) => new { i = i, v = v }).Where((t) => {
				if (t.v.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) < 0)
				{
					return false;
				}
				return !t.v.StartsWith("[DELETED]");
			}).Select((t) => t.i).ToList<int>();
		}

		private bool SelectMaster()
		{
			User user;
			bool flag;
			if (this.GetAvalibleUsers().Count <= 0)
			{
				this.TurnState = NMPB.TurnState.Disabled;
				return false;
			}
			int num = 0;
			int count = this.Queue.Count;
			do
			{
				flag = false;
				if (this.Queue.Count <= 0)
				{
					return false;
				}
				user = this.Queue.First<User>();
				this.Queue.RemoveAt(0);
				if (user.Online && !this.Banned.Contains(user.Auid))
				{
					this.Enqueue(user);
					flag = true;
				}
				int num1 = num;
				num = num1 + 1;
				if (num1 < count)
				{
					continue;
				}
				return false;
			}
			while (user.Afk || user.Skip || !flag);
			this.Master = user;
			this.Master.ShouldEnd = true;
			return true;
		}

		public void SendGTFO()
		{
			this.Client.StartNote("GTFO", 1, null);
		}

		private void SetChannel(bool? visible = null, bool? chat = null, bool? solo = null, Color? color = null)
		{
			bool flag;
			bool flag1;
			bool flag2;
			Player client = this.Client;
			bool? nullable = visible;
			flag = (nullable.HasValue ? nullable.GetValueOrDefault() : this._roomIsVisible);
			nullable = chat;
			flag1 = (nullable.HasValue ? nullable.GetValueOrDefault() : this._roomHasChat);
			nullable = solo;
			flag2 = (nullable.HasValue ? nullable.GetValueOrDefault() : this._roomSoloPlay);
			Color? nullable1 = color;
			client.SetChannelSettings(new ChannelSettings(false, flag, flag1, flag2, NamedColor.ToHex((nullable1.HasValue ? nullable1.GetValueOrDefault() : this._roomColor))));
		}

		private void SetLimitedValue(LimitedValue value, string[] args, bool admin, string phrase)
		{
			int num;
			if ((int)args.Length != 1 && !string.IsNullOrWhiteSpace(args[1]) && int.TryParse(args[1], out num))
			{
				value.Set(num, admin);
			}
			this.Say(string.Format(phrase, value));
		}

		public void SetPosition(double x)
		{
			x = Math.Max(0, Math.Min(100, x));
			if (double.IsNaN(x) || this._status.HasFlag(BotStatus.Loading))
			{
				return;
			}
			int length = this._sequence.GetLength();
			this._sequencer.set_Position((int)Math.Round((double)length * x / 100));
			this.TrackPositionSet(this, new TrackPositionEventArgs(x, this._sequencer.get_Position(), length));
		}

		public bool ShouldMinimizeChat()
		{
			if (!this.Client.get_ConnectedToRoom())
			{
				return false;
			}
			return !this.Client.IsOwner();
		}

		private void ShowBanned()
		{
			ExtensionMethods.ForEach<string>(this.Banned, new Action<string>(this.LogChat));
		}

		private void ShowCustomHelp(SayAggregator say)
		{
			foreach (KeyValuePair<string, HashSet<string>> avalibleCommandsSet in this.AvalibleCommandsSet)
			{
				if (this.Turns && avalibleCommandsSet.Key == "No Turns" || !this.Turns && avalibleCommandsSet.Key == "Turns" || !avalibleCommandsSet.Value.Any<string>())
				{
					continue;
				}
				StringBuilder stringBuilder = (new StringBuilder(avalibleCommandsSet.Key)).Append(": ");
				foreach (string value in avalibleCommandsSet.Value)
				{
					stringBuilder.Append(string.Format("/{0} ", value));
				}
				say.Say(stringBuilder.ToString());
			}
		}

		private void ShowTip(string s)
		{
			if (s.Equals("death waltz", StringComparison.OrdinalIgnoreCase))
			{
				this.Say(this.L.LBSFS);
			}
		}

		private void ShowUsers()
		{
			ExtensionMethods.ForEach<User>(
				from user in this.Users
				where user.Online
				select user, (User user) => this.LogChat(string.Concat(user.Auid, ": ", user.ToString(true))));
		}

		private void SkipOrNot()
		{
			if (this.TurnState == NMPB.TurnState.Disabled)
			{
				return;
			}
			if (this.TurnState == NMPB.TurnState.LongSong)
			{
				return;
			}
			if (this.TurnState == NMPB.TurnState.Master && !this.Playing)
			{
				this.MissedTurn();
				return;
			}
			this.RemoveMasterControl();
		}

		public void Stop()
		{
			this._sequencer.Stop();
		}

		private void StopTurns()
		{
			if (this._songDelayedThread != null)
			{
				this._songDelayedThread.Cancel();
			}
			if (this._longSongDelayedThread != null)
			{
				this._longSongDelayedThread.Cancel();
			}
			this.TurnState = NMPB.TurnState.Disabled;
		}

		private void SwitchToLongSong()
		{
			if (this.TurnState == NMPB.TurnState.Disabled)
			{
				return;
			}
			this.TurnState = NMPB.TurnState.LongSong;
			this.OldMaster = this.Master;
			if (!this.SelectMaster())
			{
				return;
			}
			this.Say(string.Format(this.L.LongTrack, this.GetUserFullName(this.Master), this.GetUserFullName(this.OldMaster)));
		}

		private void TooMany()
		{
			string giveMeCrownAndSustain;
			if (this._tooManylocker)
			{
				return;
			}
			this._tooManylocker = true;
			this.NoteLimitExceeded(this, new EventArgs());
			this.Stop();
			this.Say(this.L.TooMany);
			if (!this.Client.IsOwner() || this.Sustain)
			{
				if (this.Client.IsOwner() || !this.Sustain)
				{
					giveMeCrownAndSustain = (this.Sustain ? this.L.EnableSustain : this.L.GiveMeCrown);
				}
				else
				{
					giveMeCrownAndSustain = this.L.GiveMeCrownAndSustain;
				}
				this.Say(giveMeCrownAndSustain);
			}
			Thread.Sleep(2000);
			this._tooManylocker = false;
		}

		private string TrackName(int index)
		{
			return string.Format(this.L.TrackName, index, this._names[index]);
		}

		private bool TurnSequencer(User user)
		{
			switch (this.TurnState)
			{
				case NMPB.TurnState.Disabled:
				{
					return true;
				}
				case NMPB.TurnState.Master:
				case NMPB.TurnState.LongSong:
				{
					return (object)user == (object)this.Master;
				}
				case NMPB.TurnState.Song:
				{
					return false;
				}
			}
			return false;
		}

		private void TurnsOnTrackStop()
		{
			NMPB.TurnState turnState = this.TurnState;
			if (turnState == NMPB.TurnState.Song)
			{
				this.PickNewMaster();
				return;
			}
			if (turnState != NMPB.TurnState.LongSong)
			{
				return;
			}
			this.AfterPickMaster();
		}

		private bool TurnStop(User user)
		{
			switch (this.TurnState)
			{
				case NMPB.TurnState.Disabled:
				{
					return true;
				}
				case NMPB.TurnState.Master:
				case NMPB.TurnState.Song:
				{
					return (object)user == (object)this.Master;
				}
				case NMPB.TurnState.LongSong:
				{
					if ((object)user == (object)this.Master)
					{
						return true;
					}
					return (object)user == (object)this.OldMaster;
				}
			}
			return false;
		}

		private void UserAlive(User user)
		{
			if (user.Afk)
			{
				user.Afk = false;
				this.Enqueue(user);
			}
		}

		private void UserNotSkipped(User user)
		{
			if (user.Skip)
			{
				user.Skip = false;
				this.Enqueue(user);
			}
		}

		private bool VerifyData(string originalMessage, string signature, RSAParameters publicKey)
		{
			bool flag = false;
			using (RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider())
			{
				try
				{
					try
					{
						byte[] bytes = (new UTF8Encoding()).GetBytes(originalMessage);
						byte[] numArray = Convert.FromBase64String(signature);
						rSACryptoServiceProvider.ImportParameters(publicKey);
						flag = rSACryptoServiceProvider.VerifyData(bytes, CryptoConfig.MapNameToOID("SHA512"), numArray);
					}
					catch (Exception exception)
					{
					}
				}
				finally
				{
					rSACryptoServiceProvider.PersistKeyInCsp = false;
				}
			}
			return flag;
		}

		private void VoteSkip(User user)
		{
			if (this.TurnState != NMPB.TurnState.LongSong)
			{
				this.Say(this.L.VoteSkipNotAvalible);
				return;
			}
			List<User> queueAvalibleUsers = this.GetQueueAvalibleUsers();
			bool voted = !user.Voted;
			this.Say(string.Format((voted ? this.L.UserWantsSkip : this.L.UserDoesntWantSkip), user.Name));
			user.Voted = voted;
			int num = queueAvalibleUsers.Count<User>((User user1) => user1.Voted);
			int num1 = (int)Math.Round((double)queueAvalibleUsers.Count * 2 / 3);
			this.Say(string.Format(this.L.Voted, num, num1));
			if (num >= num1)
			{
				this.SayPosition();
				this.Stop();
			}
		}

		private void WaitSequencerStop()
		{
			while (this.Playing)
			{
				Thread.Sleep(10);
			}
		}

		private void Welcome(object sender, UserBaseEventArgs args)
		{
			this.LogChat(string.Format(this.L.UserEntred, new object[] { this.DateNowString(), args.get_User().Name, args.get_User().Color, args.get_User().Auid }));
			User user = this.FindUserByBase(args.get_User());
			if (user != null)
			{
				user.UpdateFromBase(args.get_User());
			}
			else
			{
				List<User> users = this.Users;
				User user2 = new User(args.get_User(), this.GetNearestColorName(args.get_User().Color));
				User user3 = user2;
				user = user2;
				users.Add(user3);
				if (this.Users.Count > this.MaxUserListSize)
				{
					this.Users.RemoveAll((User user1) => !user1.Online);
				}
			}
			this.UserEntered(this, new UserEventArgs(user));
			if (!this.AllowOtherBots)
			{
				DelayedTask delayedTask = new DelayedTask(new Action(this.SendGTFO), 1000);
			}
			if (this.Banned.Contains(user.Auid))
			{
				return;
			}
			if (this.Turns)
			{
				this.Enqueue(user);
			}
			if ((object)this._lastGreetedUser == (object)user)
			{
				return;
			}
			if ((DateTime.Now - this._lastHi) <= TimeSpan.FromMinutes(2))
			{
				return;
			}
			if (this.AvalibleCommandsType < NMPB.AvalibleCommandsType.TextOnly || !this.WelcomeNewUsers)
			{
				return;
			}
			this._lastHi = DateTime.Now;
			this._lastGreetedUser = user;
			DelayedTask delayedTask1 = new DelayedTask(() => this.Say(string.Format(this.L.Welcome, user.ToString(true))), 3000);
		}

		public event EventHandler<UserEventArgs> BotUserUpdated;

		public event EventHandler<TextMessageEventArgs> ChatLogged;

		public event EventHandler<ChatMessageEventArgs> ChatMessageReceived;

		public event EventHandler ConnectionBroken;

		public event EventHandler<FileDownloadEventArgs> FileDownloaded;

		public event EventHandler<DownloadFileErrorEventArgs> FileDownloadFailed;

		public event EventHandler<LoadFileEventArgs> FileLoaded;

		public event EventHandler<RenameEventArgs> FileRenamed;

		public event EventHandler<ChannelMessageEventArgs> MidiMessagePlayed;

		public event EventHandler<NamesEventArgs> NamesUpdated;

		public event EventHandler NoteLimitExceeded;

		public event EventHandler<ChannelMessageEventArgs> NotePlayed;

		public event EventHandler PlayingCompleted;

		public event EventHandler<SayMessageEventArgs> Said;

		public event EventHandler<StoppedEventArgs> SequenserStopped;

		public event EventHandler<TrackPositionEventArgs> TrackPositionSet;

		public event EventHandler<TurnsEventArgs> TurnsStateChanged;

		public event EventHandler<UserEventArgs> UserEntered;

		public event EventHandler<UserEventArgs> UserLeft;

		public event EventHandler<UserEventArgs> UserMouseMoved;

		public event EventHandler<UserEventArgs> UserNameChanged;

		public event EventHandler<UserNoteBufferEventArgs> UserNotePlayed;
	}
}