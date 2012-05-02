using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using CSharpUtils;
using CSharpUtils.Arrays;

namespace CSharpUtils.Streams
{
	public class StreamStructArrayWrapper<TType> : IArray<TType> where TType : struct
	{
		Stream Stream;
		static Type StructType = typeof(TType);
		static int StructSize = Marshal.SizeOf(StructType);

		public StreamStructArrayWrapper()
		{
			this.Stream = null;
		}

		public StreamStructArrayWrapper(Stream Stream)
		{
			this.Stream = Stream;
		}

		private void SeekToIndex(int Index)
		{
			Stream.Position = Index * StructSize;
		}

		public TType this[int Index]
		{
			get
			{
				SeekToIndex(Index);
				return Stream.ReadStruct<TType>();
			}
			set
			{
				SeekToIndex(Index);
				Stream.WriteStruct(value);
			}
		}

		public int Length
		{
			get
			{
				if (Stream == null) return 0;
				return (int)(Stream.Length / StructSize);
			}
		}

		IEnumerator<TType> IEnumerable<TType>.GetEnumerator()
		{
			for (int n = 0; n < Length; n++) yield return this[n];
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			for (int n = 0; n < Length; n++) yield return this[n];
		}

		public TType[] GetArray()
		{
			return this.ToArray();
		}
	}
}
