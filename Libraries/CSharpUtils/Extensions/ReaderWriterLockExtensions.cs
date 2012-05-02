using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

static public class ReaderWriterLockExtensions
{
	static public void ReaderLock(this ReaderWriterLock This, Action Callback)
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

	static public void WriterLock(this ReaderWriterLock This, Action Callback)
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

	static public T ReaderLock<T>(this ReaderWriterLock This, Func<T> Callback)
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

	static public T WriterLock<T>(this ReaderWriterLock This, Func<T> Callback)
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
