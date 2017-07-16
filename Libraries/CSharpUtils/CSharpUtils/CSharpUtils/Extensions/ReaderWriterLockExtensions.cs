using System;
using System.Threading;

public static class ReaderWriterLockExtensions
{
	public static void ReaderLock(this ReaderWriterLock This, Action Callback)
	{
		This.AcquireReaderLock(int.MaxValue);
		try
		{
			Callback();
		}
		finally
		{
			This.ReleaseReaderLock();
		}
	}

	public static void WriterLock(this ReaderWriterLock This, Action Callback)
	{
		This.AcquireWriterLock(int.MaxValue);
		try
		{
			Callback();
		}
		finally
		{
			This.ReleaseWriterLock();
		}
	}

	public static T ReaderLock<T>(this ReaderWriterLock This, Func<T> Callback)
	{
		This.AcquireReaderLock(int.MaxValue);
		try
		{
			return Callback();
		}
		finally
		{
			This.ReleaseReaderLock();
		}
	}

	public static T WriterLock<T>(this ReaderWriterLock This, Func<T> Callback)
	{
		This.AcquireWriterLock(int.MaxValue);
		try
		{
			return Callback();
		}
		finally
		{
			This.ReleaseWriterLock();
		}
	}
}
