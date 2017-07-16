using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpUtils.Templates.Tokenizers
{
	public class TokenReader
	{
		int Index;
		List<TemplateToken> Tokens;

		public TokenReader(List<TemplateToken> Tokens)
		{
			this.Tokens = Tokens;
		}

		void Dump()
		{
			foreach (var Token in Tokens) Console.WriteLine(Token);
		}

		public bool HasMore
		{
			get
			{
				return Index < Tokens.Count;
			}
		}

		public TemplateToken Current
		{
			get
			{
				if (!HasMore) return new EndOfFileTemplateToken()
				{
					Text = "<EOF>",
				};
				return Tokens[Index];
			}
		}

		public void MoveStart()
		{
			Index = 0;
		}

		public void MovePrev()
		{
			if (Index > 0) Index--;
		}

		public void MoveNext()
		{
			if (HasMore) Index++;
		}

		public bool CurrentIsType(Type Type)
		{
			return (Current.GetType() == Type);
		}

		public TemplateToken ExpectType(Type Type)
		{
			if (!CurrentIsType(Type)) throw (new Exception(String.Format("Expected token {0} but obtained {1}('{2}')", Type, Current.GetType(), Current.Text)));
			return Current;
		}

		public TemplateToken ExpectValue(params String[] ExpectedValue)
		{
			if (!ExpectedValue.Contains(Current.Text))
			{
				throw (new Exception(String.Format("Expected tokens [{0}] but obtained '{1}'", ExpectedValue.Select(Item => "'" + Item + "'").Implode(","), Current.Text)));
			}
			return Current;
		}

		public TemplateToken ExpectTypeAndNext(Type Type)
		{
			TemplateToken ReturnToken = ExpectType(Type);
			MoveNext();
			return ReturnToken;
		}

		public TemplateToken ExpectValueAndNext(params String[] ExpectedValue)
		{
			var Return = ExpectValue(ExpectedValue);
			MoveNext();
			return Return;
		}
	}
}
