using Sanford.Multimedia.Midi;
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NMPB
{
	internal class Sustain
	{
		private const int MidiChannelMax = 16;

		private const int NoteValueMax = 128;

		private readonly bool[,] _hold;

		private readonly bool[,] _sound;

		private readonly bool[] _sustain;

		public Sustain()
		{
			this._hold = new bool[16, 128];
			this._sound = new bool[16, 128];
			this._sustain = new bool[16];
		}

		private void NoteOff(int channel, int note)
		{
			this._hold[channel, note] = false;
			if (this._sound[channel, note])
			{
				if (!this._sustain[channel])
				{
					this._sound[channel, note] = false;
				}
				this.TryRelease(note);
			}
		}

		private void NoteOn(int channel, int note)
		{
			this._sound[channel, note] = true;
			this._hold[channel, note] = true;
		}

		private void PedalPress(int channel)
		{
			this._sustain[channel] = true;
		}

		private void PedalRelease(int channel)
		{
			this._sustain[channel] = false;
			for (int i = 0; i < 128; i++)
			{
				if (this._sound[channel, i] && !this._hold[channel, i])
				{
					this._sound[channel, i] = false;
					this.TryRelease(i);
				}
			}
		}

		public void Process(ChannelMessage msg)
		{
			if (msg.MidiChannel == 9)
			{
				return;
			}
			ChannelCommand command = msg.Command;
			if (command == ChannelCommand.NoteOff)
			{
				this.NoteOff(msg.MidiChannel, msg.Data1);
				return;
			}
			if (command == ChannelCommand.NoteOn)
			{
				if (msg.Data2 > 0)
				{
					this.NoteOn(msg.MidiChannel, msg.Data1);
					return;
				}
				this.NoteOff(msg.MidiChannel, msg.Data1);
				return;
			}
			if (command != ChannelCommand.Controller)
			{
				return;
			}
			if (msg.Data1 != 64)
			{
				return;
			}
			if (msg.Data2 >= 64)
			{
				this.PedalPress(msg.MidiChannel);
				return;
			}
			this.PedalRelease(msg.MidiChannel);
		}

		private void TryRelease(int note)
		{
			for (int i = 0; i < 16; i++)
			{
				if (this._sound[i, note])
				{
					return;
				}
			}
			this.NoteReleased(this, new NoteReleasedEventArgs(note));
		}

		public event EventHandler<NoteReleasedEventArgs> NoteReleased;
	}
}