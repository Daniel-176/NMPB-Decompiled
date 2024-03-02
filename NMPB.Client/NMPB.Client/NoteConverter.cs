using System;

namespace NMPB.Client
{
	public static class NoteConverter
	{
		public static string[] Notes;

		private readonly static string[] Letters;

		static NoteConverter()
		{
			NoteConverter.Letters = new string[] { "c", "cs", "d", "ds", "e", "f", "fs", "g", "gs", "a", "as", "b" };
			NoteConverter.Notes = new string[128];
			for (int i = 0; i < 11; i++)
			{
				for (int j = 0; j < 12; j++)
				{
					if (12 * i + j >= 128)
					{
						return;
					}
					NoteConverter.Notes[12 * i + j] = string.Concat(NoteConverter.Letters[j], i - 2);
				}
			}
		}
	}
}