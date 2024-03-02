using System;
using System.Drawing;

namespace NMPB
{
	internal class NamedColor
	{
		internal string Name;

		internal System.Drawing.Color Color;

		internal NamedColor(string name, System.Drawing.Color color)
		{
			this.Name = name;
			this.Color = color;
		}

		internal static System.Drawing.Color FromHtmlSafe(string color)
		{
			System.Drawing.Color transparent;
			try
			{
				transparent = ColorTranslator.FromHtml(color);
			}
			catch (Exception exception)
			{
				transparent = System.Drawing.Color.Transparent;
			}
			return transparent;
		}

		internal static string ToHex(System.Drawing.Color c)
		{
			byte r = c.R;
			string str = r.ToString("X2");
			r = c.G;
			string str1 = r.ToString("X2");
			r = c.B;
			return string.Concat("#", str, str1, r.ToString("X2"));
		}
	}
}