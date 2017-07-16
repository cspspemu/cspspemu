using System;
using System.Text.RegularExpressions;

namespace CSharpUtils.Templates.Tokenizers
{
	public class TokenizerStringReaderMatch
	{
		public bool Success;
		public int Index;
		public int Length;
		public String Value;
	}

	public class TokenizerStringReader
	{
		public String Text;
		public int Offset;

		public TokenizerStringReader(String Text, int Offset = 0)
		{
			this.Text = Text;
			this.Offset = Offset;
		}

		public int IndexOf(String Str)
		{
			int Index = Text.IndexOf(Str, Offset);
			if (Index != -1) Index -= Offset;
			return Index;
		}

		public TokenizerStringReaderMatch Match(Regex Regex)
		{
			var Return = Regex.Match(Text, Offset);
			return new TokenizerStringReaderMatch()
			{
				Success = Return.Success,
				Index = Return.Success ? Return.Index - Offset : -1,
				Length = Return.Success ? Return.Length : 0,
				Value = Return.Success ? Return.Value : null,
			};
		}

		public char ReadChar()
		{
			return Text[Offset++];
		}

		public void Unread(int Count = 1)
		{
			Offset -= Count;
		}

		public void Skip(int Count = 1)
		{
			Offset += Count;
		}

		public String ReadString(int Length)
		{
			var Return = Text.Substring(Offset, Length);
			Offset += Length;
			return Return;
		}

		public void SkipSpaces()
		{
			var Match = this.Match(new Regex(@"\s+", RegexOptions.Compiled));
			if (Match.Index == 0)
			{
				Skip(Match.Length);
			}
		}

		public String Peek(int Count)
		{
			return Text.Substring(Offset, Count);
		}

		public Char PeekChar()
		{
			return Text[Offset];
		}

		public String GetSlice(int Start, int End)
		{
			return Text.Substring(Start, End - Start);
		}

		int SegmentStart;
		int SegmentEnd;

		public void SegmentSetStart(int Offset = 0)
		{
			SegmentStart = this.Offset + Offset;
		}

		public void SegmentSetEnd(int Offset = 0)
		{
			SegmentEnd = this.Offset + Offset;
		}

		public String SegmentGetSlice()
		{
			return GetSlice(SegmentStart, SegmentEnd);
		}

		public String ReadString()
		{
			return ReadString(Available);
		}

		public int Available
		{
			get
			{
				return Text.Length - Offset;
			}
		}
	}
}
