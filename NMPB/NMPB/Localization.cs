using System;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;

namespace NMPB
{
	public class Localization
	{
		public string HelpFileName = "Help.txt";

		public string ColorsFileName = "Colors.txt";

		public string CultureInfoName = "en-US";

		public string ConnectionError = "{0} | SYSTEM INFO: Connection error: {1}";

		public string Motd = "{0} | SYSTEM INFO: Motd: {1}";

		public string SystemInfoConnected = "{0} | SYSTEM INFO: Bot connected. Protocol version {1}.";

		public string SystemInfoRoom = "{0} | SYSTEM INFO: Room: {1}.";

		public string UserEntred = "{0} | SYSTEM INFO: User entred. Name: {1}, Color: {2}, Auid: {3}.";

		public string UserLeft = "{0} | SYSTEM INFO: User left. Name: {1}, Color: {2}, Auid: {3}.";

		public string ConnectionDenied = "{0} | SYSTEM INFO: Connection denied by room owner.";

		public string Disconnected = "{0} | SYSTEM INFO: Disconnected.";

		public string ReadingFile = "Reading file {0} [{1}].{2}";

		public string ReadingFileException = "Unable parse the file. Is it a midi? {0}";

		public string ShutDown = "Shutting down...";

		public string CurrentPosition = "Current position: {0}% [{1} / {2}].";

		public string NoFileLoaded = "No file loaded. Use /auto [id] to load file.";

		public string BotIsBusy = "Bot is busy. Status: {0}.";

		public string ConsoleOnly = "Console only: {0}";

		public string RoomNameException = "Unable to connect to the lobby. Try different room name.";

		public string TrackTimeTemplate = "{0:00}:{1:00}";

		public string Backwards = " [Backwards]";

		public string TooMany = "Oops... Looks like this track is too hard for this site. Type /p to continue the song.";

		public string GiveMeCrownAndSustain = "To increase note quota give me the crown and enable /sustain.";

		public string EnableSustain = "To increase note quota enable /sustain.";

		public string GiveMeCrown = "To increase note quota give me the crown.";

		public string LBSFS = "Do you mean \"Last Brutal Sister Flandre Scarlet\"?";

		public string Console = "Console: {0}";

		public string NotFound = "Not found.";

		public string NowPlaying = "Now playing: {0}.";

		public string ImposterDetected = "Imposter Detected. You are a fake {0}.";

		public string BannedAnswer = "Nope.";

		public string TurnsMode = "It's turns mode.";

		public string Commands = "Commands: ";

		public string Mute = "Click on the spammer's name to mute him.";

		public string TurnInfoNever = "{0}'s turn never happens. Write /skipme to get back into turns.";

		public string UserTurnInfo = "{0}'s turn is after {1} turn(s).";

		public string AFK = "{0} was marked as AFK. Send any message to be unmarked.";

		public string NoMoreSkips = "{0} will no longer be skipped.";

		public string AutoTrackSet = "This track will be played on {0}'s turn: {1}.";

		public string SelectedTrack = "Selected track: {0}.";

		public string NoTrackSelected = "No track selected.";

		public string SkipMe = "{0} will be skipped until typing /auto or /skipme again.";

		public string UserColorInfo = "{1}'s color: {0} ({2}).";

		public string YourColorInfo = "Your color: {0}.";

		public string YouCanOnlyStop = "You can only /stop the track.";

		public string MistakesDisabled = "Mistakes disabled.";

		public string MistakesEnabled = "Mistakes enabled. Period: {0}. Maximum Delay: {1}.";

		public string NoHelpForCommand = "There is no help for the command: {0}.";

		public string SustainEnabled = "Auto-sustain is enabled.";

		public string SustainDisabled = "Auto-sustain is disabled.";

		public string InversionEnabled = "Inversion is enabled.";

		public string InversionDisabled = "Inversion is disabled.";

		public string Tempo = "Tempo: {0}% [{1}]";

		public string PreserveQuotaEnabled = "PreserveQuota mode is enabled.";

		public string PreserveQuotaDisabled = "PreserveQuota mode is disabled.";

		public string TrackbarEnabled = "Trackbar is enabled.";

		public string TrackbarDisabled = "Trackbar is disabled.";

		public string UniqueNameFound = "Found shortest unique name{1}: {0}";

		public string UniqueNameNotFound = "No unique name exists.";

		public string HorribleBug = "A horrible bug was here, but it is fixed now.";

		public string OctaveModeEnabled = "Octave mode is enabled.";

		public string OctaveModeDisabled = "Octave mode is disabled.";

		public string OctaveJumpModeEnabled = "Octave jump: 2.";

		public string OctaveJumpModeDisabled = "Octave jump: 1.";

		public string EchoModeEnabled = "Echo mode is enabled.";

		public string EchoModeDisabled = "Echo mode is disabled.";

		public string DelayEchoMode = "Echo delay: {0} ms.";

		public string OctaveNumber = "Number of octaves: {0}.";

		public string EchoNumber = "Number of echos: {0}.";

		public string TransposeNote = "Transpose: {0} semitone(s).";

