using System.Collections.Generic;

public static class LinkedListExtensions
{
	public static int GetCountLock<T>(this LinkedList<T> List)
	{
		lock (List) return List.Count;
	}

	public static T RemoveFirstAndGet<T>(this LinkedList<T> List)
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

	public static T RemoveLastAndGet<T>(this LinkedList<T> List)
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
