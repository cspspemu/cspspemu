using System;
using System.Collections.Generic;
using System.IO;
using CSharpUtils.Templates.Tokenizers;
using CSharpUtils.Templates.ParserNodes;
using CSharpUtils.Templates.Templates.ParserNodes;

namespace CSharpUtils.Templates
{
	public class Finalize_HandlingLevel_Root : Exception
	{
	}

	public class TemplateException : Exception
	{
		public TemplateException(String Message) : base(Message)
		{
		}
	}

	public class TemplateParentOutsideBlockException : TemplateException
	{
		public TemplateParentOutsideBlockException(String Message) : base(Message)
		{
		}
	}

	public class TemplateParser
	{
		TokenReader Tokens;
		TextWriter TextWriter;
		String CurrentBlockName;
		public Dictionary<String, ParserNode> Blocks;
		int IsInsideABlock = 0;

		public TemplateParser(TokenReader Tokens, TextWriter TextWriter)
		{
			this.Tokens = Tokens;
			this.TextWriter = TextWriter;
			this.Blocks = new Dictionary<string, ParserNode>();
		}

		public void Reset()
		{
			IsInsideABlock = 0;
		}

		TemplateToken CurrentToken
		{
			get
			{
				return Tokens.Current;
			}
		}

		string CurrentTokenType
		{
			get
			{
				return CurrentToken.GetType().Name;
			}
		}

		protected ParserNode _HandleLevel_OpBase(Func<ParserNode> HandleLevelNext, string[] Operators, Func<ParserNode, String, ParserNode> HandleOperator)
		{
			ParserNode ParserNode = HandleLevelNext();

			while (Tokens.HasMore)
			{
				switch (CurrentTokenType)
				{
					case "OperatorTemplateToken":
						string Operator = CurrentToken.Text;
						bool Found = false;
						foreach (var CurrentOperator in Operators)
						{
							if (Operator == CurrentOperator)
							{
								ParserNode = HandleOperator(ParserNode, Operator);
								Found = true;
								break;
							}
						}
						if (!Found)
						{
							return ParserNode;
						}
						break;
					default:
						return ParserNode;
				}
			}

			return ParserNode;
		}

		protected ParserNode _HandleLevel_Op(Func<ParserNode> HandleLevelNext, string[] Operators, Func<ParserNode, ParserNode, String, ParserNode> HandleOperator)
		{
			return _HandleLevel_OpBase(HandleLevelNext, Operators, delegate(ParserNode ParserNode, String Operator) {
				Tokens.MoveNext();
				ParserNode RightNode = HandleLevelNext();
				return HandleOperator(ParserNode, RightNode, Operator);
			});
		}

		public ParserNode _HandleLevel_Op_BinarySimple(Func<ParserNode> HandleLevelNext, params string[] BinarySimple)
		{
			return _HandleLevel_Op(
				HandleLevelNext,
				BinarySimple,
				(ParserNode LeftNode, ParserNode RightNode, String Operator) => { return new ParserNodeBinaryOperation(LeftNode, RightNode, Operator); }
			);
		}


		public ParserNode HandleLevel_Identifier()
		{
			ParserNode ParserNode;

			switch (CurrentTokenType)
			{
				case "OperatorTemplateToken":
					String Operator = CurrentToken.Text;
					switch (Operator)
					{
						// Unary Operators
						case "+": case "-":
							Tokens.MoveNext();
							ParserNode = new ParserNodeUnaryOperation()
							{
								Parent = HandleLevel_Identifier(),
								Operator = Operator,
							};
							Tokens.MoveNext();
							break;
						case "(":
							Tokens.MoveNext();
							ParserNode = HandleLevel_Expression();
							Tokens.ExpectValueAndNext(")");
							break;
						default:
							throw (new Exception(String.Format("Invalid operator '{0}'('{1}')", CurrentTokenType, CurrentToken.Text)));
					}
					break;
				case "NumericLiteralTemplateToken":
					ParserNode = new ParserNodeNumericLiteral()
					{
						Value = Int64.Parse(CurrentToken.Text),
					};
					Tokens.MoveNext();
					break;
				case "IdentifierTemplateToken":
					String Id = CurrentToken.Text;
					Tokens.MoveNext();

					// Constants.
					switch (Id) {
						case "true":
							ParserNode = new ParserNodeConstant(Id);
							break;
						case "false":
							ParserNode = new ParserNodeConstant(Id);
							break;
						case "none":
							ParserNode = new ParserNodeConstant(Id);
							break;
						default:
							ParserNode = new ParserNodeIdentifier(Id);
							break;
					}

					bool Running = true;
					while (Running)
					{
						switch (CurrentToken.Text)
						{
							// Dot identifier accessor.
							case ".": {
								Tokens.MoveNext();
								TemplateToken AcessToken = Tokens.ExpectTypeAndNext(typeof(IdentifierTemplateToken));
								ParserNode = new ParserNodeAccess(ParserNode, new ParserNodeStringLiteral(AcessToken.Text));
							} break;
							// Brace expression accessor.
							case "[": {
								Tokens.MoveNext();
								ParserNode AccessNode = HandleLevel_Expression();
								Tokens.ExpectValueAndNext("]");
								ParserNode = new ParserNodeAccess(ParserNode, AccessNode);
							} break;
							default:
								Running = false;
								break;
						}
					}

					break;
				case "StringLiteralTemplateToken":
					ParserNode = new ParserNodeStringLiteral(((StringLiteralTemplateToken)CurrentToken).UnescapedText);
					Tokens.MoveNext();
					break;
				default:
					throw (new Exception(String.Format("Invalid Identifier '{0}'('{1}')", CurrentTokenType, CurrentToken.Text)));
			}

			return ParserNode;
		}

