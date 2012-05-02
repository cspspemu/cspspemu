using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace CSharpUtils
{
	public class AsyncTask<T>
	{
		protected Semaphore Semaphore;

		public bool Ready { get; protected set; }

		protected T _Result;

		public T Result
		{
			get
			{
				Semaphore.WaitOne();
				Debug.Assert(Ready);
				return _Result;
			}
		}

		public AsyncTask(Func<T> Getter)
		{
			Semaphore = new Semaphore(0, 1);
			new Thread(delegate()
			{
				_Result = Getter();
				Ready = true;
				Semaphore.Release(1);
			}).Start();
			Thread.Yield();
		}

		static public implicit operator T(AsyncTask<T> That)
		{
			return That.Result;
		}

		public override string ToString()
		{
			return Result.ToString();
		}
	}
}
