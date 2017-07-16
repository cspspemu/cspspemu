using System;
using System.Collections.Generic;
using System.IO;

namespace CSharpUtils.Streams
{
	public class StreamChunker2
	{
		protected static int FindSequence(byte[] Array, byte[] SequenceToFind, int Start = 0, int EndIndex = int.MaxValue)
		{
			int ArrayUpTo = Math.Min(EndIndex, Array.Length) - SequenceToFind.Length;
			int SequenceToFindLength = SequenceToFind.Length;
			for (int n = Start; n < ArrayUpTo; n++)
			{
				bool Found = true;
				for (int m = 0; m < SequenceToFindLength; m++)
				{
					if (SequenceToFind[m] != Array[n + m])
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

		public static List<byte[]> SplitInChunks(Stream InputStream, byte[] Separator)
		{
			var List = new List<byte[]>();
			Split(InputStream, Separator, delegate(byte[] Chunk)
			{
				List.Add(Chunk);
			});
			return List;
		}

		public static void Split(Stream InputStream, byte[] Separator, Action<byte[]> ChunkHandler) {
			byte[] Buffer = new byte[4096];

			byte[] TempDoubleBuffer = new byte[Separator.Length * 2];

			MemoryStream Chunk = new MemoryStream();

			int StartIndex = Separator.Length;
			int SkipChunkStart = 0;
			
			while (!InputStream.Eof()) {
				Array.Copy(TempDoubleBuffer, Separator.Length, TempDoubleBuffer, 0, Separator.Length);
				int TempDoubleBufferReaded = InputStream.Read(TempDoubleBuffer, Separator.Length, Separator.Length);

				int EndIndex = Separator.Length + TempDoubleBufferReaded;

				Chunk.Write(TempDoubleBuffer, Separator.Length, TempDoubleBufferReaded);

				int FoundIndex = FindSequence(TempDoubleBuffer, Separator, StartIndex, EndIndex);
				if (FoundIndex != -1)
				{
					int BytesToRemoveFromChunk = EndIndex - FoundIndex;
					int RealChunkSize = (int)(Chunk.Length - BytesToRemoveFromChunk);

					MemoryStream NewChunk = new MemoryStream();

					NewChunk.WriteBytes(Chunk.ReadChunk(RealChunkSize, BytesToRemoveFromChunk));
					ChunkHandler(Chunk.ReadChunk(SkipChunkStart, RealChunkSize - SkipChunkStart));

					SkipChunkStart = Separator.Length;

					Chunk = NewChunk;
				}
				StartIndex = 0;
			}

			if (Chunk.Length > 0)
			{
				ChunkHandler(Chunk.ReadChunk(SkipChunkStart, (int)Chunk.Length - SkipChunkStart));
			}
		}
	}
}
