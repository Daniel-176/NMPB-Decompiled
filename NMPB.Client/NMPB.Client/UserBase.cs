using Newtonsoft.Json;
using System;

namespace NMPB.Client
{
	public class UserBase
	{
		[JsonProperty("_id")]
		public string Auid;

		[JsonProperty("id")]
		public string Id;

		[JsonProperty("name")]
		public string Name;

		[JsonProperty("color")]
		public string Color;

		[JsonProperty("x", DefaultValueHandling=DefaultValueHandling.IgnoreAndPopulate)]
		private double? _x;

		[JsonProperty("y", DefaultValueHandling=DefaultValueHandling.IgnoreAndPopulate)]
		private double? _y;

		public const string DefaultName = "Anonymous";

		public const string DefaultColor = "#ffff00";

		public const string DefaultId = "No";

		[JsonIgnore]
		public double X
		{
			get
			{
				double? nullable = this._x;
				if (!nullable.HasValue)
				{
					return 0;
				}
				return nullable.GetValueOrDefault();
			}
			set
			{
				this._x = new double?(value);
			}
		}

		[JsonIgnore]
		public double Y
		{
			get
			{
				double? nullable = this._y;
				if (!nullable.HasValue)
				{
					return 0;
				}
				return nullable.GetValueOrDefault();
			}
			set
			{
				this._y = new double?(value);
			}
		}

		public UserBase(string auid = "No", string id = "No", string name = "Anonymous", string color = "#ffff00")
		{
			this.Auid = auid ?? "No";
			this.Id = id ?? "No";
			this.Name = name ?? "Anonymous";
			this.Color = color ?? "#ffff00";
		}
	}
}