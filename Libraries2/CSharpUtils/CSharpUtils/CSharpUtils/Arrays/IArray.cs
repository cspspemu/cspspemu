using System.Collections.Generic;

namespace CSharpUtils.Arrays
{
	public interface IArray<TType> : IEnumerable<TType>
	{
		TType this[int Index] { get; set; }
		int Length { get; }
		TType[] GetArray();
	}
}
