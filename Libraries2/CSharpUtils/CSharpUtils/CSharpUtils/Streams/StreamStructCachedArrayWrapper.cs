using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using CSharpUtils.Arrays;
using CSharpUtils.Threading;

namespace CSharpUtils.Streams
{
	public class StreamStructCachedArrayWrapper<TType> : IArray<TType> where TType : struct
	{
		List<TType> CachedValues = new List<TType>();
		int NumberOfItemsToBuffer;
		Stream Stream;
		static Type StructType = typeof(TType);
		static int StructSize = Marshal.SizeOf(StructType);

		public StreamStructCachedArrayWrapper(int NumberOfItemsToBuffer, Stream Stream)
		{
			this.NumberOfItemsToBuffer = NumberOfItemsToBuffer;
			this.Stream = Stream;
			this.CustomThreadPool = new CustomThreadPool(1);
		}

		private int BufferedItemsCount
		{
			get
			{
				return CachedValues.Count;
			}
		}

		private void SecureUpToItem(int MaxItem)
		{
			MaxItem = Math.Min(MaxItem, Length);
			int ItemsToRead = MaxItem - BufferedItemsCount;

			//Console.WriteLine("SecureUpToItem: {0}, {1}, {2}", MaxItem, BufferedItemsCount, ItemsToRead);

			if (ItemsToRead > 0)
			{
				try
				{
					int DataLength = ItemsToRead * StructSize;
					var Data = new byte[DataLength];

					/*
					Stream.BeginRead(Data, 0, DataLength, (AsyncState) =>
					{
						int Readed = Stream.EndRead(AsyncState);
						CachedValues.AddRange(PointerUtils.ByteArrayToArray<TType>(Data));
					}, null);
					*/
					CustomThreadPool.AddTask(0, () =>
					{
						int Readed = Stream.Read(Data, 0, DataLength);
						CachedValues.AddRange(PointerUtils.ByteArrayToArray<TType>(Data));
					});
				}
				catch (Exception Exception)
				{
					Console.Error.WriteLine(Exception);
				}
			}
		}

		private void SecureUpToItem(int Offset, int NumberOfItemsToBuffer)
		{
			if (BufferedItemsCount - Offset < NumberOfItemsToBuffer / 2)
			{
				while (true)
				{
					if (BufferedItemsCount - Offset < NumberOfItemsToBuffer / 2)
					{
						SecureUpToItem(BufferedItemsCount + NumberOfItemsToBuffer);
					}

					if (Offset >= BufferedItemsCount)
					{
						Thread.Sleep(1);
					}
					else
					{
						break;
					}
					//Console.Write("Wait({0}, {1})", Offset, BufferedItemsCount);
				}
			}
		}

		public IEnumerator<TType> GetEnumerator()
		{
			for (int n = 0; n < Length; n++) yield return this[n];
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			for (int n = 0; n < Length; n++) yield return this[n];
		}

		public TType this[int Index]
		{
			get
			{
				if (Index < 0 || Index >= Length) throw(new IndexOutOfRangeException(String.Format("Invalid Index {0}. Must be in range {1}-{2}", Index, 00, Length)));
				SecureUpToItem(Index, NumberOfItemsToBuffer);
				return CachedValues[Index];
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public int Length
		{
			get
			{
				return (int)(Stream.Length / StructSize);
			}
		}

		public TType[] GetArray()
		{
			SecureUpToItem(Length);
			return CachedValues.ToArray();
		}

		public CustomThreadPool CustomThreadPool { get; set; }
	}
}
