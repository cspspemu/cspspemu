using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using CSharpUtils;

/// <summary>
/// 
/// </summary>
public static unsafe class ArrayExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T[] Concat<T>(this T[] left, T[] right)
    {
        if (typeof(T) == typeof(byte[]))
        {
            return (T[]) (object) ConcatBytes((byte[]) (object) left, (byte[]) (object) right);
        }

        var Return = new T[left.Length + right.Length];
        left.CopyTo(Return, 0);
        right.CopyTo(Return, left.Length);
        return Return;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="array"></param>
    /// <typeparam name="TTo"></typeparam>
    /// <returns></returns>
    public static TTo[] CastToStructArray<TTo>(this byte[] array)
    {
        return array.CastToStructArray<byte, TTo>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="input"></param>
    /// <typeparam name="TFrom"></typeparam>
    /// <typeparam name="TTo"></typeparam>
    /// <returns></returns>
    public static TTo[] CastToStructArray<TFrom, TTo>(this TFrom[] input)
    {
        var totalBytes = (Marshal.SizeOf(typeof(TFrom)) * input.Length);
        var output = new TTo[totalBytes / Marshal.SizeOf(typeof(TTo))];
        var inputHandle = GCHandle.Alloc(input, GCHandleType.Pinned);
        var outputHandle = GCHandle.Alloc(output, GCHandleType.Pinned);
        try
        {
            PointerUtils.Memcpy(
                (byte*) outputHandle.AddrOfPinnedObject().ToPointer(),
                (byte*) inputHandle.AddrOfPinnedObject().ToPointer(),
                totalBytes
            );
        }
        finally
        {
            inputHandle.Free();
            outputHandle.Free();
        }
        return output;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="arrays"></param>
    /// <returns></returns>
    public static byte[] ConcatBytes(params byte[][] arrays)
    {
        var @return = new byte[arrays.Sum(item => item.Length)];
        var offset = 0;
        foreach (var array in arrays)
        {
            Buffer.BlockCopy(array, 0, @return, offset, array.Length);
            offset += array.Length;
        }
        return @return;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="first"></param>
    /// <param name="others"></param>
    /// <returns></returns>
    public static byte[] ConcatBytes(this byte[] first, params byte[][] others)
    {
        return ConcatBytes(new[] {first}.Union(others).ToArray());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="This"></param>
    /// <param name="newSize"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T[] ResizedCopy<T>(this T[] This, int newSize)
    {
        var that = new T[newSize];
        Array.Copy(This, that, Math.Min(This.Length, newSize));
        return that;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <param name="rightOffset"></param>
    /// <param name="rightLength"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T[] Concat<T>(this T[] left, T[] right, int rightOffset, int rightLength)
    {
        var Return = new T[left.Length + rightLength];
        Array.Copy(left, 0, Return, 0, left.Length);
        Array.Copy(right, rightOffset, Return, left.Length, rightLength);
        return Return;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="this"></param>
    /// <param name="low"></param>
    /// <param name="high"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T[] SliceWithBounds<T>(this T[] @this, int low, int high)
    {
        if (high < 0)
        {
            return @this.Slice(low, @this.Length + high - low);
        }
        return @this.Slice(low, high - low);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="This"></param>
    /// <param name="start"></param>
    /// <param name="length"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T[] Slice<T>(this T[] This, int start, int length)
    {
        if (start < 0) start = This.Length - start;
        start = Math.Min(Math.Max(0, start), This.Length);
        length = Math.Min(This.Length - start, length);
        var Return = new T[length];
        Array.Copy(This, start, Return, 0, length);
        return Return;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="this"></param>
    /// <param name="start"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T[] Slice<T>(this T[] @this, int start)
    {
        return @this.Slice(start, @this.Length - start);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="this"></param>
    /// <param name="chunkSize"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IEnumerable<T[]> Split<T>(this T[] @this, int chunkSize)
    {
        var completeChunksCount = @this.Length / chunkSize;
        var partialChunkSize = @this.Length % chunkSize;
        var offset = 0;
        for (var n = 0; n < completeChunksCount; n++)
        {
            var chunk = new T[partialChunkSize];
            Array.Copy(@this, offset, chunk, 0, completeChunksCount);
            offset += completeChunksCount;
            yield return chunk;
        }

        if (partialChunkSize <= 0) yield break;

        var chunk2 = new T[partialChunkSize];
        Array.Copy(@this, offset, chunk2, 0, partialChunkSize);
        offset += partialChunkSize;
        //Chunk.CopyTo);
        yield return chunk2;
    }
}