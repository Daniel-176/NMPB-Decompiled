using NMPB.Client;
using System;

namespace NMPB
{
	public class User : UserBase
	{
		public string ColorName;

		public int Next = -1;

		public bool Voted;

		public bool ShouldEnd;

		public bool Afk;

		public bool Skip;

		public bool Online;

		public string LastSearch;

		public bool? Sustain;

		public const string DefaultColorName = "Yellow";

		public bool Avaliable
		{
			get
			{
				if (!this.Online || this.Afk)
				{
					return false;
				}
				return !this.Skip;
			}
		}

		public User(UserBase user, string colorName = "Yellow") : base("No", "No", "Anonymous", "#ffff00")
		{
			this.UpdateFromBase(user);
			this.ColorName = colorName ?? "Yellow";
		}

		public User(string auid = "No", string id = "No", string name = "Anonymous", string color = "#ffff00", string colorName = "Yellow") : base(auid, id, name, color)
		{
			this.ColorName = colorName ?? "Yellow";
		}

		public string ToString(bool addColor = false)
		{
			if (!addColor)
			{
				return this.Name;
			}
			return string.Format("{0} ({1})", this.Name, this.ColorName);
		}

		public void UpdateFromBase(UserBase user)
		{
			this.Auid = user.Auid;
			this.Id = user.Id;
			this.Name = user.Name;
			this.Color = user.Color;
			base.set_X(user.get_X());
			base.set_Y(user.get_Y());
			this.Online = true;
		}
	}
}