using System;
using System.Text;

namespace NMPB
{
	public class SayAggregator
	{
		private StringBuilder _sb;

		private Bot _bot;

		public SayAggregator(Bot bot)
		{
			this._bot = bot;
		}

		public void Commit()
		{
			if (this._sb != null)
			{
				while (this._sb.Length > 500)
				{
					this._bot.Say(this._sb.ToString(0, 500));
					this._sb.Remove(0, 500);
				}
				this._bot.Say(this._sb.ToString());
			}
		}

		public void Say(string message)
		{
			if (this._sb == null && !this._bot.ShouldMinimizeChat())
			{
				this._bot.Say(message);
				return;
			}
			if (this._sb == null)
			{
				this._sb = new StringBuilder(message);
				return;
			}
			this._sb.Append(" | ");
			this._sb.Append(message);
		}
	}
}