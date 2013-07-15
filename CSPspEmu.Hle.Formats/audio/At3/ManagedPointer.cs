using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

static public class ManagedPointerExtensions
{
	static public ManagedPointer<T> GetPointer<T>(this T[] This, int Offset)
	{
		return new ManagedPointer<T>(This, Offset);
	}

	static public ManagedPointer<T> GetPointer<T>(this ManagedPointer<T> This, int Offset)
	{
		return This.Slice(Offset);
	}
}

public class ManagedPointer<T>
{
	private readonly T[] Array;
	private readonly int Offset;
	private readonly int Length;

	public ManagedPointer(T[] Array, int Offset = 0, int Length = -1)
	{
		if (Length < 0) Length = Array.Length;
		if (Length > Offset + Array.Length) Length = Offset + Array.Length;
		this.Array = Array;
		this.Offset = Offset;
		this.Length = Length;
	}

	private void CheckBounds(int Index)
	{
		if (Index < 0 || Index >= Length) throw (new IndexOutOfRangeException());
	}

	public T this[int Index]
	{
		get { CheckBounds(Index); return Array[Offset + Index]; }
		set { CheckBounds(Index); Array[Offset + Index] = value; }
	}

	public T this[uint Index]
	{
		get { CheckBounds((int)Index); return Array[Offset + Index]; }
		set { CheckBounds((int)Index); Array[Offset + Index] = value; }
	}

	public static ManagedPointer<T> operator +(ManagedPointer<T> Left, int Right)
	{
		return new ManagedPointer<T>(Left.Array, Left.Offset + Right, Left.Length - Right);
	}

	public static ManagedPointer<T> operator +(int Right, ManagedPointer<T> Left)
	{
		return new ManagedPointer<T>(Left.Array, Left.Offset + Right, Left.Length - Right);
	}

	public static implicit operator ManagedPointer<T>(T[] That)
	{
		return new ManagedPointer<T>(That);
	}

	//public static implicit operator T[](ArraySlice<T> that)
	//{
	//}

	public ManagedPointer<T> Slice(int Offset, int Length = -1)
	{
		if (Length < 0) Length = this.Length;
		if (Length >= this.Length - Offset) Length = this.Length - Offset;
		return new ManagedPointer<T>(this.Array, this.Offset + Offset, Length);
	}

	public void Memcpy(ManagedPointer<T> Src, int Length)
	{
		var Dst = this;
		for (int n = 0; n < Length; n++) Dst[n] = Src[n];
	}

	public T[] GetArray(int len_s = -1)
	{
		if (len_s < 0) len_s = this.Length;
		var Output = new T[len_s];
		for (int n = 0; n < len_s; n++) Output[n] = this[n];
		return Output;
	}
}