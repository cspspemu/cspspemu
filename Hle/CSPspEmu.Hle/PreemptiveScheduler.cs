using System;
using System.Collections.Generic;
using System.Linq;

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
		bool ThrowException;

		public IEnumerable<T> GetThreadsInQueue()
		{
			lock (this)
			{
				return CurrentReadyQueue.ToArray();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="NewItemsFirst"></param>
		/// <param name="ThrowException"></param>
		public PreemptiveScheduler(bool NewItemsFirst, bool ThrowException = true)
		{
			this.NewItemsFirst = NewItemsFirst;
			this.ThrowException = ThrowException;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="PreemptiveItem"></param>
		public void Remove(T PreemptiveItem)
		{
			lock (this)
			{
				Items.Remove(PreemptiveItem);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Item"></param>
		public void Update(T Item)
		{
			lock (this)
			{
				//Console.WriteLine("PreemptiveScheduler.Update: {0}", Item);

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
		}

		private void AddItemToCurrentReadyQueue(T Item)
		{
			lock (this)
			{
				//Console.WriteLine("AddItemToCurrentReadyQueue: {0}", Item);
				if (!CurrentReadyQueue.Contains(Item))
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
		public bool ReScheduleHighestPriority()
		{
			lock (this)
			{
				var ReadyItems = Items.Where(Item => Item.Ready);
				if (!ReadyItems.Any())
				{
					if (ThrowException) throw (new Exception("No items to schedule"));
					return false;
				}
				else
				{
					CurrentHighestPriority = ReadyItems.Max(Item => Item.Priority);

					foreach (var Item in Items.Where(Item => Item.Ready && Item.Priority == CurrentHighestPriority))
					{
						AddItemToCurrentReadyQueue(Item);
					}

					return true;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void Next()
		{
			lock (this)
			{
				bool Found = false;
				Current = default(T);

				while (!Found)
				{
					// Queue is empty, lets find the higher Ready priority.
					if (CurrentReadyQueue.Count == 0)
					{
						if (!ReScheduleHighestPriority()) break;
					}

					// Get next in the queue.
					while (CurrentReadyQueue.Count > 0)
					{
						var Item = CurrentReadyQueue.First.Value;
						CurrentReadyQueue.RemoveFirst();

						if (Item.Ready && Item.Priority == CurrentHighestPriority)
						{
							//Console.WriteLine("Rescheduled again: {0}", Item);
							Current = Item;
							Found = true;
							CurrentReadyQueue.AddLast(Item);
							break;
						}
						else
						{
							//Console.WriteLine("Not scheduled again: {0} : Ready: {1}, Priority : {2} != {3}", Item, Item.Ready, Item.Priority, CurrentHighestPriority);
						}
					}
				}
			}
		}
	}
}
