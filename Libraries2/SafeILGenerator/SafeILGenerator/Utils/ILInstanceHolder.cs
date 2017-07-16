//#define DEBUG_ILINSTANCEHOLDERPOOL_TIME

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeILGenerator.Utils
{
	public class ILInstanceHolder
	{
		private static Dictionary<Type, List<ILInstanceHolderPool>> TypePools = new Dictionary<Type, List<ILInstanceHolderPool>>();

		public static ILInstanceHolderPoolItem Alloc(Type Type, object Value = null)
		{
			lock (TypePools)
			{
				if (!TypePools.ContainsKey(Type))
				{
					TypePools[Type] = new List<ILInstanceHolderPool>();
				}
				var PoolsType = TypePools[Type];
				var FreePool = PoolsType.Where(Pool => Pool.HasAvailable).FirstOrDefault();
				if (FreePool == null)
				{
					int NextPoolSize = 1 << (PoolsType.Count + 2);
					//if (NextPoolSize < 2048) NextPoolSize = 2048;

#if DEBUG_ILINSTANCEHOLDERPOOL_TIME
					Console.BackgroundColor = ConsoleColor.DarkRed;
					Console.Error.Write("Create ILInstanceHolderPool({0})[{1}]...", Type, NextPoolSize);
					var Start = DateTime.UtcNow;
#endif
					PoolsType.Add(FreePool = new ILInstanceHolderPool(Type, NextPoolSize));
#if DEBUG_ILINSTANCEHOLDERPOOL_TIME
					var End = DateTime.UtcNow;
					Console.Error.WriteLine("Ok({0})", End - Start);
					Console.ResetColor();
#endif
				}
				var Item = FreePool.Alloc();
				Item.Value = Value;
				return Item;
			}
		}

		public static ILInstanceHolderPoolItem<TType> TAlloc<TType>(TType Value = default(TType))
		{
			return new ILInstanceHolderPoolItem<TType>(Alloc(typeof(TType), Value));
		}

		public static int FreeCount
		{
			get
			{
				return TypePools.Values.Sum(Pools => Pools.Sum(Pool => Pool.FreeCount));
			}
		}

		public static int CapacityCount
		{
			get
			{
				return TypePools.Values.Sum(Pools => Pools.Sum(Pool => Pool.CapacityCount));
			}
		}
	}
}
