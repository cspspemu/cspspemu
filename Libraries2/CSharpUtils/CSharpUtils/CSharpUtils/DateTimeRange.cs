using System;

namespace CSharpUtils
{
	public struct DateTimeRange
	{
		public enum PrecisionType
		{
			Ticks,
			Seconds,
			Minutes,
			Hours,
			Days,
			Months,
			Years,
			/*
			Seconds = 1,
			Minutes = 60,
			Hours   = 60 * 60,
			Days    = 24 * 60 * 60,
			Months  = 30 * 24 * 60 * 60,
			Years   = 365 * 24 * 60 * 60,
				* */
		}

		private bool WasUpdated;
		private DateTime _Time, _TimeStart, _TimeEnd;
		private PrecisionType _Precision;

		static DateTimeRange FromUnixTimestamp(long UnixTimestamp, PrecisionType Precision = PrecisionType.Seconds)
		{
			return new DateTimeRange(ConvertFromUnixTimestamp(UnixTimestamp), Precision);
		}

		public static implicit operator DateTimeRange(long UnixTimestamp)
		{
			return FromUnixTimestamp(UnixTimestamp, PrecisionType.Seconds);
		}

		public DateTimeRange(DateTime Time, PrecisionType Precision = PrecisionType.Ticks)
		{
			this._Time = Time;
			this._Precision = Precision;
			this.WasUpdated = true;
			this._TimeStart = DateTime.MinValue;
			this._TimeEnd = DateTime.MaxValue;
		}

		public DateTime Time
		{
			set
			{
				_Time = value;
				WasUpdated = true;
			}
			get
			{
				return _Time;
			}
		}

		public DateTime TimeStart
		{
			get
			{
				CheckUpdateTimeStartEnd();
				return _TimeStart;
			}
		}

		public DateTime TimeEnd
		{
			get
			{
				CheckUpdateTimeStartEnd();
				return _TimeEnd;
			}
		}

		public long UnixTimestamp
		{
			get
			{
				return ConvertToUnixTimestamp(Time);
			}
		}

		public static DateTime ConvertFromUnixTimestamp(long timestamp)
		{
			DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
			return origin.AddSeconds(timestamp);
		}

		public static long ConvertToUnixTimestamp(DateTime date)
		{
			DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
			TimeSpan diff = date - origin;
			return (long)Math.Floor(diff.TotalSeconds);
		}

		public bool Contains(DateTime PreciseTime)
		{
			return (PreciseTime >= TimeStart) && (PreciseTime < TimeEnd);
		}

		public bool Contains(DateTimeRange that)
		{
			return Contains(that.TimeStart) && Contains(that.TimeEnd - new TimeSpan(1));
		}

		public PrecisionType Precision
		{
			set
			{
				_Precision = value;
				WasUpdated = true;
			}
			get
			{
				return _Precision;
			}
		}

		public static implicit operator DateTime(DateTimeRange that)
		{
			return that.Time;
		}

		public static implicit operator DateTimeRange(DateTime that)
		{
			return new DateTimeRange(that);
		}

		public static bool operator ==(DateTimeRange a, DateTimeRange b)
		{
			return a.Contains(b) || b.Contains(a);
		}

		public static bool operator !=(DateTimeRange a, DateTimeRange b)
		{
			return !(a == b);
		}

		public override int GetHashCode()
		{
			return TimeStart.GetHashCode() ^ Precision.GetHashCode();
		}

		public override bool Equals(object that)
		{
			if (that is DateTimeRange)
			{
				return ((DateTimeRange)this == (DateTimeRange)that);
			}
			else if (that is DateTime)
			{
				return this.Contains((DateTime)that);
			}
			{
				return false;
			}
		}

		private void CheckUpdateTimeStartEnd()
		{
			if (WasUpdated)
			{
				WasUpdated = false;
				ModifiedTimeRange(_Time, _Precision, out _TimeStart, out _TimeEnd);
			}
		}

		public static void ModifiedTimeRange(DateTime Time, PrecisionType Precision, out DateTime ModifiedTimeStart, out DateTime ModifiedTimeEnd)
		{
			switch (Precision)
			{
				case PrecisionType.Ticks:
					ModifiedTimeEnd = ModifiedTimeStart = new DateTime(Time.Ticks);
					break;
				case PrecisionType.Seconds:
					ModifiedTimeStart = new DateTime(Time.Year, Time.Month, Time.Day, Time.Hour, Time.Minute, Time.Second + 0);
					ModifiedTimeEnd = new DateTime(Time.Year, Time.Month, Time.Day, Time.Hour, Time.Minute, Time.Second + 1);
					break;
				case PrecisionType.Minutes:
					ModifiedTimeStart = new DateTime(Time.Year, Time.Month, Time.Day, Time.Hour, Time.Minute + 0, 0);
					ModifiedTimeEnd = new DateTime(Time.Year, Time.Month, Time.Day, Time.Hour, Time.Minute + 1, 0);
					break;
				case PrecisionType.Hours:
					ModifiedTimeStart = new DateTime(Time.Year, Time.Month, Time.Day, Time.Hour + 0, 0, 0);
					ModifiedTimeEnd = new DateTime(Time.Year, Time.Month, Time.Day, Time.Hour + 1, 0, 0);
					break;
				case PrecisionType.Days:
					ModifiedTimeStart = new DateTime(Time.Year, Time.Month, Time.Day + 0);
					ModifiedTimeEnd = new DateTime(Time.Year, Time.Month, Time.Day + 1);
					break;
				case PrecisionType.Months:
					ModifiedTimeStart = new DateTime(Time.Year, Time.Month + 0, 0);
					ModifiedTimeEnd = new DateTime(Time.Year, Time.Month + 1, 0);
					break;
				case PrecisionType.Years:
					ModifiedTimeStart = new DateTime(Time.Year + 0, 0, 0);
					ModifiedTimeEnd = new DateTime(Time.Year + 1, 0, 0);
					break;
				default:
					ModifiedTimeStart = DateTime.MinValue;
					ModifiedTimeEnd = DateTime.MaxValue;
					break;
			}
		}

		public override string ToString()
		{
			return "DateTimeRange(TimeStart=" + this.TimeStart + ", TimeEnd=" + this.TimeEnd + ", Time=" + this.Time + ", Precision=" + this.Precision + ")";
		}
	}
}
