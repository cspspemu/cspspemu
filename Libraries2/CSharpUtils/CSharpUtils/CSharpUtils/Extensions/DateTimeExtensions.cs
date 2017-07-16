using System;

public static class DateTimeExtensions
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="DateTime"></param>
	/// <returns></returns>
	public static long GetTotalNanoseconds(this DateTime DateTime)
	{
		return DateTime.Ticks * 10;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="DateTime"></param>
	/// <returns></returns>
	public static long GetTotalMicroseconds(this DateTime DateTime)
	{
		return DateTime.Ticks / 10;
	}

	public static DateTime FromMicroseconds(long Microseconds)
	{
		return new DateTime(Microseconds * 10);
	}
}
