using System;
using System.Collections.Generic;
using System.Linq;

public static class LinqExExtensionsExt
{
    /// <summary>
    /// http://msdn.microsoft.com/en-us/magazine/cc163329.aspx
    /// http://stackoverflow.com/questions/3789998/parallel-foreach-vs-foreachienumerablet-asparallel
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="Items"></param>
    /// <param name="action"></param>
    public static void ForEach<T>(this ParallelQuery<T> Items, Action<T> action)
    {
        Items.ForAll(action);
    }

    public static int LocateWhereMinIndex<T>(this IEnumerable<T> Items, Func<T, bool> where,
        Func<T, dynamic> compareValue)
    {
        bool First = true;
        T MinItem = default(T);
        dynamic MinValue = null;
        int MinIndex = -1;
        int Index = 0;
        foreach (var Item in Items)
        {
            if (where(Item))
            {
                dynamic CurValue = compareValue(Item);

                if (First || (CurValue < MinValue))
                {
                    MinItem = Item;
                    MinValue = CurValue;
                    MinIndex = Index;
                    First = false;
                }
            }

            Index++;
        }

        return MinIndex;
    }

    public static T LocateMin<T>(this IEnumerable<T> Items, Func<T, dynamic> compareValue)
    {
        bool First = true;
        T MinItem = default(T);
        dynamic MinValue = null;
        foreach (var Item in Items)
        {
            dynamic CurValue = compareValue(Item);

            if (First || (CurValue < MinValue))
            {
                MinItem = Item;
                MinValue = CurValue;
                First = false;
            }
        }
        return MinItem;
    }
}