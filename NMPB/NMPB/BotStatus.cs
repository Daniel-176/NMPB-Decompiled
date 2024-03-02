using System;

namespace NMPB
{
	[Flags]
	public enum BotStatus
	{
		Ready,
		Loading,
		Downloading
	}
}