using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

static public class IntegerExtensions
{
	public static byte RotateLeft(this byte value, int count)
	{
		return (byte)((value << count) | (value >> (8 - count)));
	}

	public static byte RotateRight(this byte value, int count)
	{
		return (byte)((value >> count) | (value << (8 - count)));
	}

	public static uint RotateLeft(this uint value, int count)
	{
		return (value << count) | (value >> (32 - count));
	}

	public static uint RotateRight(this uint value, int count)
	{
		return (value >> count) | (value << (32 - count));
	}

	public static ulong RotateLeft(this ulong value, int count)
	{
		return (value << count) | (value >> (64 - count));
	}

	public static ulong RotateRight(this ulong value, int count)
	{
		return (value >> count) | (value << (64 - count));
	}

	public static T[] Repeat<T>(this T RepeatedValue, int Count)
	{
		var List = new T[Count];
		for (int n = 0; n < Count; n++) List[n] = RepeatedValue;
		return List;
	}

	public static uint ExtractUnsigned(this ushort Value, int Offset, int Count)
	{
		var Mask = (1 << Count) - 1;
		return (uint)((Value >> Offset) & Mask);
	}

	public static uint ExtractUnsignedScale(this ushort Value, int Offset, int Count, int Scale)
	{
		var Mask = (1 << Count) - 1;
		return (uint)((Value.ExtractUnsigned(Offset, Count) * Scale) / Mask);
	}
}