		public string EchoDamingEffect = "Echo fade: {0}%.";

		public string ReflectionFail = "Fail: {0}";

		public string ReflectionMemberNotFound = "Member '{0}' is not found.";

		public string ReflectionArgumentFail = "Unable to parse argument '{2}' for {0}. Value must be {1}.";

		public string ReflectionTypeNotSupported = "Required type {0} is not supported.";

		public string DLTooLong = "Unable to download. It takes too long to download this file.";

		public string UnableToDownload = "Unable to download. {0}";

		public string UnableToDownload01 = "Unable to download. {0} {1}";

		public string Downloading = "Downloading...";

		public string IncorrectURL = "Incorrect URL.";

		public string FileTooBig = "File is too big. It's almost certainly not a midi file.";

		public string UnableToFetfilesize = "Unable to get filesize. It's not a direct link.";

		public string DLNoResponse = "Unable to download. No response was received during the time-out period for a request.";

		public string FileSaved = "File saved. ID: {0}. Name: {1}.";

		public string FileFound = "File found. ID: {0}. Name: {1}.";

		public string UseAutoToLoad = "Use /auto {0} to load this track.";

		public string FileRenamed = "File {0} renamed to: {1}. Old name: {2}.";

		public string HashesDoNotMatch = "Hashes do not match.";

		public string FileDeleted = "File {0} deleted.";

		public string FileRemoved = "File {0} removed.";

		public string FileRestored = "File {0} restored.";

		public string FileAdded = "File added: {0}.";

		public string UnableToAddFile = "Unable to add file. File is already exists: {0}";

		public string ListInfo = "Use /list [id] to start from another id. Total: {0}.";

		public string SearchNext = "There are {0} more results. Type /{3} {2} or /{3} {1} {2} to continue.";

		public string UnableToLoadDeleted = "Unable to load {0}: {1}. File is deleted.";

		public string TrackName = "Id: {0}. Name: {1}";

		public string WorkingDirectory = "Working directory: {0}";

		public string Welcome = "Welcome, {0}. Use /help for the command list.";

		public string TurnsEnabled = "Turns are enabled.";

		public string TurnsDisabled = "Turns are disabled.";

		public string TurnNow = "It is now {0}'s turn. You have {1} seconds to pick a track.";

		public string TimeUp = "{1} seconds is up. Now, {0} can only /stop the track. Next: {2}.";

		public string LongTrack = "Track looks pretty long. {0} can take the controls now. {1} can still /stop this track. Everyone else can /voteskip it.";

		public string SkipTurn = "{0}'s turn was skipped and he is now marked as AFK.";

		public string MasterLeaves = "{0} leaves.";

		public string CanTakeControl = "{0} can take control now.";

		public string CurrentTurnInfo = "It's {0}'s turn. Next: {1}.";

		public string LongTrackTurnInfo = "It's {0}'s turn. But {1} can take control and everyone else can /voteskip it.";

		public string VoteSkipNotAvalible = "Voteskip is only available after three minutes of playing the current track.";

		public string UserWantsSkip = "{0} wants to skip this track.";

		public string UserDoesntWantSkip = "{0} doesn't want to skip this track.";

		public string Voted = "Voted: {0}. Required: {1}.";

		public string WrongColor = "Wrong color";

		public string AnyColor = "Any color";

		public string OrIfUserBack = ". Or {0}, if he comes back";

		public string PersonalSustainOn = "{0}'s auto-sustain is turned on.";

		public string PersonalSustainOff = "{0}'s auto-sustain is turned off.";

		public string PersonalSustainDisabled = "{0}'s auto-sustain is disabled.";

		public string UsersInQueue = "Users in queue: {0}.";

		[XmlIgnore]
		public System.Globalization.CultureInfo CultureInfo;

		public const string LocalizationFolder = "localization";

		public string ColorsFilePath
		{
			get
			{
				char directorySeparatorChar = Path.DirectorySeparatorChar;
				return string.Concat("localization", directorySeparatorChar.ToString(), this.ColorsFileName);
			}
		}

		public string HelpFilePath
		{
			get
			{
				char directorySeparatorChar = Path.DirectorySeparatorChar;
				return string.Concat("localization", directorySeparatorChar.ToString(), this.HelpFileName);
			}
		}

		public Localization()
		{
			this.CultureInfo = new System.Globalization.CultureInfo(this.CultureInfoName);
		}

		public static Localization Load(string name)
		{
			Localization localization;
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(Localization));
			using (TextReader streamReader = new StreamReader(name))
			{
				Localization cultureInfo = (Localization)xmlSerializer.Deserialize(streamReader);
				streamReader.Close();
				cultureInfo.CultureInfo = new System.Globalization.CultureInfo(cultureInfo.CultureInfoName);
				localization = cultureInfo;
			}
			return localization;
		}

		public void Save(string name)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(Localization));
			using (TextWriter streamWriter = new StreamWriter(name))
			{
				xmlSerializer.Serialize(streamWriter, this);
				streamWriter.Close();
			}
		}
	}
}