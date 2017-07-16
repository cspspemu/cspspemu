using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Nodes;
using SafeILGenerator.Ast.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace SafeILGenerator.Ast.Generators
{
	public class GeneratorIL : Generator<GeneratorIL>
	{
		protected MethodInfo MethodInfo;
		protected ILGenerator ILGenerator;
		protected bool GenerateLines;
		protected List<string> Lines = new List<string>();
		private Dictionary<AstLocal, LocalBuilder> _LocalBuilderCache = new Dictionary<AstLocal, LocalBuilder>();
		private Dictionary<AstLabel, Label> _LabelCache = new Dictionary<AstLabel, Label>();
		private Stack<AstNodeExpr> PlaceholderStack = new Stack<AstNodeExpr>();
		private int SwitchVarCount = 0;

		public GeneratorIL() : base()
		{
		}

		//LocalBuilder
		private LocalBuilder _GetLocalBuilderFromAstLocal(AstLocal AstLocal)
		{
			//AstLocal.Create
			if (!_LocalBuilderCache.ContainsKey(AstLocal))
			{
				var LocalBuilder = ILGenerator.DeclareLocal(AstLocal.Type);
				_LocalBuilderCache[AstLocal] = LocalBuilder;
				if (!(MethodInfo is DynamicMethod))
				{
					//LocalBuilder.SetLocalSymInfo(AstLocal.Name);
				}
			}
			return _LocalBuilderCache[AstLocal];
		}

		private Label _GetLabelFromAstLabel(AstLabel AstLabel)
		{
			if (!_LabelCache.ContainsKey(AstLabel))
			{
				_LabelCache[AstLabel] = ILGenerator.DefineLabel();
			}
			return _LabelCache[AstLabel];
		}

		public GeneratorIL Init(MethodInfo MethodInfo, ILGenerator ILGenerator, bool GenerateLines = false)
		{
			this.MethodInfo = MethodInfo;
			this.ILGenerator = ILGenerator;
			this.GenerateLines = GenerateLines;
			return this;
		}

		public override GeneratorIL Reset()
		{
			this._LocalBuilderCache = new Dictionary<AstLocal, LocalBuilder>();
			this._LabelCache = new Dictionary<AstLabel, Label>();
			this.PlaceholderStack = new Stack<AstNodeExpr>();
			this.Lines = new List<string>();
			this.SwitchVarCount = 0;
			return base.Reset();
		}

		public string GenerateToString(MethodInfo MethodInfo, AstNode AstNode)
		{
			return String.Join("\n", GenerateToStringList(MethodInfo, AstNode));
		}

		public string[] GenerateToStringList(MethodInfo MethodInfo, AstNode AstNode)
		{
			var ILGenerator = new DynamicMethod("test", MethodInfo.ReturnType, MethodInfo.GetParameters().Select(Param => Param.ParameterType).ToArray()).GetILGenerator();
			this.Init(MethodInfo, ILGenerator, GenerateLines: true).GenerateRoot(AstNode);
			return this.Lines.ToArray();
		}

		public string GenerateToString<TDelegate>(AstNode AstNode)
		{
			var MethodInfo = typeof(TDelegate).GetMethod("Invoke");
			return GenerateToString(MethodInfo, AstNode);
		}

		//static public TDelegate GenerateDelegate<TGenerator, TDelegate>(string MethodName, AstNode AstNode) where TGenerator : GeneratorIL, new()
		//{
		//	return new TGenerator()._GenerateDelegate<TDelegate>(MethodName, AstNode);
		//}

		public TDelegate GenerateDelegate<TDelegate>(string MethodName, AstNode AstNode)
		{
			var MethodInfo = typeof(TDelegate).GetMethod("Invoke");
			var DynamicMethod = new DynamicMethod(
				MethodName,
				MethodInfo.ReturnType,
				MethodInfo.GetParameters().Select(Parameter => Parameter.ParameterType).ToArray(),
				Assembly.GetExecutingAssembly().ManifestModule
			);
			var ILGenerator = DynamicMethod.GetILGenerator();
			this.Reset();
			this.Init(DynamicMethod, ILGenerator, GenerateLines: false);
			this.Generate(AstNode);
			return (TDelegate)(object)DynamicMethod.CreateDelegate(typeof(TDelegate));
		}

		protected void EmitHook(OpCode OpCode, object Param)
		{
			if (GenerateLines)
			{
				Lines.Add(String.Format("  {0} {1}", OpCode, Param));
			}
		}

		protected void EmitComment(string Text)
		{
			if (GenerateLines)
			{
				Lines.Add(String.Format("; {0}", Text));
			}
		}

		protected void DefineLabelHook()
		{
		}

		protected void MarkLabelHook(AstLabel Label)
		{
			if (GenerateLines)
			{
				Lines.Add(String.Format("Label_{0}:;", Label.Name));
			}
		}

		protected AstLabel DefineLabel(string Name) { DefineLabelHook(); return AstLabel.CreateLabel(Name); }
		protected void MarkLabel(AstLabel Label) { MarkLabelHook(Label); if (ILGenerator != null) ILGenerator.MarkLabel(_GetLabelFromAstLabel(Label)); }

		protected void Emit(OpCode OpCode) { EmitHook(OpCode, null); if (ILGenerator != null) ILGenerator.Emit(OpCode); }
		protected void Emit(OpCode OpCode, int Value) { EmitHook(OpCode, Value); if (ILGenerator != null) ILGenerator.Emit(OpCode, Value); }
		protected void Emit(OpCode OpCode, long Value) { EmitHook(OpCode, Value); if (ILGenerator != null) ILGenerator.Emit(OpCode, Value); }
		protected void Emit(OpCode OpCode, float Value) { EmitHook(OpCode, Value); if (ILGenerator != null) ILGenerator.Emit(OpCode, Value); }
		protected void Emit(OpCode OpCode, double Value) { EmitHook(OpCode, Value); if (ILGenerator != null) ILGenerator.Emit(OpCode, Value); }
		protected void Emit(OpCode OpCode, string Value) { EmitHook(OpCode, Value); if (ILGenerator != null) ILGenerator.Emit(OpCode, Value); }
		protected void Emit(OpCode OpCode, LocalBuilder Value) { EmitHook(OpCode, Value); if (ILGenerator != null) ILGenerator.Emit(OpCode, Value); }
		protected void Emit(OpCode OpCode, MethodInfo Value) { EmitHook(OpCode, Value); if (ILGenerator != null) ILGenerator.Emit(OpCode, Value); }
		protected void Emit(OpCode OpCode, ConstructorInfo Value) { EmitHook(OpCode, Value); if (ILGenerator != null) ILGenerator.Emit(OpCode, Value); }
		protected void Emit(OpCode OpCode, FieldInfo Value) { EmitHook(OpCode, Value); if (ILGenerator != null) ILGenerator.Emit(OpCode, Value); }
		protected void Emit(OpCode OpCode, Type Value) { EmitHook(OpCode, Value); if (ILGenerator != null) ILGenerator.Emit(OpCode, Value); }
		protected void Emit(OpCode OpCode, AstLabel Value) { EmitHook(OpCode, Value); if (ILGenerator != null) ILGenerator.Emit(OpCode, _GetLabelFromAstLabel(Value)); }
		protected void Emit(OpCode OpCode, params AstLabel[] Value) { EmitHook(OpCode, Value); if (ILGenerator != null) ILGenerator.Emit(OpCode, Value.Select(Item => _GetLabelFromAstLabel(Item)).ToArray()); }

		protected virtual void _Generate(AstNodeExprNull Null)
		{
			Emit(OpCodes.Ldnull);
		}

		protected virtual void _Generate(AstNodeExprImm Item)
		{
			var ItemType = AstUtils.GetSignedType(Item.Type);
			var ItemValue = Item.Value;

			if (ItemType.IsEnum)
			{
				ItemType = ItemType.GetEnumUnderlyingType();
				ItemValue = AstUtils.CastType(ItemValue, ItemType);
			}

			if (
				ItemType == typeof(int)
				|| ItemType == typeof(sbyte)
				|| ItemType == typeof(short)
				|| ItemType == typeof(bool)
			)
			{
				var Value = (int)Convert.ToInt64(ItemValue);
				switch (Value)
				{
					case -1: Emit(OpCodes.Ldc_I4_M1); break;
					case 0: Emit(OpCodes.Ldc_I4_0); break;
					case 1: Emit(OpCodes.Ldc_I4_1); break;
					case 2: Emit(OpCodes.Ldc_I4_2); break;
					case 3: Emit(OpCodes.Ldc_I4_3); break;
					case 4: Emit(OpCodes.Ldc_I4_4); break;
					case 5: Emit(OpCodes.Ldc_I4_5); break;
					case 6: Emit(OpCodes.Ldc_I4_6); break;
					case 7: Emit(OpCodes.Ldc_I4_7); break;
					case 8: Emit(OpCodes.Ldc_I4_8); break;
					default: Emit(OpCodes.Ldc_I4, Value); break;
				}
			}
			else if (ItemType == typeof(long) || ItemType == typeof(ulong))
			{
				Emit(OpCodes.Ldc_I8, Convert.ToInt64(ItemValue));
			}
			else if (ItemType == typeof(IntPtr))
			{
#if false
				Emit(OpCodes.Ldc_I8, ((IntPtr)Item.Value).ToInt64());
				Emit(OpCodes.Conv_I);
#else
				if (Environment.Is64BitProcess)
				{
					Emit(OpCodes.Ldc_I8, ((IntPtr)Item.Value).ToInt64());
					Emit(OpCodes.Conv_I);
				}
				else
				{
					Emit(OpCodes.Ldc_I4, ((IntPtr)Item.Value).ToInt32());
					Emit(OpCodes.Conv_I);
				}
#endif
			}
			else if (ItemType == typeof(float))
			{
				Emit(OpCodes.Ldc_R4, (float)Item.Value);
			}
			else if (Item.Value == null)
			{
				Emit(OpCodes.Ldnull);
			}
			else if (ItemType == typeof(string))
			{
				Emit(OpCodes.Ldstr, (string)Item.Value);
			}
			else if (ItemType == typeof(Type))
			{
				Emit(OpCodes.Ldtoken, (Type)Item.Value);
				Emit(OpCodes.Call, ((Func<RuntimeTypeHandle, Type>)Type.GetTypeFromHandle).Method);
				//IL_0005: call class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
			}
			else
			{
				throw (new NotImplementedException(String.Format("Can't handle immediate type {0}", ItemType)));
			}
		}

		protected virtual void _Generate(AstNodeStmComment Comment)
		{
			EmitComment(Comment.CommentText);
		}

		protected virtual void _Generate(AstNodeStmContainer Container)
		{
			foreach (var Node in Container.Nodes)
			{
				Generate(Node);
			}
		}

		protected virtual void _Generate(AstNodeExprArgument Argument)
		{
			int ArgumentIndex = Argument.AstArgument.Index;
			switch (ArgumentIndex)
			{
				case 0: Emit(OpCodes.Ldarg_0); break;
				case 1: Emit(OpCodes.Ldarg_1); break;
				case 2: Emit(OpCodes.Ldarg_2); break;
				case 3: Emit(OpCodes.Ldarg_3); break;
				default: Emit(OpCodes.Ldarg, ArgumentIndex); break;
			}
		}

		protected virtual void _Generate(AstNodeExprLocal Local)
		{
			var LocalBuilder = _GetLocalBuilderFromAstLocal(Local.AstLocal);

			switch (LocalBuilder.LocalIndex)
			{
				case 0: Emit(OpCodes.Ldloc_0); break;
				case 1: Emit(OpCodes.Ldloc_1); break;
				case 2: Emit(OpCodes.Ldloc_2); break;
				case 3: Emit(OpCodes.Ldloc_3); break;
				default: Emit(OpCodes.Ldloc, LocalBuilder); break;
			}
		}

		protected virtual void _Generate(AstNodeExprPropertyAccess PropertyAccess)
		{
			Generate(PropertyAccess.Instance);
			Emit(OpCodes.Callvirt, PropertyAccess.Property.GetMethod);
		}

		protected virtual void _Generate(AstNodeExprFieldAccess FieldAccess)
		{
			Generate(FieldAccess.Instance);
			//Console.WriteLine("{0}", FieldAccess.Field);
			//if (FieldAccess.Field.FieldType.IsValueType)
			//{
			//	Emit(OpCodes.Ldflda, FieldAccess.Field);
			//}
			//else
			{
				Emit(OpCodes.Ldfld, FieldAccess.Field);
			}
		}

		protected virtual void _Generate(AstNodeExprStaticFieldAccess FieldAccess)
		{
			Emit(OpCodes.Ldsfld, FieldAccess.Field);
		}

		protected virtual void _Generate(AstNodeExprArrayAccess ArrayAccess)
		{
			Generate(ArrayAccess.ArrayInstance);
			Generate(ArrayAccess.Index);

			if (false) { }

			else if (ArrayAccess.ElementType == typeof(byte)) Emit(OpCodes.Ldelem_U1);
			else if (ArrayAccess.ElementType == typeof(ushort)) Emit(OpCodes.Ldelem_U2);
			else if (ArrayAccess.ElementType == typeof(uint)) Emit(OpCodes.Ldelem_U4);
			else if (ArrayAccess.ElementType == typeof(ulong)) Emit(OpCodes.Ldelem_I8);

			else if (ArrayAccess.ElementType == typeof(sbyte)) Emit(OpCodes.Ldelem_I1);
			else if (ArrayAccess.ElementType == typeof(short)) Emit(OpCodes.Ldelem_I2);
			else if (ArrayAccess.ElementType == typeof(int)) Emit(OpCodes.Ldelem_I4);
			else if (ArrayAccess.ElementType == typeof(long)) Emit(OpCodes.Ldelem_I8);

			else if (ArrayAccess.ElementType.IsPointer) Emit(OpCodes.Ldelem_I);

			else Emit(OpCodes.Ldelem_Ref);
		}

		protected virtual void _Generate(AstNodeExprIndirect Indirect)
		{
			Generate(Indirect.PointerExpression);
			var PointerType = Indirect.PointerExpression.Type.GetElementType();

			if (false) { }

			else if (PointerType == typeof(byte)) Emit(OpCodes.Ldind_U1);
			else if (PointerType == typeof(ushort)) Emit(OpCodes.Ldind_U2);
			else if (PointerType == typeof(uint)) Emit(OpCodes.Ldind_U4);
			else if (PointerType == typeof(ulong)) Emit(OpCodes.Ldind_I8);

			else if (PointerType == typeof(sbyte)) Emit(OpCodes.Ldind_I1);
			else if (PointerType == typeof(short)) Emit(OpCodes.Ldind_I2);
			else if (PointerType == typeof(int)) Emit(OpCodes.Ldind_I4);
			else if (PointerType == typeof(long)) Emit(OpCodes.Ldind_I8);

			else if (PointerType == typeof(float)) Emit(OpCodes.Ldind_R4);
			else if (PointerType == typeof(double)) Emit(OpCodes.Ldind_R8);

			else throw (new NotImplementedException("Can't load indirect value"));
		}

		protected virtual void _Generate(AstNodeExprGetAddress GetAddress)
		{
			var AstNodeExprFieldAccess = (GetAddress.Expression as AstNodeExprFieldAccess);
			var AstNodeExprArgument = (GetAddress.Expression as AstNodeExprArgument);

			if (AstNodeExprFieldAccess != null)
			{
				Generate(AstNodeExprFieldAccess.Instance);
				Emit(OpCodes.Ldflda, AstNodeExprFieldAccess.Field);
			}
			else if (AstNodeExprArgument != null)
			{
				Emit(OpCodes.Ldarga, AstNodeExprArgument.AstArgument.Index);
			}
			else
			{
				throw (new NotImplementedException("Can't implement AstNodeExprGetAddress for '" + GetAddress.Expression.GetType() + "'"));
			}
		}

		protected virtual void _Generate(AstNodeExprSetGetLValuePlaceholder Placeholder)
		{
			var AstExpr = PlaceholderStack.Pop();
			if (AstExpr.Type != Placeholder.Type) throw (new Exception("Invalid Expression for placeholder " + AstExpr.Type + " != " + Placeholder.Type + "."));
			Generate(AstExpr);
		}

		protected virtual void _Generate(AstNodeExprSetGetLValue SetGetLValue)
		{
			Generate(SetGetLValue.GetExpression);
		}

		protected virtual void _Generate(AstNodeStmAssign Assign)
		{
			//Assign.Local.LocalBuilder.LocalIndex
			var AstNodeExprLocal = (Assign.LValue as AstNodeExprLocal);
			var AstNodeExprArgument = (Assign.LValue as AstNodeExprArgument);
			var AstNodeExprFieldAccess = (Assign.LValue as AstNodeExprFieldAccess);
			var AstNodeExprStaticFieldAccess = (Assign.LValue as AstNodeExprStaticFieldAccess);
			var AstNodeExprIndirect = (Assign.LValue as AstNodeExprIndirect);
			var AstNodeExprArrayAccess = (Assign.LValue as AstNodeExprArrayAccess);
			var AstNodeExprPropertyAccess = (Assign.LValue as AstNodeExprPropertyAccess);
			var AstNodeExprSetGetLValue = (Assign.LValue as AstNodeExprSetGetLValue);
			

			if (AstNodeExprLocal != null)
			{
				Generate(Assign.Value);
				Emit(OpCodes.Stloc, _GetLocalBuilderFromAstLocal(AstNodeExprLocal.AstLocal));
			}
			else if (AstNodeExprArgument != null)
			{
				Generate(Assign.Value);
				Emit(OpCodes.Starg, AstNodeExprArgument.AstArgument.Index);
			}
			else if (AstNodeExprFieldAccess != null)
			{
				Generate(AstNodeExprFieldAccess.Instance);
				Generate(Assign.Value);
				Emit(OpCodes.Stfld, AstNodeExprFieldAccess.Field);
			}
			else if (AstNodeExprStaticFieldAccess != null)
			{
				Generate(Assign.Value);
				Emit(OpCodes.Stsfld, AstNodeExprStaticFieldAccess.Field);
			}
			else if (AstNodeExprArrayAccess != null)
			{
				Generate(AstNodeExprArrayAccess.ArrayInstance);
				Generate(AstNodeExprArrayAccess.Index);
				Generate(Assign.Value);
				Emit(OpCodes.Stelem, AstNodeExprArrayAccess.ArrayInstance.Type.GetElementType());
			}
			else if (AstNodeExprIndirect != null)
			{
				var PointerType = AstUtils.GetSignedType(AstNodeExprIndirect.PointerExpression.Type.GetElementType());

				Generate(AstNodeExprIndirect.PointerExpression);
				Generate(Assign.Value);

				if (PointerType == typeof(sbyte)) Emit(OpCodes.Stind_I1);
				else if (PointerType == typeof(short)) Emit(OpCodes.Stind_I2);
				else if (PointerType == typeof(int)) Emit(OpCodes.Stind_I4);
				else if (PointerType == typeof(long)) Emit(OpCodes.Stind_I8);
				else if (PointerType == typeof(float)) Emit(OpCodes.Stind_R4);
				else if (PointerType == typeof(double)) Emit(OpCodes.Stind_R8);
				else if (PointerType == typeof(bool)) Emit(OpCodes.Stind_I1);
				else throw (new NotImplementedException("Can't store indirect value"));
			}
			else if (AstNodeExprPropertyAccess != null)
			{
				Generate(AstNodeExprPropertyAccess.Instance);
				Generate(Assign.Value);
				Emit(OpCodes.Callvirt, AstNodeExprPropertyAccess.Property.SetMethod);
			}
			else if (AstNodeExprSetGetLValue != null)
			{
				PlaceholderStack.Push(Assign.Value);
				Generate(AstNodeExprSetGetLValue.SetExpression);
				if (AstNodeExprSetGetLValue.SetExpression.Type != typeof(void))
				{
					Emit(OpCodes.Pop);
				}
			}
			else
			{
				throw (new NotImplementedException("Not implemented AstNodeStmAssign LValue: " + Assign.LValue.GetType()));
			}
			//Assign.Local
		}

		protected virtual void _Generate(AstNodeStmReturn Return)
		{
			var ExpressionType = (Return.Expression != null) ? Return.Expression.Type : typeof(void);

			if (ExpressionType != MethodInfo.ReturnType)
			{
				throw (new Exception(String.Format("Return type mismatch {0} != {1}", ExpressionType, MethodInfo.ReturnType)));
			}

			if (Return.Expression != null) Generate(Return.Expression);
			Emit(OpCodes.Ret);
		}

		protected virtual void _Generate(AstNodeExprCallTailCall Call)
		{
			Generate(Call.Call);
			Emit(OpCodes.Ret);
		}

		protected virtual void _Generate(AstNodeExprCallStatic Call)
		{
			if (Call.MethodInfo.CallingConvention.HasFlag(CallingConventions.HasThis))
			{
				throw (new Exception("CallString calling convention shouldn't have this '" + Call.MethodInfo + "'"));
			}
			switch (Call.MethodInfo.CallingConvention & CallingConventions.Any)
			{
				case CallingConventions.Standard:
					foreach (var Parameter in Call.Parameters) Generate(Parameter);
					if (Call.IsTail) Emit(OpCodes.Tailcall);
					Emit(OpCodes.Call, Call.MethodInfo);
					break;
				default:
					throw (new Exception(String.Format("Can't handle calling convention {0}", Call.MethodInfo.CallingConvention)));
			}
			
		}

		protected virtual void _Generate(AstNodeExprCallDelegate Call)
		{
			_Generate((AstNodeExprCallInstance)Call);
		}

		protected virtual void _Generate(AstNodeExprCallInstance Call)
		{
			if (!Call.MethodInfo.CallingConvention.HasFlag(CallingConventions.HasThis))
			{
				throw(new Exception("CallInstance calling convention should have this"));
			}
			switch (Call.MethodInfo.CallingConvention & CallingConventions.Any)
			{
				case CallingConventions.Standard:
					Generate(Call.Instance);
					foreach (var Parameter in Call.Parameters) Generate(Parameter);
					if (Call.IsTail) Emit(OpCodes.Tailcall);
					Emit(OpCodes.Callvirt, Call.MethodInfo);
					break;
				default:
					throw (new Exception(String.Format("Can't handle calling convention {0}", Call.MethodInfo.CallingConvention)));
			}
		}

		protected virtual void _GenerateCastToType(Type CastedType)
		{
			if (false) { }
			else if (CastedType == typeof(sbyte)) Emit(OpCodes.Conv_I1);
			else if (CastedType == typeof(short)) Emit(OpCodes.Conv_I2);
			else if (CastedType == typeof(int)) Emit(OpCodes.Conv_I4);
			else if (CastedType == typeof(long)) Emit(OpCodes.Conv_I8);

			else if (CastedType == typeof(byte)) Emit(OpCodes.Conv_U1);
			else if (CastedType == typeof(ushort)) Emit(OpCodes.Conv_U2);
			else if (CastedType == typeof(uint)) Emit(OpCodes.Conv_U4);
			else if (CastedType == typeof(ulong)) Emit(OpCodes.Conv_U8);

			else if (CastedType == typeof(float)) Emit(OpCodes.Conv_R4);
			else if (CastedType == typeof(double)) Emit(OpCodes.Conv_R8);

            else if (CastedType == typeof(bool)) Emit(OpCodes.Conv_I1);

			else if (CastedType.IsPointer) Emit(OpCodes.Conv_I);
			else if (CastedType.IsByRef) Emit(OpCodes.Conv_I);

			else if (CastedType.IsPrimitive)
			{
				throw (new NotImplementedException("Not implemented cast other primitives"));
			}

			else if (CastedType.IsEnum)
			{
				_GenerateCastToType(CastedType.GetEnumUnderlyingType());
				//throw (new NotImplementedException("Not implemented cast other primitives"));
			}

			else
			{
				Emit(OpCodes.Castclass, CastedType);
				//throw (new NotImplementedException("Not implemented cast class"));
			}
		}

		protected virtual void _Generate(AstNodeExprCast Cast)
		{
			var CastedType = Cast.CastedType;

			Generate(Cast.Expr);

			if (Cast.Explicit)
			{
				_GenerateCastToType(CastedType);
			}
		}

		protected virtual void _Generate(AstNodeExprTerop Terop)
		{
			if (Terop.True.Type != Terop.False.Type) throw (new InvalidOperationException(String.Format("AstNodeExprTerop '?:' types must match {0} != {1}", Terop.True.Type, Terop.False.Type)));
			var TernaryType = Terop.True.Type;
			var TernaryTempAstLocal = AstLocal.Create(TernaryType);

			Generate(new AstNodeStmIfElse(
				Terop.Cond,
				new AstNodeStmAssign(new AstNodeExprLocal(TernaryTempAstLocal), Terop.True),
				new AstNodeStmAssign(new AstNodeExprLocal(TernaryTempAstLocal), Terop.False)
			));

			Generate(new AstNodeExprLocal(TernaryTempAstLocal));
		}

		protected virtual void _Generate(AstNodeStmIfElse IfElse)
		{
			var AfterIfLabel = DefineLabel("AfterIf");

			Generate(IfElse.Condition);
			Emit(OpCodes.Brfalse, AfterIfLabel);
			Generate(IfElse.True);

			if (IfElse.False != null)
			{
				var AfterElseLabel = DefineLabel("AfterElse");
				Emit(OpCodes.Br, AfterElseLabel);

				MarkLabel(AfterIfLabel);

				Generate(IfElse.False);

				MarkLabel(AfterElseLabel);
			}
			else
			{
				MarkLabel(AfterIfLabel);
			}
		}

		protected virtual void _Generate(AstNodeExprBinop Item)
		{
			var LeftType = Item.LeftNode.Type;
			var RightType = Item.RightNode.Type;

			//if (LeftType != RightType) throw(new Exception(String.Format("BinaryOp Type mismatch ({0}) != ({1})", LeftType, RightType)));

			//Item.GetType().GenericTypeArguments[0]
			this.Generate(Item.LeftNode);
			this.Generate(Item.RightNode);

			//switch (Item.Operator)
			//{
			//	case "||":
			//	case "&&":
			//		if (LeftType != typeof(bool) || RightType != typeof(bool))
			//		{
			//			throw(new InvalidOperationException(String.Format("Operator '{0}' requires boolean types but found {1}, {2}", Item.Operator, LeftType, RightType)));
			//		}
			//		break;
			//}

			switch (Item.Operator)
			{
				case "+" : Emit(AstUtils.IsTypeSigned(LeftType) ? OpCodes.Add : OpCodes.Add); break;
				case "-" : Emit(AstUtils.IsTypeSigned(LeftType) ? OpCodes.Sub : OpCodes.Sub); break;
				case "*" : Emit(AstUtils.IsTypeSigned(LeftType) ? OpCodes.Mul : OpCodes.Mul); break;
				case "/" : Emit(AstUtils.IsTypeSigned(LeftType) ? OpCodes.Div : OpCodes.Div_Un); break;
				case "%" : Emit(AstUtils.IsTypeSigned(LeftType) ? OpCodes.Rem : OpCodes.Rem_Un); break;
				case "==": Emit(OpCodes.Ceq); break;
				case "!=": Emit(OpCodes.Ceq); Emit(OpCodes.Ldc_I4_0); Emit(OpCodes.Ceq); break;
				case "<" : Emit(AstUtils.IsTypeSigned(LeftType) ? OpCodes.Clt : OpCodes.Clt_Un); break;
				case ">" : Emit(AstUtils.IsTypeSigned(LeftType) ? OpCodes.Cgt : OpCodes.Cgt_Un); break;
				case "<=": Emit(AstUtils.IsTypeSigned(LeftType) ? OpCodes.Cgt : OpCodes.Cgt_Un); Emit(OpCodes.Ldc_I4_0); Emit(OpCodes.Ceq); break;
				case ">=": Emit(AstUtils.IsTypeSigned(LeftType) ? OpCodes.Clt : OpCodes.Clt_Un); Emit(OpCodes.Ldc_I4_0); Emit(OpCodes.Ceq); break;
				case "&":
				case "&&": Emit(OpCodes.And); break;
				case "|":
				case "||": Emit(OpCodes.Or); break;
				case "^" : Emit(OpCodes.Xor); break;
				case "<<": Emit(OpCodes.Shl); break;
				case ">>": Emit(AstUtils.IsTypeSigned(LeftType) ? OpCodes.Shr : OpCodes.Shr_Un); break;
				default: throw(new NotImplementedException(String.Format("Not implemented operator '{0}'", Item.Operator)));
			}
		}

		protected virtual void _Generate(AstNodeStmExpr Stat)
		{
			var ExpressionType = Stat.AstNodeExpr.Type;
			Generate(Stat.AstNodeExpr);

			if (ExpressionType != typeof(void))
			{
				Emit(OpCodes.Pop);
			}
		}

		protected virtual void _Generate(AstNodeStmEmpty Empty)
		{
		}

		protected virtual void _Generate(AstNodeStmLabel Label)
		{
			MarkLabel(Label.AstLabel);
		}

		protected virtual void _Generate(AstNodeStmGotoIfTrue Goto)
		{
			Generate(Goto.Condition);
			Emit(OpCodes.Brtrue, Goto.AstLabel);
		}

		protected virtual void _Generate(AstNodeStmGotoIfFalse Goto)
		{
			Generate(Goto.Condition);
			Emit(OpCodes.Brfalse, Goto.AstLabel);
		}

		protected virtual void _Generate(AstNodeStmGotoAlways Goto)
		{
			Emit(OpCodes.Br, Goto.AstLabel);
		}

		protected virtual void _Generate(AstNodeExprUnop Item)
		{
			var RightType = Item.RightNode.Type;

			this.Generate(Item.RightNode);

			switch (Item.Operator)
			{
				case "~": Emit(OpCodes.Not); break;
				case "+": break;
				case "-": Emit(OpCodes.Neg); break;
				case "!": Emit(OpCodes.Ldc_I4_0); Emit(OpCodes.Ceq); break;
				default: throw(new NotImplementedException(String.Format("Not implemented operator '{0}'", Item.Operator)));
			}
		}

		protected virtual void _Generate(AstNodeStmSwitch Switch)
		{
			var AllCaseValues = Switch.Cases.Select(Case => Case.CaseValue);
			if (AllCaseValues.Count() != AllCaseValues.Distinct().Count())
			{
				throw(new Exception("Repeated case in switch!"));
			}

			// Check types and unique values.

			var EndCasesLabel = AstLabel.CreateLabel("EndCasesLabel");
			var DefaultLabel = AstLabel.CreateLabel("DefaultLabel");

			if (Switch.Cases.Length > 0)
			{
				var CommonType = Switch.Cases.First().CaseValue.GetType();
				if (!Switch.Cases.All(Case => Case.CaseValue.GetType() == CommonType))
				{
					throw(new Exception("All cases should have the same type"));
				}

				bool DoneSpecialized = false;

				// Specialized constant-time integer switch (if possible)
				if (AstUtils.IsIntegerType(CommonType))
				{
					var CommonMin = Switch.Cases.Min(Case => AstUtils.CastType<long>(Case.CaseValue));
					var CommonMax = Switch.Cases.Max(Case => AstUtils.CastType<long>(Case.CaseValue));
					var CasesLength = (CommonMax - CommonMin) + 1;

					// No processing tables greater than 4096 elements.
					if (CasesLength <= 4096)
					{
						var Labels = new AstLabel[CasesLength];
						for (int n = 0; n < CasesLength; n++) Labels[n] = DefaultLabel;

						foreach (var Case in Switch.Cases)
						{
							long RealValue = AstUtils.CastType<long>(Case.CaseValue);
							long Offset = RealValue - CommonMin;
							Labels[Offset] = AstLabel.CreateLabel("Case_" + RealValue);
						}

						/*
						//var SwitchVarLocal = AstLocal.Create(AllCaseValues.First().GetType(), "SwitchVarLocal" + SwitchVarCount++);
						//Generate(new AstNodeStmAssign(new AstNodeExprLocal(SwitchVarLocal), Switch.SwitchValue - new AstNodeExprCast(CommonType, CommonMin)));
						//Generate(new AstNodeStmIfElse(new AstNodeExprBinop(new AstNodeExprLocal(SwitchVarLocal), "<", 0), new AstNodeStmGotoAlways(DefaultLabel)));
						//Generate(new AstNodeStmIfElse(new AstNodeExprBinop(new AstNodeExprLocal(SwitchVarLocal), ">=", CasesLength), new AstNodeStmGotoAlways(DefaultLabel)));
						//Generate(new AstNodeExprLocal(SwitchVarLocal));
						*/

						Generate(Switch.SwitchValue - new AstNodeExprCast(CommonType, CommonMin));
						Emit(OpCodes.Switch, Labels);
						Generate(new AstNodeStmGotoAlways(DefaultLabel));
						foreach (var Case in Switch.Cases)
						{
							long RealValue = AstUtils.CastType<long>(Case.CaseValue);
							long Offset = RealValue - CommonMin;
							Generate(new AstNodeStmLabel(Labels[Offset]));
							{
								Generate(Case.Code);
							}
							Generate(new AstNodeStmGotoAlways(EndCasesLabel));
						}

						DoneSpecialized = true;
					}
					else
					{
						// TODO: find a common shift and masks for all the values to reduce CasesLength.
						// TODO: On too large test cases, split them recursively in:
						// if (Var < Half) { switch(Var - LowerPartMin) { ... } } else { switch(Var - Half - UpperPartMin) { ... } }
					}

				}
				// Specialized switch for strings (checking length, then hash, then contents)
				else if (CommonType == typeof(string))
				{
					// TODO!
				}
				
				// Generic if/else
				if (!DoneSpecialized)
				{
					var SwitchVarLocal = AstLocal.Create(AllCaseValues.First().GetType(), "SwitchVarLocal" + SwitchVarCount++);
					Generate(new AstNodeStmAssign(new AstNodeExprLocal(SwitchVarLocal), Switch.SwitchValue));
					//Switch.Cases
					foreach (var Case in Switch.Cases)
					{
						var LabelSkipThisCase = AstLabel.CreateLabel("LabelCase" + Case.CaseValue);
						Generate(new AstNodeStmGotoIfFalse(LabelSkipThisCase, new AstNodeExprBinop(new AstNodeExprLocal(SwitchVarLocal), "==", new AstNodeExprImm(Case.CaseValue))));
						Generate(Case.Code);
						Generate(new AstNodeStmGotoAlways(EndCasesLabel));
						Generate(new AstNodeStmLabel(LabelSkipThisCase));
					}
				}
			}

			Generate(new AstNodeStmLabel(DefaultLabel));
			if (Switch.CaseDefault != null)
			{
				Generate(Switch.CaseDefault.Code);
			}

			Generate(new AstNodeStmLabel(EndCasesLabel));
		}

		protected virtual void _Generate(AstNodeExprNewArray NewArray)
		{
			var TempArrayLocal = AstLocal.Create(NewArray.Type, "$TempArray");
			Generate(new AstNodeExprImm(NewArray.Length));
			Emit(OpCodes.Newarr, NewArray.ElementType);
			Emit(OpCodes.Stloc, _GetLocalBuilderFromAstLocal(TempArrayLocal));
			for (int n = 0; n < NewArray.Length; n++)
			{
				Generate(new AstNodeStmAssign(new AstNodeExprArrayAccess(new AstNodeExprLocal(TempArrayLocal), n), NewArray.Values[n]));
			}
			Generate(new AstNodeExprLocal(TempArrayLocal));
		}

		protected virtual void _Generate(AstNodeExprNew AstNodeExprNew)
		{
			var Constructor = AstNodeExprNew.Type.GetConstructor(AstNodeExprNew.Params.Select(Param => Param.Type).ToArray());
			foreach (var Param in AstNodeExprNew.Params)
			{
				Generate(Param);
			}
			Emit(OpCodes.Newobj, Constructor);
		}

		protected virtual void _Generate(AstNodeStmThrow AstNodeStmThrow)
		{
			Generate(AstNodeStmThrow.AstNodeExpr);
			Emit(OpCodes.Throw);
		}
	}
}
