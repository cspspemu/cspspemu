using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CSharpUtils.SpaceAssigner;
using CSharpUtils.Streams;

static public class StreamExtensionsExt
{
	/// <summary>
	/// 
	/// </summary>
	/// <remarks>Presumes it is ordered.</remarks>
	/// <param name="Spaces"></param>
	/// <param name="Thresold"></param>
	/// <returns></returns>
	static public SpaceAssigner1D.Space[] JoinWithThresold(this SpaceAssigner1D.Space[] Spaces, int Thresold = 32)
	{
		var NewSpaces = new Stack<SpaceAssigner1D.Space>();
		NewSpaces.Push(Spaces[0]);
		for (int n = 1; n < Spaces.Length; n++)
		{
			var PrevSpace = NewSpaces.Pop();
			var CurrentSpace = Spaces[n];

			//Console.WriteLine("{0} - {1}", PrevSpace, CurrentSpace);

			if (CurrentSpace.Min - PrevSpace.Max <= Thresold)
			{
				NewSpaces.Push(new SpaceAssigner1D.Space(PrevSpace.Min, CurrentSpace.Max));
			}
			else
			{
				NewSpaces.Push(PrevSpace);
				NewSpaces.Push(CurrentSpace);
			}
		}

		return NewSpaces.Reverse().ToArray();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="Stream"></param>
	/// <param name="Spaces"></param>
	/// <returns></returns>
	static public MapStream ConvertSpacesToMapStream(this Stream Stream, SpaceAssigner1D.Space[] Spaces)
	{
		var MapStream = new MapStream();

		foreach (var Space in Spaces)
		{
			MapStream.Map(Space.Min, Stream.SliceWithBounds(Space.Min, Space.Max));
		}

		return MapStream;
	}
}
