//#define DEBUG_ILINSTANCEHOLDERPOOL_TIME

using System;
using System.Collections.Generic;
using System.Linq;

namespace SafeILGenerator.Utils
{
    public class IlInstanceHolder
    {
        private static Dictionary<Type, List<IlInstanceHolderPool>> TypePools =
            new Dictionary<Type, List<IlInstanceHolderPool>>();

        public static IlInstanceHolderPoolItem Alloc(Type type, object value = null)
        {
            lock (TypePools)
            {
                if (!TypePools.ContainsKey(type))
                {
                    TypePools[type] = new List<IlInstanceHolderPool>();
                }
                var poolsType = TypePools[type];
                var freePool = poolsType.FirstOrDefault(pool => pool.HasAvailable);
                if (freePool == null)
                {
                    var nextPoolSize = 1 << (poolsType.Count + 2);
                    //if (NextPoolSize < 2048) NextPoolSize = 2048;

#if DEBUG_ILINSTANCEHOLDERPOOL_TIME
					Console.BackgroundColor = ConsoleColor.DarkRed;
					Console.Error.Write("Create ILInstanceHolderPool({0})[{1}]...", Type, NextPoolSize);
					var Start = DateTime.UtcNow;
#endif
                    poolsType.Add(freePool = new IlInstanceHolderPool(type, nextPoolSize));
#if DEBUG_ILINSTANCEHOLDERPOOL_TIME
					var End = DateTime.UtcNow;
					Console.Error.WriteLine("Ok({0})", End - Start);
					Console.ResetColor();
#endif
                }
                var item = freePool.Alloc();
                item.Value = value;
                return item;
            }
        }

        public static IlInstanceHolderPoolItem<TType> TaAlloc<TType>(TType value = default(TType))
        {
            return new IlInstanceHolderPoolItem<TType>(Alloc(typeof(TType), value));
        }

        public static int FreeCount
        {
            get { return TypePools.Values.Sum(pools => pools.Sum(pool => pool.FreeCount)); }
        }

        public static int CapacityCount
        {
            get { return TypePools.Values.Sum(pools => pools.Sum(pool => pool.CapacityCount)); }
        }
    }
}