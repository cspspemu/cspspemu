using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpUtils
{
	/// <summary>
	///  @TODO Have to improve performance without allocating memory all the time. Maybe a RingBuffer or so.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ProduceConsumeBuffer<T>
	{
		public T[] Items = new T[0];
		public long TotalConsumed = 0;

		public void Produce(T[] NewBytes)
		{
			Items = Items.Concat(NewBytes);
		}

		public void Produce(T[] NewBytes, int Offset, int Length)
		{
			Items = Items.Concat(NewBytes, Offset, Length);
		}

		public int ConsumeRemaining
		{
			get
			{
				return Items.Length;
			}
		}

		public T[] ConsumePeek(int Length)
		{
			return Items.Slice(0, Length);
		}

		public ArraySegment<T> ConsumePeekArraySegment(int Length)
		{
			return new ArraySegment<T>(Items, 0, Length);
		}

		public int Consume(T[] Buffer, int Offset, int Length)
		{
			Length = Math.Min(Length, ConsumeRemaining);
			Array.Copy(Items, 0, Buffer, Offset, Length);
			Items = Items.Slice(Length);
			TotalConsumed += Length;
			return Length;
		}

		public T[] Consume(int Length)
		{
			var Return = ConsumePeek(Length);
			Items = Items.Slice(Length);
			TotalConsumed += Length;
			return Return;
		}

		public int IndexOf(T Item)
		{
			return Array.IndexOf(Items, Item);
		}

		public int IndexOf(T[] Sequence)
		{
			for (int n = 0; n <= Items.Length - Sequence.Length; n++)
			{
				bool Found = true;
				for (int m = 0; m < Sequence.Length; m++)
				{
					if (!Items[n + m].Equals(Sequence[m]))
					{
						Found = false;
						break;
					}
				}
				if (Found)
				{
					return n;
				}
			}
			return -1;
		}
	}
}
