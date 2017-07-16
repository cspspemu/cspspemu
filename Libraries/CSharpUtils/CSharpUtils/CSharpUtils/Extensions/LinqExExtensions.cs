using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CSharpUtils;

public static class LinqExExtensions
{
	public static bool ContainsSubset<TSource>(this IEnumerable<TSource> Superset, IEnumerable<TSource> Subset)
	{
		return !Subset.Except(Superset).Any();
	}

	/// <summary>
	/// See ToDictionary.
	/// </summary>
	/// <typeparam name="TValue"></typeparam>
	/// <typeparam name="TKey"></typeparam>
	/// <param name="ListItems"></param>
	/// <param name="KeySelector"></param>
	/// <returns></returns>
	public static Dictionary<TKey, TValue> CreateDictionary<TValue, TKey>(this IEnumerable<TValue> ListItems, Func<TValue, TKey> KeySelector)
	{
		var Dictionary = new Dictionary<TKey, TValue>();
		foreach (var Item in ListItems) Dictionary.Add(KeySelector(Item), Item);
		return Dictionary;
	}

	public static Dictionary<TDictionaryKey, TDictionaryValue> CreateDictionary<TValue, TDictionaryKey, TDictionaryValue>(this IEnumerable<TValue> ListItems, Func<TValue, TDictionaryKey> KeySelector, Func<TValue, TDictionaryValue> ValueSelector)
	{
		var Dictionary = new Dictionary<TDictionaryKey, TDictionaryValue>();
		foreach (var Item in ListItems) Dictionary.Add(KeySelector(Item), ValueSelector(Item));
		return Dictionary;
	}

	public static T[] Compact<T>(this T[,] In)
	{
		int Length0 = In.GetLength(0);
		int Length1 = In.GetLength(1);
		T[] Out = new T[Length0 * Length1];
		int OutOffset = 0;
		for (int InOffset0 = 0; InOffset0 < Length0; InOffset0++)
		{
			for (int InOffset1 = 0; InOffset1 < Length1; InOffset1++)
			{
				Out[OutOffset++] = In[InOffset0, InOffset1];
			}
		}
		return Out;
	}

	public static IEnumerable<TSource> OrderByNatural<TSource, TString>(this IEnumerable<TSource> Items, Func<TSource, TString> selector)
	{
		Func<string, object> convert = str =>
		{
			int result;
			if (int.TryParse(str, out result))
			{
				return result;
			}
			else {
				return str;
			}
		};

		return Items.OrderBy(
			Item => Regex.Split(selector(Item).ToString().Replace(" ", ""), "([0-9]+)").Select(convert),
			new EnumerableComparer<object>()
		);
	}

	public static IEnumerable<TSource> OrderByNatural<TSource>(this IEnumerable<TSource> Items)
	{
		return Items.OrderByNatural(Value => Value);
	}

	public static IEnumerable<TSource> DistinctByKey<TSource, TResult>(this IEnumerable<TSource> Items, Func<TSource, TResult> Selector)
	{
		return Items.Distinct(new LinqEqualityComparer<TSource, TResult>(Selector));
	}

	public static String ToHexString(this IEnumerable<byte> Bytes)
	{
		return String.Join("", Bytes.Select(Byte => Byte.ToString("x2")));
	}

	public static String Implode<TSource>(this IEnumerable<TSource> Items, String Separator)
	{
		return String.Join(Separator, Items.Select(Item => Item.ToString()));
	}

	public static String ToStringArray<TSource>(this IEnumerable<TSource> Items, String Separator = ",")
	{
		return Items.Implode(Separator);
	}

	public static void ForEach<T>(this IEnumerable<T> Items, Action<int, T> action)
	{
		int index = 0;
		foreach (var Item in Items) action(index++, Item);
	}

	public static void ForEach<T>(this IEnumerable<T> Items, Action<T> action)
	{
		foreach (var Item in Items) action(Item);
	}

	public static T[] ToArray2<T>(this IEnumerable<T> Items)
	{
		List<T> ListItems = new List<T>();
		foreach (var Item in Items) ListItems.Add(Item);
		return ListItems.ToArray();
	}

	public static T FirstOrDefault<T>(this IEnumerable<T> Items, T Default)
	{
		if (Items.Any()) return Items.First();
		return Default;
	}

	public static T BinarySearch<T, TKey>(this IList<T> list, Func<T, TKey> keySelector, TKey key)
			where TKey : IComparable<TKey>
	{
		int min = 0;
		int max = list.Count;
		while (min < max)
		{
			int mid = (max + min) / 2;
			T midItem = list[mid];
			TKey midKey = keySelector(midItem);
			int comp = midKey.CompareTo(key);
			if (comp < 0)
			{
				min = mid + 1;
			}
			else if (comp > 0)
			{
				max = mid - 1;
			}
			else
			{
				return midItem;
			}
		}
		if (min == max &&
			keySelector(list[min]).CompareTo(key) == 0)
		{
			return list[min];
		}
		throw new InvalidOperationException("Item not found");
	}

	/*
	public static T ProcessNewObject<T>(T Object, Action<T> Callback)
	{
		Callback(Object);
		return Object;
	}
	*/
}