		public ParserNode HandleLevel_Pow()
		{
			return _HandleLevel_Op_BinarySimple(HandleLevel_Identifier, "**");
		}

		public ParserNode HandleLevel_Mul()
		{
			return _HandleLevel_Op_BinarySimple(HandleLevel_Pow, "*", "/", "//", "%");
		}

		public ParserNode HandleLevel_Sum()
		{
			return _HandleLevel_Op_BinarySimple(HandleLevel_Mul, "+", "-");
		}

		public ParserNode HandleLevel_And()
		{
			return _HandleLevel_Op_BinarySimple(HandleLevel_Sum, "&&");
		}

		public ParserNode HandleLevel_Or()
		{
			return _HandleLevel_Op_BinarySimple(HandleLevel_And, "||");
		}

		public ParserNode HandleLevel_Filter()
		{
			return _HandleLevel_OpBase(HandleLevel_Or, new string[] { "|" }, (ParserNode LeftNode, String Operator) =>
			{
				Tokens.MoveNext();

				var Parameters = new List<ParserNode>();
				Parameters.Add(LeftNode);

				TemplateToken AcessToken = Tokens.ExpectTypeAndNext(typeof(IdentifierTemplateToken));
				if (Tokens.Current.Text == "(")
				{
					Tokens.MoveNext();

					while (Tokens.HasMore)
					{
						Parameters.Add(HandleLevel_Expression());
						String TokenType = Tokens.ExpectValueAndNext(",", ")").Text;
						if (TokenType == ")") break;
					}
				}
				return new ParserNodeFilter(AcessToken.Text, Parameters.ToArray());
			});
		}

		public ParserNode HandleLevel_Ternary()
		{
			return _HandleLevel_Op(
				HandleLevel_Filter,
				new string[] { "?" },
				(ParserNode ConditionNode, ParserNode TrueNode, String Operator) => {
					Tokens.ExpectValueAndNext(":");
					ParserNode FalseNode = HandleLevel_Sum();
					return new ParserNodeTernaryOperation(ConditionNode, TrueNode, FalseNode, Operator);
				}
			);
		}

		public ParserNode HandleLevel_Sli()
		{
			return _HandleLevel_Op(
				HandleLevel_Ternary,
				new string[] { ".." },
				(ParserNode LeftNode, ParserNode RightNode, String Operator) => { return new ParserNodeBinaryOperation(LeftNode, RightNode, Operator); }
			);
		}

		public ParserNode HandleLevel_Expression()
		{
			return HandleLevel_Sli();
		}

		public ParserNode HandleLevel_Tag()
		{
			return HandleLevel_Expression();
		}

		protected ParserNode HandleLevel_TagSpecial_For()
		{
			Tokens.MoveNext();

			String VarName = CurrentToken.Text;
			Tokens.MoveNext();
			Tokens.ExpectValueAndNext("in");
			ParserNode LoopIterator = HandleLevel_Expression();
			Tokens.ExpectValueAndNext("%}");

			ParserNode ElseBlock = new DummyParserNode();
			ParserNode BodyBlock = HandleLevel_Root();

			if (Tokens.Current.Text == "else")
			{
				Tokens.MoveNext();
				Tokens.ExpectValueAndNext("%}");
				ElseBlock = HandleLevel_Root();
			}

			Tokens.ExpectValueAndNext("endfor");
			Tokens.ExpectValueAndNext("%}");

			return new ForeachParserNode()
			{
				LoopIterator = LoopIterator,
				VarName = VarName,
				BodyBlock = BodyBlock,
				ElseBlock = ElseBlock,
			};
		}

		protected ParserNode HandleLevel_TagSpecial_Extends()
		{

			Tokens.MoveNext();
			ParserNodeExtends ParserNodeExtends = new ParserNodeExtends() {
				Parent = HandleLevel_Expression()
			};
			Tokens.ExpectValueAndNext("%}");
			return ParserNodeExtends;
		}

