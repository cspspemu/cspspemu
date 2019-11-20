using System;

namespace CSharpUtils
{
    /// <summary>
    /// 
    /// </summary>
    public struct DateTimeRange
    {
        /// <summary>
        /// 
        /// </summary>
        public enum PrecisionType
        {
            /// <summary>
            /// 
            /// </summary>
            Ticks,

            /// <summary>
            /// 
            /// </summary>
            Seconds,

            /// <summary>
            /// 
            /// </summary>
            Minutes,

            /// <summary>
            /// 
            /// </summary>
            Hours,

            /// <summary>
            /// 
            /// </summary>
            Days,

            /// <summary>
            /// 
            /// </summary>
            Months,

            /// <summary>
            /// 
            /// </summary>
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

        private bool _wasUpdated;
        private DateTime _time, _timeStart, _timeEnd;
        private PrecisionType _precision;

        private static DateTimeRange FromUnixTimestamp(long unixTimestamp,
            PrecisionType precision = PrecisionType.Seconds)
        {
            return new DateTimeRange(ConvertFromUnixTimestamp(unixTimestamp), precision);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unixTimestamp"></param>
        /// <returns></returns>
        public static implicit operator DateTimeRange(long unixTimestamp)
        {
            return FromUnixTimestamp(unixTimestamp);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="time"></param>
        /// <param name="precision"></param>
        public DateTimeRange(DateTime time, PrecisionType precision = PrecisionType.Ticks)
        {
            _time = time;
            _precision = precision;
            _wasUpdated = true;
            _timeStart = DateTime.MinValue;
            _timeEnd = DateTime.MaxValue;
        }

        /// <summary>
        /// 
        /// </summary>
        public DateTime Time
        {
            set
            {
                _time = value;
                _wasUpdated = true;
            }
            get => _time;
        }

        /// <summary>
        /// 
        /// </summary>
        public DateTime TimeStart => CheckUpdateTimeStartEnd()._timeStart;

        /// <summary>
        /// 
        /// </summary>
        public DateTime TimeEnd => CheckUpdateTimeStartEnd()._timeEnd;

        /// <summary>
        /// 
        /// </summary>
        public long UnixTimestamp => ConvertToUnixTimestamp(Time);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static DateTime ConvertFromUnixTimestamp(long timestamp)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(timestamp);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static long ConvertToUnixTimestamp(DateTime date)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var diff = date - origin;
            return (long) Math.Floor(diff.TotalSeconds);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="preciseTime"></param>
        /// <returns></returns>
        public bool Contains(DateTime preciseTime)
        {
            return preciseTime >= TimeStart && preciseTime < TimeEnd;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public bool Contains(DateTimeRange that)
        {
            return Contains(that.TimeStart) && Contains(that.TimeEnd - new TimeSpan(1));
        }

        /// <summary>
        /// 
        /// </summary>
        public PrecisionType Precision
        {
            set
            {
                _precision = value;
                _wasUpdated = true;
            }
            get => _precision;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public static implicit operator DateTime(DateTimeRange that) => that.Time;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public static implicit operator DateTimeRange(DateTime that) => new DateTimeRange(that);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(DateTimeRange a, DateTimeRange b) => a.Contains(b) || b.Contains(a);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(DateTimeRange a, DateTimeRange b) => !(a == b);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => TimeStart.GetHashCode() ^ Precision.GetHashCode();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public override bool Equals(object that)
        {
            if (that is DateTimeRange) return this == (DateTimeRange) that;
            if (that is DateTime) return Contains((DateTime) that);
            return false;
        }

        private DateTimeRange CheckUpdateTimeStartEnd()
        {
            if (_wasUpdated)
            {
                _wasUpdated = false;
                ModifiedTimeRange(_time, _precision, out _timeStart, out _timeEnd);
            }
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="time"></param>
        /// <param name="precision"></param>
        /// <param name="modifiedTimeStart"></param>
        /// <param name="modifiedTimeEnd"></param>
        public static void ModifiedTimeRange(DateTime time, PrecisionType precision, out DateTime modifiedTimeStart,
            out DateTime modifiedTimeEnd)
        {
            switch (precision)
            {
                case PrecisionType.Ticks:
                    modifiedTimeEnd = modifiedTimeStart = new DateTime(time.Ticks);
                    break;
                case PrecisionType.Seconds:
                    modifiedTimeStart = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute,
                        time.Second + 0);
                    modifiedTimeEnd = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute,
                        time.Second + 1);
                    break;
                case PrecisionType.Minutes:
                    modifiedTimeStart = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute + 0, 0);
                    modifiedTimeEnd = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute + 1, 0);
                    break;
                case PrecisionType.Hours:
                    modifiedTimeStart = new DateTime(time.Year, time.Month, time.Day, time.Hour + 0, 0, 0);
                    modifiedTimeEnd = new DateTime(time.Year, time.Month, time.Day, time.Hour + 1, 0, 0);
                    break;
                case PrecisionType.Days:
                    modifiedTimeStart = new DateTime(time.Year, time.Month, time.Day + 0);
                    modifiedTimeEnd = new DateTime(time.Year, time.Month, time.Day + 1);
                    break;
                case PrecisionType.Months:
                    modifiedTimeStart = new DateTime(time.Year, time.Month + 0, 0);
                    modifiedTimeEnd = new DateTime(time.Year, time.Month + 1, 0);
                    break;
                case PrecisionType.Years:
                    modifiedTimeStart = new DateTime(time.Year + 0, 0, 0);
                    modifiedTimeEnd = new DateTime(time.Year + 1, 0, 0);
                    break;
                default:
                    modifiedTimeStart = DateTime.MinValue;
                    modifiedTimeEnd = DateTime.MaxValue;
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString() =>
            $"DateTimeRange(TimeStart={TimeStart}, TimeEnd={TimeEnd}, Time={Time}, Precision={Precision})";
    }
}