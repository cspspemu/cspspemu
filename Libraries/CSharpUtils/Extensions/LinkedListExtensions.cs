using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

static public class LinkedListExtensions
{
	static public int GetCountLock<T>(this LinkedList<T> List)
	{
		lock (List) return List.Count;
	}

	static public T RemoveFirstAndGet<T>(this LinkedList<T> List)
	{
		lock (List)
		{
			try
			{
				return List.First.Value;
			}
			finally
			{
				List.RemoveFirst();
			}
		}
	}

	static public T RemoveLastAndGet<T>(this LinkedList<T> List)
	{
		lock (List)
		{
			try
			{
				return List.Last.Value;
			}
			finally
			{
				List.RemoveLast();
			}
		}
	}
}
