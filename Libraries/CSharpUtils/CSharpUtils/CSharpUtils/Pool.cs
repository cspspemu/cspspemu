using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpUtils
{
	public class Pool<TType>
	{
		private Queue<TType> FreeQueue = new Queue<TType>();
		private HashSet<TType> AllocatedHashSet = new HashSet<TType>();

		public Pool(int Count, Func<int, TType> Allocator)
		{
			foreach (var Item in Enumerable.Range(0, Count).Select(Index => Allocator(Index)))
			{
				FreeQueue.Enqueue(Item);
			}
		}

		public void Free(TType Item)
		{
			AllocatedHashSet.Remove(Item);
			FreeQueue.Enqueue(Item);
		}

		public TType Allocate()
		{
			var Item = FreeQueue.Dequeue();
			AllocatedHashSet.Add(Item);
			return Item;
		}
	}
}
