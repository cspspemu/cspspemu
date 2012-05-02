using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpUtils
{
	class TimeSampler
	{
		Dictionary<DateTime, double> Samples = new Dictionary<DateTime, double>();

		static public TimeSpan Measure(Action Action)
		{
			var Start = DateTime.UtcNow;
			Action();
			var End = DateTime.UtcNow;
			return End - Start;
		}

		public void AddAt(DateTime DateTime, double Sample)
		{
			Samples[DateTime] = Sample;
		}

		public void AddNow(double Sample)
		{
			AddAt(DateTime.UtcNow, Sample);
		}

		public double GetInterpolatedSampleAt(DateTime Time)
		{
			throw(new NotImplementedException());
		}

		public DateTime GetNearestDateTimeAt(DateTime Time)
		{
			return Samples
				.OrderBy(Item => (Time - Item.Key).Duration())
				.First()
				.Key
			;
		}

		/*
		public DateTime GetLowerDateTimeAt(DateTime Time)
		{
			return Samples
				.Where(Item => Item.Key <= Time)
				.OrderByDescending(Item => Item.Key)
				.First()
				.Key
			;
		}

		public DateTime GetUpperDateTimeAt(DateTime Time)
		{
			return Samples
				.Where(Item => Item.Key >= Time)
				.OrderBy(Item => Item.Key)
				.First()
				.Key
			;
		}*/

		public double GetSampleAt(DateTime Time)
		{
			return Samples[Time];
		}

		public double GetIncrementPerSecond(TimeSpan TimeSpan, DateTime DateTimeNow)
		{
			//var Time1 = GetLowerDateTimeAt(DateTimeNow - TimeSpan);
			//var Time2 = GetLowerDateTimeAt(DateTimeNow);
			var Time1 = GetNearestDateTimeAt(DateTimeNow - TimeSpan);
			var Time2 = GetNearestDateTimeAt(DateTimeNow);

			var Sample1 = GetSampleAt(Time1);
			var Sample2 = GetSampleAt(Time2);

			return (Sample2 - Sample1) / (Time2 - Time1).TotalSeconds;
		}

		public double GetIncrementPerSecond(TimeSpan TimeSpan)
		{
			return GetIncrementPerSecond(TimeSpan, DateTime.UtcNow);
		}

		static public double Difference(double A, double B)
		{
			return (B - A);
		}

		static public double Interpolate(double A, double B, double Step)
		{
			return Difference(B, A) * Step + A;
		}
	}
}