		ParserNode InsideABlock(String BlockName, Func<ParserNode> Callback)
		{
			String PreviousBlockName = CurrentBlockName;
			ParserNode ParserNode;
			try
			{
				CurrentBlockName = BlockName;
				IsInsideABlock++;
				ParserNode = Callback();
				return ParserNode;
			}
			finally
			{
				IsInsideABlock--;
				CurrentBlockName = PreviousBlockName;
			}
		}

		protected ParserNode HandleLevel_TagSpecial_Block()
		{
			Tokens.MoveNext();

			String BlockName = CurrentToken.Text;
			Tokens.MoveNext();
			Tokens.ExpectValueAndNext("%}");

			ParserNode BodyBlock = InsideABlock(BlockName, delegate()
			{
				return HandleLevel_Root();
			});

			Tokens.ExpectValueAndNext("endblock");
			Tokens.ExpectValueAndNext("%}");

			Blocks[BlockName] = BodyBlock;

			return new ParserNodeCallBlock(BlockName);
		}

		protected ParserNode HandleLevel_TagSpecial_Autoescape()
		{
			Tokens.MoveNext();

			ParserNode AutoEscapeExpression = HandleLevel_Expression();

			Tokens.ExpectValueAndNext("%}");

			ParserNode BodyBlock = HandleLevel_Root();

			Tokens.ExpectValueAndNext("endautoescape");
			Tokens.ExpectValueAndNext("%}");

			return new ParserNodeAutoescape(AutoEscapeExpression, BodyBlock);
		}

		public ParserNode HandleLevel_TagSpecial_If()
		{
			bool Alive = true;

			Tokens.MoveNext();

			ParserNode ConditionNode = HandleLevel_Expression();
			Tokens.ExpectValueAndNext("%}");

			ParserNode BodyIfNode = HandleLevel_Root();
			ParserNode BodyElseNode = new DummyParserNode();

			while (Alive)
			{
				switch (CurrentToken.Text)
				{
					case "endif":
						Tokens.MoveNext();
						Tokens.ExpectValueAndNext("%}");
						Alive = false;
						break;
					case "else":
						Tokens.MoveNext();
						Tokens.ExpectValueAndNext("%}");

						BodyElseNode = HandleLevel_Root();

						break;
					default:
						throw (new Exception(String.Format("Unprocessed Token Type '{0}'", CurrentTokenType)));
				}
			}

			return new ParserNodeIf()
			{
				ConditionNode = ConditionNode,
				BodyIfNode = BodyIfNode,
				BodyElseNode = BodyElseNode,
			};
		}

		private ParserNode HandleLevel_TagSpecial_Parent()
		{
			if (IsInsideABlock == 0) throw (new TemplateParentOutsideBlockException("Parent can only be called inside a block"));

			Tokens.MoveNext();
			Tokens.ExpectValueAndNext("%}");

			return new ParserNodeBlockParent(CurrentBlockName);
		}

		public ParserNode HandleLevel_TagSpecial()
		{
			string SpecialType = CurrentToken.Text;
			switch (SpecialType)
			{
				case "if"     : return HandleLevel_TagSpecial_If();
				case "block"  : return HandleLevel_TagSpecial_Block();
				case "autoescape": return HandleLevel_TagSpecial_Autoescape();
				case "parent": return HandleLevel_TagSpecial_Parent();
				case "for"    : return HandleLevel_TagSpecial_For();
				case "extends": return HandleLevel_TagSpecial_Extends();
				case "else":
				case "endif":
				case "endblock":
				case "endautoescape":
				case "endfor":
					throw (new Finalize_HandlingLevel_Root());
				default:
					throw (new Exception(String.Format("Unprocessed Tag Type '{0}'('{1}')", CurrentTokenType, CurrentToken.Text)));
			}
			//return HandleLevel_Expression();
		}

		public ParserNode HandleLevel_Root()
		{
			var ParserNodeContainer = new ParserNodeContainer();

			try
			{
				while (Tokens.HasMore)
				{
					//Console.WriteLine(CurrentToken);
					switch (CurrentTokenType)
					{
						case "RawTemplateToken":
							ParserNodeContainer.Add(new ParserNodeLiteral()
							{
								Text = ((RawTemplateToken)CurrentToken).Text,
							});
							Tokens.MoveNext();
							break;
						case "OpenTagTemplateToken":
							string OpenType = CurrentToken.Text;

							Tokens.MoveNext();

							switch (OpenType)
							{
								case "{{":
									ParserNodeContainer.Add(new ParserNodeOutputExpression() { Parent = HandleLevel_Tag() });
									Tokens.ExpectValueAndNext("}}");
									break;
								case "{%": {
									ParserNode ParserNode = HandleLevel_TagSpecial();
									ParserNodeContainer.Add(ParserNode);
									//ParserNode.Dump();
								} break;
							}
							break;
						default:
							throw (new Exception(String.Format("Unprocessed Token Type '{0}'('{1}')", CurrentTokenType, CurrentToken.Text)));
					}
				}
			}
			catch (Finalize_HandlingLevel_Root)
			{
			}

			return ParserNodeContainer;
		}
	}
}
