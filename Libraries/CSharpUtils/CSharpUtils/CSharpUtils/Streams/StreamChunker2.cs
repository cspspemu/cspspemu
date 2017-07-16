using System;
using System.Collections.Generic;
using System.IO;

namespace CSharpUtils.Streams
{
    /// <summary>
    /// 
    /// </summary>
    public class StreamChunker2
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="sequenceToFind"></param>
        /// <param name="start"></param>
        /// <param name="endIndex"></param>
        /// <returns></returns>
        protected static int FindSequence(byte[] array, byte[] sequenceToFind, int start = 0,
            int endIndex = int.MaxValue)
        {
            var arrayUpTo = Math.Min(endIndex, array.Length) - sequenceToFind.Length;
            var sequenceToFindLength = sequenceToFind.Length;
            for (var n = start; n < arrayUpTo; n++)
            {
                var found = true;
                for (var m = 0; m < sequenceToFindLength; m++)
                {
                    if (sequenceToFind[m] != array[n + m])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    return n;
                }
            }
            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static List<byte[]> SplitInChunks(Stream inputStream, byte[] separator)
        {
            var list = new List<byte[]>();
            Split(inputStream, separator, delegate(byte[] chunk) { list.Add(chunk); });
            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="separator"></param>
        /// <param name="chunkHandler"></param>
        public static void Split(Stream inputStream, byte[] separator, Action<byte[]> chunkHandler)
        {
            //var buffer = new byte[4096];
            var tempDoubleBuffer = new byte[separator.Length * 2];
            var chunk = new MemoryStream();
            var startIndex = separator.Length;
            var skipChunkStart = 0;

            while (!inputStream.Eof())
            {
                Array.Copy(tempDoubleBuffer, separator.Length, tempDoubleBuffer, 0, separator.Length);
                var tempDoubleBufferReaded = inputStream.Read(tempDoubleBuffer, separator.Length, separator.Length);
                var endIndex = separator.Length + tempDoubleBufferReaded;
                chunk.Write(tempDoubleBuffer, separator.Length, tempDoubleBufferReaded);
                var foundIndex = FindSequence(tempDoubleBuffer, separator, startIndex, endIndex);
                if (foundIndex != -1)
                {
                    var bytesToRemoveFromChunk = endIndex - foundIndex;
                    var realChunkSize = (int) (chunk.Length - bytesToRemoveFromChunk);
                    var newChunk = new MemoryStream();

                    newChunk.WriteBytes(chunk.ReadChunk(realChunkSize, bytesToRemoveFromChunk));
                    chunkHandler(chunk.ReadChunk(skipChunkStart, realChunkSize - skipChunkStart));

                    skipChunkStart = separator.Length;

                    chunk = newChunk;
                }
                startIndex = 0;
            }

            if (chunk.Length > 0)
            {
                chunkHandler(chunk.ReadChunk(skipChunkStart, (int) chunk.Length - skipChunkStart));
            }
        }
    }
}