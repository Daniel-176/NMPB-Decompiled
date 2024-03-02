using Newtonsoft.Json;
using System;

namespace NMPB.Client
{
	public class CrownPoint
	{
		[JsonProperty("x", DefaultValueHandling=DefaultValueHandling.IgnoreAndPopulate)]
		public double X;

		[JsonProperty("y", DefaultValueHandling=DefaultValueHandling.IgnoreAndPopulate)]
		public double Y;

		public CrownPoint(double x, double y)
		{
			this.X = x;
			this.Y = y;
		}
	}
}