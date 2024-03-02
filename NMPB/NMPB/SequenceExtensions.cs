using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NMPB
{
	internal static class SequenceExtensions
	{
		public static TimeSpan GetActualLength(this Sequence sequence, int position = -1)
		{
			Func<MidiEvent, bool> func1 = null;
			int length = position;
			IOrderedEnumerable<MidiEvent> midiEvents1 = sequence.SelectMany<Track, MidiEvent>((Track track) => track.Iterator()).Where<MidiEvent>((MidiEvent midi) => {
				if (!(midi.get_MidiMessage() is MetaMessage))
				{
					return false;
				}
				return ((MetaMessage)midi.get_MidiMessage()).get_MetaType() == 81;
			}).OrderBy<MidiEvent, int>((MidiEvent midi) => midi.get_AbsoluteTicks());
			if (length < 0)
			{
				length = sequence.GetLength();
			}
			double num = 0;
			double division = 500 / (double)sequence.get_Division();
			int num1 = 0;
			foreach (MidiEvent midiEvent in midiEvents1)
			{
				if (midiEvent.get_AbsoluteTicks() <= length)
				{
					num = num + (double)(midiEvent.get_AbsoluteTicks() - num1) * division;
					num1 = midiEvent.get_AbsoluteTicks();
					division = (double)(new TempoChangeBuilder((MetaMessage)midiEvent.get_MidiMessage())).get_Tempo() / (double)sequence.get_Division() / 1000;
				}
				else
				{
					goto Label0;
				}
			}
		Label0:
			num = (length != sequence.GetLength() ? num + (double)(sequence.Max<Track>((Track track) => {
				IEnumerable<MidiEvent> midiEvents = track.Iterator();
				Func<MidiEvent, bool> u003cu003e9_5 = func1;
				if (u003cu003e9_5 == null)
				{
					Func<MidiEvent, bool> absoluteTicks = (MidiEvent midi) => midi.get_AbsoluteTicks() <= length;
					Func<MidiEvent, bool> func = absoluteTicks;
					func1 = absoluteTicks;
					u003cu003e9_5 = func;
				}
				return (midiEvents.LastOrDefault<MidiEvent>(u003cu003e9_5) ?? track.Iterator().First<MidiEvent>()).get_AbsoluteTicks();
			}) - num1) * division : num + (double)(sequence.Max<Track>((Track track) => track.GetMidiEvent(track.get_Count() - 1).get_AbsoluteTicks()) - num1) * division);
			return TimeSpan.FromMilliseconds(num);
		}

		public static Sequence Reverse(this Sequence sequence, bool disposeOld = false)
		{
			List<LinkedList<MidiEvent>> list = (
				from t in sequence
				select new LinkedList<MidiEvent>(t.Iterator())).ToList<LinkedList<MidiEvent>>();
			Track track = new Track();
			double absoluteTicks = 0;
			double division = 500 / (double)sequence.get_Division();
			int num = 0;
			while (list.Any<LinkedList<MidiEvent>>())
			{
				int absoluteTicks1 = list[0].First.Value.get_AbsoluteTicks();
				int num1 = 0;
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].First.Value.get_AbsoluteTicks() < absoluteTicks1)
					{
						num1 = i;
						absoluteTicks1 = list[i].First.Value.get_AbsoluteTicks();
					}
				}
				MidiEvent value = list[num1].First.Value;
				if (list[num1].First.Next != null)
				{
					list[num1].RemoveFirst();
				}
				else
				{
					list.RemoveAt(num1);
				}
				MetaMessage midiMessage = value.get_MidiMessage() as MetaMessage;
				if (midiMessage == null)
				{
					track.Insert((int)(absoluteTicks + (double)(value.get_AbsoluteTicks() - num) * division), value.get_MidiMessage());
				}
				else
				{
					if (midiMessage.get_MetaType() != 81)
					{
						continue;
					}
					absoluteTicks = absoluteTicks + (double)(value.get_AbsoluteTicks() - num) * division;
					num = value.get_AbsoluteTicks();
					division = (double)(new TempoChangeBuilder(midiMessage)).get_Tempo() / (double)sequence.get_Division() / 1000;
				}
			}
			if (disposeOld)
			{
				sequence.Dispose();
			}
			int length = track.get_Length();
			Track track1 = new Track();
			TempoChangeBuilder tempoChangeBuilder = new TempoChangeBuilder();
			tempoChangeBuilder.set_Tempo(192000);
			TempoChangeBuilder tempoChangeBuilder1 = tempoChangeBuilder;
			tempoChangeBuilder1.Build();
			track1.Insert(0, tempoChangeBuilder1.get_Result());
			foreach (MidiEvent midiEvent in track.Iterator().Reverse<MidiEvent>())
			{
				track1.Insert(length - midiEvent.get_AbsoluteTicks(), midiEvent.get_MidiMessage());
			}
			Sequence sequence1 = new Sequence(192);
			sequence1.Add(track1);
			return sequence1;
		}

		public static Sequence Trim(this Sequence sequence, bool disposeOld = false)
		{
			int absoluteTicks = 0;
			foreach (Track track in sequence)
			{
				foreach (MidiEvent midiEvent in track.Iterator())
				{
					ChannelMessage midiMessage = midiEvent.get_MidiMessage() as ChannelMessage;
					if (midiMessage == null || midiMessage.get_Command() != 144 && midiMessage.get_Command() != 128 || midiEvent.get_AbsoluteTicks() <= absoluteTicks)
					{
						continue;
					}
					absoluteTicks = midiEvent.get_AbsoluteTicks();
				}
			}
			Sequence sequence1 = new Sequence(sequence.get_Division());
			foreach (Track track1 in sequence)
			{
				Track track2 = new Track();
				foreach (MidiEvent midiEvent1 in track1.Iterator())
				{
					if (midiEvent1.get_AbsoluteTicks() > absoluteTicks)
					{
						continue;
					}
					MetaMessage metaMessage = midiEvent1.get_MidiMessage() as MetaMessage;
					if (metaMessage != null && metaMessage.get_MetaType() == 47)
					{
						continue;
					}
					track2.Insert(midiEvent1.get_AbsoluteTicks(), midiEvent1.get_MidiMessage());
				}
				sequence1.Add(track2);
			}
			if (disposeOld)
			{
				sequence.Dispose();
			}
			return sequence1;
		}
	}
}