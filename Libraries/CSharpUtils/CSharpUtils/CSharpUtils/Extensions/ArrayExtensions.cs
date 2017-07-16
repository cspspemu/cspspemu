using CSharpUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

unsafe static public class ArrayExtensions
{
    static public T[] Concat<T>(this T[] Left, T[] Right)
    {
        if (typeof(T) == typeof(byte[]))
        {
            return (T[]) (object) ConcatBytes((byte[]) (object) Left, (byte[]) (object) Right);
        }

        var Return = new T[Left.Length + Right.Length];
        Left.CopyTo(Return, 0);
        Right.CopyTo(Return, Left.Length);
        return Return;
    }

    static public TTo[] CastToStructArray<TTo>(this byte[] Array)
    {
        return Array.CastToStructArray<byte, TTo>();
    }

    static public TTo[] CastToStructArray<TFrom, TTo>(this TFrom[] Input)
    {
        int TotalBytes = (Marshal.SizeOf(typeof(TFrom)) * Input.Length);
        var Output = new TTo[TotalBytes / Marshal.SizeOf(typeof(TTo))];
        var InputHandle = GCHandle.Alloc(Input, GCHandleType.Pinned);
        var OutputHandle = GCHandle.Alloc(Output, GCHandleType.Pinned);
        try
        {
            PointerUtils.Memcpy(
                (byte*) OutputHandle.AddrOfPinnedObject().ToPointer(),
                (byte*) InputHandle.AddrOfPinnedObject().ToPointer(),
                TotalBytes
            );
        }
        finally
        {
            InputHandle.Free();
            OutputHandle.Free();
        }
        return Output;
    }

    static public byte[] ConcatBytes(params byte[][] Arrays)
    {
        var Return = new byte[Arrays.Sum(Item => Item.Length)];
        var Offset = 0;
        foreach (var Array in Arrays)
        {
            Buffer.BlockCopy(Array, 0, Return, Offset, Array.Length);
            Offset += Array.Length;
        }
        return Return;
    }

    static public byte[] ConcatBytes(this byte[] First, params byte[][] Others)
    {
        return ConcatBytes(new[] {First}.Union(Others).ToArray());
    }

    static public T[] ResizedCopy<T>(this T[] This, int NewSize)
    {
        var That = new T[NewSize];
        Array.Copy(This, That, Math.Min(This.Length, NewSize));
        return That;
    }

    static public T[] Concat<T>(this T[] Left, T[] Right, int RightOffset, int RightLength)
    {
        var Return = new T[Left.Length + RightLength];
        Array.Copy(Left, 0, Return, 0, Left.Length);
        Array.Copy(Right, RightOffset, Return, Left.Length, RightLength);
        return Return;
    }

    static public T[] SliceWithBounds<T>(this T[] This, int Low, int High)
    {
        if (High < 0)
        {
            return This.Slice(Low, This.Length + High - Low);
        }
        else
        {
            return This.Slice(Low, High - Low);
        }
    }

    static public T[] Slice<T>(this T[] This, int Start, int Length)
    {
        if (Start < 0) Start = This.Length - Start;
        Start = Math.Min(Math.Max(0, Start), This.Length);
        Length = Math.Min(This.Length - Start, Length);
        var Return = new T[Length];
        Array.Copy(This, Start, Return, 0, Length);
        return Return;
    }

    static public T[] Slice<T>(this T[] This, int Start)
    {
        return This.Slice(Start, This.Length - Start);
    }

    static public IEnumerable<T[]> Split<T>(this T[] This, int ChunkSize)
    {
        int CompleteChunksCount = This.Length / ChunkSize;
        int PartialChunkSize = This.Length % ChunkSize;
        int Offset = 0;
        for (int n = 0; n < CompleteChunksCount; n++)
        {
            var Chunk = new T[PartialChunkSize];
            Array.Copy(This, Offset, Chunk, 0, CompleteChunksCount);
            Offset += CompleteChunksCount;
            yield return Chunk;
        }
        if (PartialChunkSize > 0)
        {
            var Chunk = new T[PartialChunkSize];
            Array.Copy(This, Offset, Chunk, 0, PartialChunkSize);
            Offset += PartialChunkSize;
            //Chunk.CopyTo);
            yield return Chunk;
        }
    }
}