using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle
{
	public interface IPreemptiveItem
	{
		int Priority { get; }
		bool Ready { get; }

		//event Action<IPreemptiveItem> Updated;
	}

	public class PreemptiveScheduler<T> where T : IPreemptiveItem
	{
		LinkedList<T> CurrentReadyQueue = new LinkedList<T>();
		List<T> Items = new List<T>();

		bool NewItemsFirst;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="NewItemsFirst"></param>
		public PreemptiveScheduler(bool NewItemsFirst)
		{
			this.NewItemsFirst = NewItemsFirst;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="PreemptiveItem"></param>
		public void Remove(T PreemptiveItem)
		{
			Items.Remove(PreemptiveItem);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Item"></param>
		public void Update(T Item)
		{
			if (!Items.Contains(Item))
			{
				//Items.Remove(PreemptiveItem);
				Items.Add(Item);
			}

			if (Item.Ready && Item.Priority >= CurrentHighestPriority)
			{
				if (Item.Priority == CurrentHighestPriority)
				{
					AddItemToCurrentReadyQueue(Item);
				}
				else
				{
					ReScheduleHighestPriority();
				}
			}
		}

		private void AddItemToCurrentReadyQueue(T Item)
		{
			if (NewItemsFirst)
			{
				CurrentReadyQueue.AddFirst(Item);
			}
			else
			{
				CurrentReadyQueue.AddLast(Item);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int CurrentHighestPriority { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public T Current { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>If an item changes its priority, this method should be executed.</remarks>
		public void ReScheduleHighestPriority()
		{
			if (Items.Count == 0) throw (new Exception("No items to schedule"));

			CurrentHighestPriority = Items.Where(Item => Item.Ready).Max(Item => Item.Priority);

			foreach (var Item in Items.Where(Item => Item.Ready && Item.Priority == CurrentHighestPriority))
			{
				AddItemToCurrentReadyQueue(Item);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void Next()
		{
			bool Found = false;
			Current = default(T);

			while (!Found)
			{
				// Queue is empty, lets find the higher Ready priority.
				if (CurrentReadyQueue.Count == 0)
				{
					ReScheduleHighestPriority();
				}

				// Get next in the queue.
				while (CurrentReadyQueue.Count > 0)
				{
					var Item = CurrentReadyQueue.RemoveFirstAndGet();
					if (Item.Ready && Item.Priority == CurrentHighestPriority)
					{
						Current = Item;
						Found = true;
						CurrentReadyQueue.AddLast(Item);
						break;
					}
				}
			}
		}
	}
}
