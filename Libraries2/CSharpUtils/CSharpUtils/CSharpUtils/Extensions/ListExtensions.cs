using System;
using System.Collections.Generic;

public static class ListExtensions
{
	public static int BoundIndex<T>(this List<T> SortedAndNonRepeatedItems, T Item, int LowerIndex, int HigherIndex) where T : IComparable
	{
		//Console.WriteLine("[{0}, {1}]", LowerIndex, HigherIndex);
		int Index = LowerIndex;
		int MinIndex = LowerIndex;
		int MaxIndex = HigherIndex;
		int MaxIterations = 100;
		while (HigherIndex > LowerIndex)
		{
			if (HigherIndex - LowerIndex <= 2)
			{
				LowerIndex++;
			}
			Index = LowerIndex + (HigherIndex - LowerIndex) / 2;
			int Sign = SortedAndNonRepeatedItems[Index].CompareTo(Item);
			//Console.WriteLine(String.Format("Index: {0} [{1} - {2}] : {3}", Index, LowerIndex, HigherIndex, Sign));

			if (Sign < 0)
			{
				LowerIndex = Index;
			}
			else if (Sign > 0)
			{
				HigherIndex = Index;
			}
			else
			{
				break;
			}
			if (MaxIterations-- <= 0)
			{
				throw(new Exception("Internal Error!"));
			}
		}

		return Index;
	}

	public static int BoundIndex<T>(this List<T> SortedAndNonRepeatedItems, T Item) where T : IComparable
	{
		return SortedAndNonRepeatedItems.BoundIndex(Item, 0, SortedAndNonRepeatedItems.Count - 1);
	}

	public static IEnumerable<T> LowerBound<T>(this List<T> SortedAndNonRepeatedItems, T Item, bool Including = true) where T : IComparable
	{
		int Index = Math.Min(SortedAndNonRepeatedItems.BoundIndex(Item) + 1, SortedAndNonRepeatedItems.Count - 1);
		int CompareValue = Including ? 1 : 0;
		for (; Index >= 0; Index--)
		{
			//Console.WriteLine(Index);
			if (SortedAndNonRepeatedItems[Index].CompareTo(Item) < CompareValue)
			{
				yield return SortedAndNonRepeatedItems[Index];
			}
		}
	}

	public static IEnumerable<T> UpperBound<T>(this List<T> SortedAndNonRepeatedItems, T Item, bool Including = true) where T : IComparable
	{
		int Index = Math.Max(SortedAndNonRepeatedItems.BoundIndex(Item) - 1, 0);
		int CompareValue = Including ? -1 : 0;
		for (; Index < SortedAndNonRepeatedItems.Count; Index++)
		{
			if (SortedAndNonRepeatedItems[Index].CompareTo(Item) > CompareValue)
			{
				yield return SortedAndNonRepeatedItems[Index];
			}
		}
	}
}
