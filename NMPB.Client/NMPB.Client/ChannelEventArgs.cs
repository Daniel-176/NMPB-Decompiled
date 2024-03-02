using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NMPB.Client
{
	public class ChannelEventArgs : EventArgs
	{
		public ChannelInfo Channel
		{
			get;
			private set;
		}

		public string ParticipantId
		{
			get;
			private set;
		}

		public List<UserBase> UpdatedUsers
		{
			get;
			private set;
		}

		public List<UserBase> UpdatingUsers
		{
			get;
			private set;
		}

		public ChannelEventArgs(ChannelInfo channel, string participantId, List<UserBase> updatingUsers, List<UserBase> updatedUsers)
		{
			this.Channel = channel;
			this.ParticipantId = participantId;
			this.UpdatingUsers = updatingUsers;
			this.UpdatedUsers = updatedUsers;
		}
	}
}