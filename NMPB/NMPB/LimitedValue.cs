using System;

namespace NMPB
{
	public class LimitedValue
	{
		private int _value;

		public int Min;

		public int Max;

		public int MinAdmin;

		public int MaxAdmin;

		public int Value
		{
			get
			{
				return this._value;
			}
			set
			{
				this._value = LimitedValue.Clamp(value, this.MinAdmin, this.MaxAdmin);
			}
		}

		public LimitedValue(int min, int max, int minAdmin, int maxAdmin, int initialValue = 0)
		{
			this.Min = min;
			this.Max = max;
			this.MinAdmin = minAdmin;
			this.MaxAdmin = maxAdmin;
			this.Value = initialValue;
		}

		public static int Clamp(int value, int min, int max)
		{
			return Math.Min(max, Math.Max(min, value));
		}

		public static implicit operator Int32(LimitedValue val)
		{
			return val.Value;
		}

		public void Set(int value, bool isAdmin)
		{
			if (isAdmin)
			{
				this.Value = value;
				return;
			}
			this._value = LimitedValue.Clamp(value, this.Min, this.Max);
		}

		public override string ToString()
		{
			return this._value.ToString();
		}
	}
}