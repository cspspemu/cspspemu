using System;
using System.Collections.Generic;
using System.IO;

namespace CSharpUtils
{
	public sealed class ProduceConsumerBufferStream : Stream
	{
		private MemoryStream MemoryStream;

		private long ConsumePosition = 0;
		private long TotalProducePosition = 0;
		private long TotalConsumePosition = 0;

		public ProduceConsumerBufferStream()
		{
			this.MemoryStream = new MemoryStream();
		}

		public override bool CanRead
		{
			get { return true; }
		}

		public override bool CanSeek
		{
			get { return false; }
		}

		public override bool CanWrite
		{
			get { return true; }
		}

		public override void Flush()
		{
		}

		public override long Length
		{
			get {
				return MemoryStream.Length;
			}
		}

		public override long Position
		{
			get
			{
				return ConsumePosition;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		private void TryReduceMemorySize()
		{
			if (ReadTransactionStack.Count == 0)
			{
				if (ConsumePosition >= 512 * 1024 || ConsumePosition >= Length / 2)
				{
					var NewMemoryStream = new MemoryStream();
					this.MemoryStream.SliceWithLength(ConsumePosition).CopyToFast(NewMemoryStream);
					ConsumePosition = 0;
					this.MemoryStream = NewMemoryStream;
				}
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			MemoryStream.Position = ConsumePosition;
			int Readed = MemoryStream.Read(buffer, offset, count);
			ConsumePosition = MemoryStream.Position;
			TotalConsumePosition += Readed;
			TryReduceMemorySize();
			return Readed;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			MemoryStream.Position = MemoryStream.Length;
			MemoryStream.Write(buffer, offset, count);
			TotalProducePosition += count;
		}

		internal class ReadTransactionState
		{
			internal long ConsumePosition;
			internal long TotalConsumePosition;
		}

		private readonly Stack<ReadTransactionState> ReadTransactionStack = new Stack<ReadTransactionState>();

		public void ReadTransactionBegin()
		{
			ReadTransactionStack.Push(new ReadTransactionState()
			{
				ConsumePosition = this.ConsumePosition,
				TotalConsumePosition = this.TotalConsumePosition,
			});
		}

		public void ReadTransactionCommit()
		{
			ReadTransactionStack.Pop();
		}

		public void ReadTransactionRevert()
		{
			var ReadTransactionState = ReadTransactionStack.Pop();
			this.ConsumePosition = ReadTransactionState.ConsumePosition;
			this.TotalConsumePosition = ReadTransactionState.TotalConsumePosition;
		}
	}

	/// <summary>
	///  @TODO Have to improve performance without allocating memory all the time. Maybe a RingBuffer or so.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ProduceConsumeBuffer<T>
	{
		public T[] Items = new T[0];
		public long TotalConsumed { get; private set; }
	
		public void Produce(T[] NewBytes)
		{
			Produce(NewBytes, 0, NewBytes.Length);
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
			Length = Math.Min(Length, ConsumeRemaining);
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
			Length = Math.Min(Length, ConsumeRemaining);
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
