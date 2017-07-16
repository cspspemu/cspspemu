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
        protected ILGenerator IlGenerator;
        protected bool GenerateLines;
        protected List<string> Lines = new List<string>();
        private Dictionary<AstLocal, LocalBuilder> _localBuilderCache = new Dictionary<AstLocal, LocalBuilder>();
        private Dictionary<AstLabel, Label> _labelCache = new Dictionary<AstLabel, Label>();
        private Stack<AstNodeExpr> _placeholderStack = new Stack<AstNodeExpr>();
        private int _switchVarCount = 0;

        public GeneratorIL() : base()
        {
        }

        //LocalBuilder
        private LocalBuilder _GetLocalBuilderFromAstLocal(AstLocal astLocal)
        {
            //AstLocal.Create
            if (!_localBuilderCache.ContainsKey(astLocal))
            {
                var localBuilder = IlGenerator.DeclareLocal(astLocal.Type);
                _localBuilderCache[astLocal] = localBuilder;
                if (!(MethodInfo is DynamicMethod))
                {
                    //LocalBuilder.SetLocalSymInfo(AstLocal.Name);
                }
            }
            return _localBuilderCache[astLocal];
        }

        private Label _GetLabelFromAstLabel(AstLabel astLabel)
        {
            if (!_labelCache.ContainsKey(astLabel))
            {
                _labelCache[astLabel] = IlGenerator.DefineLabel();
            }
            return _labelCache[astLabel];
        }

        public GeneratorIL Init(MethodInfo methodInfo, ILGenerator ilGenerator, bool generateLines = false)
        {
            this.MethodInfo = methodInfo;
            this.IlGenerator = ilGenerator;
            this.GenerateLines = generateLines;
            return this;
        }

        public override GeneratorIL Reset()
        {
            this._localBuilderCache = new Dictionary<AstLocal, LocalBuilder>();
            this._labelCache = new Dictionary<AstLabel, Label>();
            this._placeholderStack = new Stack<AstNodeExpr>();
            this.Lines = new List<string>();
            this._switchVarCount = 0;
            return base.Reset();
        }

        public string GenerateToString(MethodInfo methodInfo, AstNode astNode)
        {
            return String.Join("\n", GenerateToStringList(methodInfo, astNode));
        }

        public string[] GenerateToStringList(MethodInfo methodInfo, AstNode astNode)
        {
            var ilGenerator = new DynamicMethod("test", methodInfo.ReturnType,
                methodInfo.GetParameters().Select(param => param.ParameterType).ToArray()).GetILGenerator();
            Init(methodInfo, ilGenerator, generateLines: true).GenerateRoot(astNode);
            return Lines.ToArray();
        }

        public string GenerateToString<TDelegate>(AstNode astNode)
        {
            var methodInfo = typeof(TDelegate).GetMethod("Invoke");
            return GenerateToString(methodInfo, astNode);
        }

        //static public TDelegate GenerateDelegate<TGenerator, TDelegate>(string MethodName, AstNode AstNode) where TGenerator : GeneratorIL, new()
        //{
        //	return new TGenerator()._GenerateDelegate<TDelegate>(MethodName, AstNode);
        //}

        public TDelegate GenerateDelegate<TDelegate>(string methodName, AstNode astNode)
        {
            var methodInfo = typeof(TDelegate).GetMethod("Invoke");
            var dynamicMethod = new DynamicMethod(
                methodName,
                methodInfo.ReturnType,
                methodInfo.GetParameters().Select(parameter => parameter.ParameterType).ToArray(),
                Assembly.GetExecutingAssembly().ManifestModule
            );
            var ilGenerator = dynamicMethod.GetILGenerator();
            Reset();
            Init(dynamicMethod, ilGenerator, generateLines: false);
            Generate(astNode);
            return (TDelegate) (object) dynamicMethod.CreateDelegate(typeof(TDelegate));
        }

        protected void EmitHook(OpCode opCode, object param)
        {
            if (GenerateLines)
            {
                Lines.Add(String.Format("  {0} {1}", opCode, param));
            }
        }

        protected void EmitComment(string text)
        {
            if (GenerateLines)
            {
                Lines.Add(String.Format("; {0}", text));
            }
        }

        protected void DefineLabelHook()
        {
        }

        protected void MarkLabelHook(AstLabel label)
        {
            if (GenerateLines)
            {
                Lines.Add(String.Format("Label_{0}:;", label.Name));
            }
        }

        protected AstLabel DefineLabel(string name)
        {
            DefineLabelHook();
            return AstLabel.CreateLabel(name);
        }

        protected void MarkLabel(AstLabel label)
        {
            MarkLabelHook(label);
            if (IlGenerator != null) IlGenerator.MarkLabel(_GetLabelFromAstLabel(label));
        }

        protected void Emit(OpCode opCode)
        {
            EmitHook(opCode, null);
            if (IlGenerator != null) IlGenerator.Emit(opCode);
        }

        protected void Emit(OpCode opCode, int value)
        {
            EmitHook(opCode, value);
            if (IlGenerator != null) IlGenerator.Emit(opCode, value);
        }

        protected void Emit(OpCode opCode, long value)
        {
            EmitHook(opCode, value);
            if (IlGenerator != null) IlGenerator.Emit(opCode, value);
        }

        protected void Emit(OpCode opCode, float value)
        {
            EmitHook(opCode, value);
            if (IlGenerator != null) IlGenerator.Emit(opCode, value);
        }

        protected void Emit(OpCode opCode, double value)
        {
            EmitHook(opCode, value);
            if (IlGenerator != null) IlGenerator.Emit(opCode, value);
        }

        protected void Emit(OpCode opCode, string value)
        {
            EmitHook(opCode, value);
            if (IlGenerator != null) IlGenerator.Emit(opCode, value);
        }

        protected void Emit(OpCode opCode, LocalBuilder value)
        {
            EmitHook(opCode, value);
            if (IlGenerator != null) IlGenerator.Emit(opCode, value);
        }

        protected void Emit(OpCode opCode, MethodInfo value)
        {
            EmitHook(opCode, value);
            if (IlGenerator != null) IlGenerator.Emit(opCode, value);
        }

        protected void Emit(OpCode opCode, ConstructorInfo value)
        {
            EmitHook(opCode, value);
            if (IlGenerator != null) IlGenerator.Emit(opCode, value);
        }

        protected void Emit(OpCode opCode, FieldInfo value)
        {
            EmitHook(opCode, value);
            if (IlGenerator != null) IlGenerator.Emit(opCode, value);
        }

        protected void Emit(OpCode opCode, Type value)
        {
            EmitHook(opCode, value);
            if (IlGenerator != null) IlGenerator.Emit(opCode, value);
        }

        protected void Emit(OpCode opCode, AstLabel value)
        {
            EmitHook(opCode, value);
            if (IlGenerator != null) IlGenerator.Emit(opCode, _GetLabelFromAstLabel(value));
        }

        protected void Emit(OpCode opCode, params AstLabel[] value)
        {
            EmitHook(opCode, value);
            if (IlGenerator != null)
                IlGenerator.Emit(opCode, value.Select(Item => _GetLabelFromAstLabel(Item)).ToArray());
        }

        protected virtual void _Generate(AstNodeExprNull Null)
        {
            Emit(OpCodes.Ldnull);
        }

        protected virtual void _Generate(AstNodeExprImm item)
        {
            var itemType = AstUtils.GetSignedType(item.Type);
            var itemValue = item.Value;

            if (itemType.IsEnum)
            {
                itemType = itemType.GetEnumUnderlyingType();
                itemValue = AstUtils.CastType(itemValue, itemType);
            }

            if (
                itemType == typeof(int)
                || itemType == typeof(sbyte)
                || itemType == typeof(short)
                || itemType == typeof(bool)
            )
            {
                var value = (int) Convert.ToInt64(itemValue);
                switch (value)
                {
                    case -1:
                        Emit(OpCodes.Ldc_I4_M1);
                        break;
                    case 0:
                        Emit(OpCodes.Ldc_I4_0);
                        break;
                    case 1:
                        Emit(OpCodes.Ldc_I4_1);
                        break;
                    case 2:
                        Emit(OpCodes.Ldc_I4_2);
                        break;
                    case 3:
                        Emit(OpCodes.Ldc_I4_3);
                        break;
                    case 4:
                        Emit(OpCodes.Ldc_I4_4);
                        break;
                    case 5:
                        Emit(OpCodes.Ldc_I4_5);
                        break;
                    case 6:
                        Emit(OpCodes.Ldc_I4_6);
                        break;
                    case 7:
                        Emit(OpCodes.Ldc_I4_7);
                        break;
                    case 8:
                        Emit(OpCodes.Ldc_I4_8);
                        break;
                    default:
                        Emit(OpCodes.Ldc_I4, value);
                        break;
                }
            }
            else if (itemType == typeof(long) || itemType == typeof(ulong))
            {
                Emit(OpCodes.Ldc_I8, Convert.ToInt64(itemValue));
            }
            else if (itemType == typeof(IntPtr))
            {
#if false
				Emit(OpCodes.Ldc_I8, ((IntPtr)Item.Value).ToInt64());
				Emit(OpCodes.Conv_I);
#else
                if (Environment.Is64BitProcess)
                {
                    Emit(OpCodes.Ldc_I8, ((IntPtr) item.Value).ToInt64());
                    Emit(OpCodes.Conv_I);
                }
                else
                {
                    Emit(OpCodes.Ldc_I4, ((IntPtr) item.Value).ToInt32());
                    Emit(OpCodes.Conv_I);
                }
#endif
            }
            else if (itemType == typeof(float))
            {
                Emit(OpCodes.Ldc_R4, (float) item.Value);
            }
            else if (item.Value == null)
            {
                Emit(OpCodes.Ldnull);
            }
            else if (itemType == typeof(string))
            {
                Emit(OpCodes.Ldstr, (string) item.Value);
            }
            else if (itemType == typeof(Type))
            {
                Emit(OpCodes.Ldtoken, (Type) item.Value);
                Emit(OpCodes.Call, ((Func<RuntimeTypeHandle, Type>) Type.GetTypeFromHandle).Method);
                //IL_0005: call class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
            }
            else
            {
                throw (new NotImplementedException(String.Format("Can't handle immediate type {0}", itemType)));
            }
        }

        protected virtual void _Generate(AstNodeStmComment comment)
        {
            EmitComment(comment.CommentText);
        }

        protected virtual void _Generate(AstNodeStmContainer container)
        {
            foreach (var node in container.Nodes)
            {
                Generate(node);
            }
        }

        protected virtual void _Generate(AstNodeExprArgument argument)
        {
            var argumentIndex = argument.AstArgument.Index;
            switch (argumentIndex)
            {
                case 0:
                    Emit(OpCodes.Ldarg_0);
                    break;
                case 1:
                    Emit(OpCodes.Ldarg_1);
                    break;
                case 2:
                    Emit(OpCodes.Ldarg_2);
                    break;
                case 3:
                    Emit(OpCodes.Ldarg_3);
                    break;
                default:
                    Emit(OpCodes.Ldarg, argumentIndex);
                    break;
            }
        }

        protected virtual void _Generate(AstNodeExprLocal local)
        {
            var localBuilder = _GetLocalBuilderFromAstLocal(local.AstLocal);

            switch (localBuilder.LocalIndex)
            {
                case 0:
                    Emit(OpCodes.Ldloc_0);
                    break;
                case 1:
                    Emit(OpCodes.Ldloc_1);
                    break;
                case 2:
                    Emit(OpCodes.Ldloc_2);
                    break;
                case 3:
                    Emit(OpCodes.Ldloc_3);
                    break;
                default:
                    Emit(OpCodes.Ldloc, localBuilder);
                    break;
            }
        }

        protected virtual void _Generate(AstNodeExprPropertyAccess propertyAccess)
        {
            Generate(propertyAccess.Instance);
            Emit(OpCodes.Callvirt, propertyAccess.Property.GetMethod);
        }

        protected virtual void _Generate(AstNodeExprFieldAccess fieldAccess)
        {
            Generate(fieldAccess.Instance);
            //Console.WriteLine("{0}", FieldAccess.Field);
            //if (FieldAccess.Field.FieldType.IsValueType)
            //{
            //	Emit(OpCodes.Ldflda, FieldAccess.Field);
            //}
            //else
            {
                Emit(OpCodes.Ldfld, fieldAccess.Field);
            }
        }

        protected virtual void _Generate(AstNodeExprStaticFieldAccess fieldAccess)
        {
            Emit(OpCodes.Ldsfld, fieldAccess.Field);
        }

        protected virtual void _Generate(AstNodeExprArrayAccess arrayAccess)
        {
            Generate(arrayAccess.ArrayInstance);
            Generate(arrayAccess.Index);

            if (false)
            {
            }

            else if (arrayAccess.ElementType == typeof(byte)) Emit(OpCodes.Ldelem_U1);
            else if (arrayAccess.ElementType == typeof(ushort)) Emit(OpCodes.Ldelem_U2);
            else if (arrayAccess.ElementType == typeof(uint)) Emit(OpCodes.Ldelem_U4);
            else if (arrayAccess.ElementType == typeof(ulong)) Emit(OpCodes.Ldelem_I8);

            else if (arrayAccess.ElementType == typeof(sbyte)) Emit(OpCodes.Ldelem_I1);
            else if (arrayAccess.ElementType == typeof(short)) Emit(OpCodes.Ldelem_I2);
            else if (arrayAccess.ElementType == typeof(int)) Emit(OpCodes.Ldelem_I4);
            else if (arrayAccess.ElementType == typeof(long)) Emit(OpCodes.Ldelem_I8);

            else if (arrayAccess.ElementType.IsPointer) Emit(OpCodes.Ldelem_I);

            else Emit(OpCodes.Ldelem_Ref);
        }

        protected virtual void _Generate(AstNodeExprIndirect indirect)
        {
            Generate(indirect.PointerExpression);
            var pointerType = indirect.PointerExpression.Type.GetElementType();

            if (pointerType == typeof(byte)) Emit(OpCodes.Ldind_U1);
            else if (pointerType == typeof(ushort)) Emit(OpCodes.Ldind_U2);
            else if (pointerType == typeof(uint)) Emit(OpCodes.Ldind_U4);
            else if (pointerType == typeof(ulong)) Emit(OpCodes.Ldind_I8);

            else if (pointerType == typeof(sbyte)) Emit(OpCodes.Ldind_I1);
            else if (pointerType == typeof(short)) Emit(OpCodes.Ldind_I2);
            else if (pointerType == typeof(int)) Emit(OpCodes.Ldind_I4);
            else if (pointerType == typeof(long)) Emit(OpCodes.Ldind_I8);

            else if (pointerType == typeof(float)) Emit(OpCodes.Ldind_R4);
            else if (pointerType == typeof(double)) Emit(OpCodes.Ldind_R8);

            else throw (new NotImplementedException("Can't load indirect value"));
        }

        protected virtual void _Generate(AstNodeExprGetAddress getAddress)
        {
            var astNodeExprFieldAccess = (getAddress.Expression as AstNodeExprFieldAccess);
            var astNodeExprArgument = (getAddress.Expression as AstNodeExprArgument);

            if (astNodeExprFieldAccess != null)
            {
                Generate(astNodeExprFieldAccess.Instance);
                Emit(OpCodes.Ldflda, astNodeExprFieldAccess.Field);
            }
            else if (astNodeExprArgument != null)
            {
                Emit(OpCodes.Ldarga, astNodeExprArgument.AstArgument.Index);
            }
            else
            {
                throw (new NotImplementedException("Can't implement AstNodeExprGetAddress for '" +
                                                   getAddress.Expression.GetType() + "'"));
            }
        }

        protected virtual void _Generate(AstNodeExprSetGetLValuePlaceholder placeholder)
        {
            var astExpr = _placeholderStack.Pop();
            if (astExpr.Type != placeholder.Type)
                throw (new Exception("Invalid Expression for placeholder " + astExpr.Type + " != " + placeholder.Type +
                                     "."));
            Generate(astExpr);
        }

        protected virtual void _Generate(AstNodeExprSetGetLValue setGetLValue)
        {
            Generate(setGetLValue.GetExpression);
        }

        // @TODO: Rewrite using C# 7 features
        protected virtual void _Generate(AstNodeStmAssign assign)
        {
            //Assign.Local.LocalBuilder.LocalIndex
            var astNodeExprLocal = assign.LeftValue as AstNodeExprLocal;
            var astNodeExprArgument = assign.LeftValue as AstNodeExprArgument;
            var astNodeExprFieldAccess = assign.LeftValue as AstNodeExprFieldAccess;
            var astNodeExprStaticFieldAccess = assign.LeftValue as AstNodeExprStaticFieldAccess;
            var astNodeExprIndirect = assign.LeftValue as AstNodeExprIndirect;
            var astNodeExprArrayAccess = assign.LeftValue as AstNodeExprArrayAccess;
            var astNodeExprPropertyAccess = assign.LeftValue as AstNodeExprPropertyAccess;
            var astNodeExprSetGetLValue = assign.LeftValue as AstNodeExprSetGetLValue;


            if (astNodeExprLocal != null)
            {
                Generate(assign.Value);
                Emit(OpCodes.Stloc, _GetLocalBuilderFromAstLocal(astNodeExprLocal.AstLocal));
            }
            else if (astNodeExprArgument != null)
            {
                Generate(assign.Value);
                Emit(OpCodes.Starg, astNodeExprArgument.AstArgument.Index);
            }
            else if (astNodeExprFieldAccess != null)
            {
                Generate(astNodeExprFieldAccess.Instance);
                Generate(assign.Value);
                Emit(OpCodes.Stfld, astNodeExprFieldAccess.Field);
            }
            else if (astNodeExprStaticFieldAccess != null)
            {
                Generate(assign.Value);
                Emit(OpCodes.Stsfld, astNodeExprStaticFieldAccess.Field);
            }
            else if (astNodeExprArrayAccess != null)
            {
                Generate(astNodeExprArrayAccess.ArrayInstance);
                Generate(astNodeExprArrayAccess.Index);
                Generate(assign.Value);
                Emit(OpCodes.Stelem, astNodeExprArrayAccess.ArrayInstance.Type.GetElementType());
            }
            else if (astNodeExprIndirect != null)
            {
                var pointerType = AstUtils.GetSignedType(astNodeExprIndirect.PointerExpression.Type.GetElementType());

                Generate(astNodeExprIndirect.PointerExpression);
                Generate(assign.Value);

                if (pointerType == typeof(sbyte)) Emit(OpCodes.Stind_I1);
                else if (pointerType == typeof(short)) Emit(OpCodes.Stind_I2);
                else if (pointerType == typeof(int)) Emit(OpCodes.Stind_I4);
                else if (pointerType == typeof(long)) Emit(OpCodes.Stind_I8);
                else if (pointerType == typeof(float)) Emit(OpCodes.Stind_R4);
                else if (pointerType == typeof(double)) Emit(OpCodes.Stind_R8);
                else if (pointerType == typeof(bool)) Emit(OpCodes.Stind_I1);
                else throw (new NotImplementedException("Can't store indirect value"));
            }
            else if (astNodeExprPropertyAccess != null)
            {
                Generate(astNodeExprPropertyAccess.Instance);
                Generate(assign.Value);
                Emit(OpCodes.Callvirt, astNodeExprPropertyAccess.Property.SetMethod);
            }
            else if (astNodeExprSetGetLValue != null)
            {
                _placeholderStack.Push(assign.Value);
                Generate(astNodeExprSetGetLValue.SetExpression);
                if (astNodeExprSetGetLValue.SetExpression.Type != typeof(void))
                {
                    Emit(OpCodes.Pop);
                }
            }
            else
            {
                throw (new NotImplementedException("Not implemented AstNodeStmAssign LValue: " +
                                                   assign.LeftValue.GetType()));
            }
            //Assign.Local
        }

        protected virtual void _Generate(AstNodeStmReturn Return)
        {
            var expressionType = (Return.Expression != null) ? Return.Expression.Type : typeof(void);

            if (expressionType != MethodInfo.ReturnType)
            {
                throw (new Exception(String.Format("Return type mismatch {0} != {1}", expressionType,
                    MethodInfo.ReturnType)));
            }

            if (Return.Expression != null) Generate(Return.Expression);
            Emit(OpCodes.Ret);
        }

        protected virtual void _Generate(AstNodeExprCallTailCall call)
        {
            Generate(call.Call);
            Emit(OpCodes.Ret);
        }

        protected virtual void _Generate(AstNodeExprCallStatic call)
        {
            if (call.MethodInfo.CallingConvention.HasFlag(CallingConventions.HasThis))
            {
                throw (new Exception("CallString calling convention shouldn't have this '" + call.MethodInfo + "'"));
            }
            switch (call.MethodInfo.CallingConvention & CallingConventions.Any)
            {
                case CallingConventions.Standard:
                    foreach (var parameter in call.Parameters) Generate(parameter);
                    if (call.IsTail) Emit(OpCodes.Tailcall);
                    Emit(OpCodes.Call, call.MethodInfo);
                    break;
                default:
                    throw (new Exception(String.Format("Can't handle calling convention {0}",
                        call.MethodInfo.CallingConvention)));
            }
        }

        protected virtual void _Generate(AstNodeExprCallDelegate call)
        {
            _Generate((AstNodeExprCallInstance) call);
        }

        protected virtual void _Generate(AstNodeExprCallInstance call)
        {
            if (!call.MethodInfo.CallingConvention.HasFlag(CallingConventions.HasThis))
            {
                throw(new Exception("CallInstance calling convention should have this"));
            }
            switch (call.MethodInfo.CallingConvention & CallingConventions.Any)
            {
                case CallingConventions.Standard:
                    Generate(call.Instance);
                    foreach (var parameter in call.Parameters) Generate(parameter);
                    if (call.IsTail) Emit(OpCodes.Tailcall);
                    Emit(OpCodes.Callvirt, call.MethodInfo);
                    break;
                default:
                    throw (new Exception($"Can't handle calling convention {call.MethodInfo.CallingConvention}"));
            }
        }

        protected virtual void _GenerateCastToType(Type castedType)
        {
            if (false)
            {
            }
            else if (castedType == typeof(sbyte)) Emit(OpCodes.Conv_I1);
            else if (castedType == typeof(short)) Emit(OpCodes.Conv_I2);
            else if (castedType == typeof(int)) Emit(OpCodes.Conv_I4);
            else if (castedType == typeof(long)) Emit(OpCodes.Conv_I8);

            else if (castedType == typeof(byte)) Emit(OpCodes.Conv_U1);
            else if (castedType == typeof(ushort)) Emit(OpCodes.Conv_U2);
            else if (castedType == typeof(uint)) Emit(OpCodes.Conv_U4);
            else if (castedType == typeof(ulong)) Emit(OpCodes.Conv_U8);

            else if (castedType == typeof(float)) Emit(OpCodes.Conv_R4);
            else if (castedType == typeof(double)) Emit(OpCodes.Conv_R8);

            else if (castedType == typeof(bool)) Emit(OpCodes.Conv_I1);

            else if (castedType.IsPointer) Emit(OpCodes.Conv_I);
            else if (castedType.IsByRef) Emit(OpCodes.Conv_I);

            else if (castedType.IsPrimitive)
            {
                throw (new NotImplementedException("Not implemented cast other primitives"));
            }

            else if (castedType.IsEnum)
            {
                _GenerateCastToType(castedType.GetEnumUnderlyingType());
                //throw (new NotImplementedException("Not implemented cast other primitives"));
            }

            else
            {
                Emit(OpCodes.Castclass, castedType);
                //throw (new NotImplementedException("Not implemented cast class"));
            }
        }

        protected virtual void _Generate(AstNodeExprCast cast)
        {
            var castedType = cast.CastedType;

            Generate(cast.Expr);

            if (cast.Explicit)
            {
                _GenerateCastToType(castedType);
            }
        }

        protected virtual void _Generate(AstNodeExprTerop terop)
        {
            if (terop.True.Type != terop.False.Type)
                throw (new InvalidOperationException(String.Format("AstNodeExprTerop '?:' types must match {0} != {1}",
                    terop.True.Type, terop.False.Type)));
            var ternaryType = terop.True.Type;
            var ternaryTempAstLocal = AstLocal.Create(ternaryType);

            Generate(new AstNodeStmIfElse(
                terop.Cond,
                new AstNodeStmAssign(new AstNodeExprLocal(ternaryTempAstLocal), terop.True),
                new AstNodeStmAssign(new AstNodeExprLocal(ternaryTempAstLocal), terop.False)
            ));

            Generate(new AstNodeExprLocal(ternaryTempAstLocal));
        }

        protected virtual void _Generate(AstNodeStmIfElse ifElse)
        {
            var afterIfLabel = DefineLabel("AfterIf");

            Generate(ifElse.Condition);
            Emit(OpCodes.Brfalse, afterIfLabel);
            Generate(ifElse.True);

            if (ifElse.False != null)
            {
                var afterElseLabel = DefineLabel("AfterElse");
                Emit(OpCodes.Br, afterElseLabel);

                MarkLabel(afterIfLabel);

                Generate(ifElse.False);

                MarkLabel(afterElseLabel);
            }
            else
            {
                MarkLabel(afterIfLabel);
            }
        }

        protected virtual void _Generate(AstNodeExprBinop item)
        {
            var leftType = item.LeftNode.Type;
            //var rightType = item.RightNode.Type;

            //if (LeftType != RightType) throw(new Exception(String.Format("BinaryOp Type mismatch ({0}) != ({1})", LeftType, RightType)));

            //Item.GetType().GenericTypeArguments[0]
            Generate(item.LeftNode);
            Generate(item.RightNode);

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

            switch (item.Operator)
            {
                case "+":
                    Emit(OpCodes.Add);
                    break;
                case "-":
                    Emit(OpCodes.Sub);
                    break;
                case "*":
                    Emit(OpCodes.Mul);
                    break;
                case "/":
                    Emit(AstUtils.IsTypeSigned(leftType) ? OpCodes.Div : OpCodes.Div_Un);
                    break;
                case "%":
                    Emit(AstUtils.IsTypeSigned(leftType) ? OpCodes.Rem : OpCodes.Rem_Un);
                    break;
                case "==":
                    Emit(OpCodes.Ceq);
                    break;
                case "!=":
                    Emit(OpCodes.Ceq);
                    Emit(OpCodes.Ldc_I4_0);
                    Emit(OpCodes.Ceq);
                    break;
                case "<":
                    Emit(AstUtils.IsTypeSigned(leftType) ? OpCodes.Clt : OpCodes.Clt_Un);
                    break;
                case ">":
                    Emit(AstUtils.IsTypeSigned(leftType) ? OpCodes.Cgt : OpCodes.Cgt_Un);
                    break;
                case "<=":
                    Emit(AstUtils.IsTypeSigned(leftType) ? OpCodes.Cgt : OpCodes.Cgt_Un);
                    Emit(OpCodes.Ldc_I4_0);
                    Emit(OpCodes.Ceq);
                    break;
                case ">=":
                    Emit(AstUtils.IsTypeSigned(leftType) ? OpCodes.Clt : OpCodes.Clt_Un);
                    Emit(OpCodes.Ldc_I4_0);
                    Emit(OpCodes.Ceq);
                    break;
                case "&":
                case "&&":
                    Emit(OpCodes.And);
                    break;
                case "|":
                case "||":
                    Emit(OpCodes.Or);
                    break;
                case "^":
                    Emit(OpCodes.Xor);
                    break;
                case "<<":
                    Emit(OpCodes.Shl);
                    break;
                case ">>":
                    Emit(AstUtils.IsTypeSigned(leftType) ? OpCodes.Shr : OpCodes.Shr_Un);
                    break;
                default: throw(new NotImplementedException($"Not implemented operator '{item.Operator}'"));
            }
        }

        protected virtual void _Generate(AstNodeStmExpr stat)
        {
            var expressionType = stat.AstNodeExpr.Type;
            Generate(stat.AstNodeExpr);

            if (expressionType != typeof(void))
            {
                Emit(OpCodes.Pop);
            }
        }

        protected virtual void _Generate(AstNodeStmEmpty empty)
        {
        }

        protected virtual void _Generate(AstNodeStmLabel label)
        {
            MarkLabel(label.AstLabel);
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

        protected virtual void _Generate(AstNodeExprUnop item)
        {
            //var rightType = item.RightNode.Type;

            Generate(item.RightNode);

            switch (item.Operator)
            {
                case "~":
                    Emit(OpCodes.Not);
                    break;
                case "+": break;
                case "-":
                    Emit(OpCodes.Neg);
                    break;
                case "!":
                    Emit(OpCodes.Ldc_I4_0);
                    Emit(OpCodes.Ceq);
                    break;
                default: throw(new NotImplementedException($"Not implemented operator '{item.Operator}'"));
            }
        }

        protected virtual void _Generate(AstNodeStmSwitch Switch)
        {
            var allCaseValues = Switch.Cases.Select(Case => Case.CaseValue);
            var caseValues = allCaseValues as IList<object> ?? allCaseValues.ToList();
            if (caseValues.Count != caseValues.Distinct().Count())
            {
                throw(new Exception("Repeated case in switch!"));
            }

            // Check types and unique values.

            var endCasesLabel = AstLabel.CreateLabel("EndCasesLabel");
            var defaultLabel = AstLabel.CreateLabel("DefaultLabel");

            if (Switch.Cases.Length > 0)
            {
                var commonType = Switch.Cases.First().CaseValue.GetType();
                if (Switch.Cases.Any(Case => Case.CaseValue.GetType() != commonType))
                {
                    throw(new Exception("All cases should have the same type"));
                }

                var doneSpecialized = false;

                // Specialized constant-time integer switch (if possible)
                if (AstUtils.IsIntegerType(commonType))
                {
                    var commonMin = Switch.Cases.Min(Case => AstUtils.CastType<long>(Case.CaseValue));
                    var commonMax = Switch.Cases.Max(Case => AstUtils.CastType<long>(Case.CaseValue));
                    var casesLength = (commonMax - commonMin) + 1;

                    // No processing tables greater than 4096 elements.
                    if (casesLength <= 4096)
                    {
                        var labels = new AstLabel[casesLength];
                        for (var n = 0; n < casesLength; n++) labels[n] = defaultLabel;

                        foreach (var Case in Switch.Cases)
                        {
                            var realValue = AstUtils.CastType<long>(Case.CaseValue);
                            var offset = realValue - commonMin;
                            labels[offset] = AstLabel.CreateLabel("Case_" + realValue);
                        }

                        /*
                        //var SwitchVarLocal = AstLocal.Create(AllCaseValues.First().GetType(), "SwitchVarLocal" + SwitchVarCount++);
                        //Generate(new AstNodeStmAssign(new AstNodeExprLocal(SwitchVarLocal), Switch.SwitchValue - new AstNodeExprCast(CommonType, CommonMin)));
                        //Generate(new AstNodeStmIfElse(new AstNodeExprBinop(new AstNodeExprLocal(SwitchVarLocal), "<", 0), new AstNodeStmGotoAlways(DefaultLabel)));
                        //Generate(new AstNodeStmIfElse(new AstNodeExprBinop(new AstNodeExprLocal(SwitchVarLocal), ">=", CasesLength), new AstNodeStmGotoAlways(DefaultLabel)));
                        //Generate(new AstNodeExprLocal(SwitchVarLocal));
                        */

                        Generate(Switch.SwitchValue - new AstNodeExprCast(commonType, commonMin));
                        Emit(OpCodes.Switch, labels);
                        Generate(new AstNodeStmGotoAlways(defaultLabel));
                        foreach (var Case in Switch.Cases)
                        {
                            var realValue = AstUtils.CastType<long>(Case.CaseValue);
                            var offset = realValue - commonMin;
                            Generate(new AstNodeStmLabel(labels[offset]));
                            {
                                Generate(Case.Code);
                            }
                            Generate(new AstNodeStmGotoAlways(endCasesLabel));
                        }

                        doneSpecialized = true;
                    }
                    else
                    {
                        // TODO: find a common shift and masks for all the values to reduce CasesLength.
                        // TODO: On too large test cases, split them recursively in:
                        // if (Var < Half) { switch(Var - LowerPartMin) { ... } } else { switch(Var - Half - UpperPartMin) { ... } }
                    }
                }
                // Specialized switch for strings (checking length, then hash, then contents)
                else if (commonType == typeof(string))
                {
                    // TODO!
                }

                // Generic if/else
                if (!doneSpecialized)
                {
                    var switchVarLocal =
                        AstLocal.Create(caseValues.First().GetType(), "SwitchVarLocal" + _switchVarCount++);
                    Generate(new AstNodeStmAssign(new AstNodeExprLocal(switchVarLocal), Switch.SwitchValue));
                    //Switch.Cases
                    foreach (var Case in Switch.Cases)
                    {
                        var labelSkipThisCase = AstLabel.CreateLabel("LabelCase" + Case.CaseValue);
                        Generate(new AstNodeStmGotoIfFalse(labelSkipThisCase,
                            new AstNodeExprBinop(new AstNodeExprLocal(switchVarLocal), "==",
                                new AstNodeExprImm(Case.CaseValue))));
                        Generate(Case.Code);
                        Generate(new AstNodeStmGotoAlways(endCasesLabel));
                        Generate(new AstNodeStmLabel(labelSkipThisCase));
                    }
                }
            }

            Generate(new AstNodeStmLabel(defaultLabel));
            if (Switch.CaseDefault != null)
            {
                Generate(Switch.CaseDefault.Code);
            }

            Generate(new AstNodeStmLabel(endCasesLabel));
        }

        protected virtual void _Generate(AstNodeExprNewArray newArray)
        {
            var tempArrayLocal = AstLocal.Create(newArray.Type, "$TempArray");
            Generate(new AstNodeExprImm(newArray.Length));
            Emit(OpCodes.Newarr, newArray.ElementType);
            Emit(OpCodes.Stloc, _GetLocalBuilderFromAstLocal(tempArrayLocal));
            for (var n = 0; n < newArray.Length; n++)
            {
                Generate(new AstNodeStmAssign(new AstNodeExprArrayAccess(new AstNodeExprLocal(tempArrayLocal), n),
                    newArray.Values[n]));
            }
            Generate(new AstNodeExprLocal(tempArrayLocal));
        }

        protected virtual void _Generate(AstNodeExprNew astNodeExprNew)
        {
            var constructor =
                astNodeExprNew.Type.GetConstructor(astNodeExprNew.Params.Select(param => param.Type).ToArray());
            foreach (var param in astNodeExprNew.Params)
            {
                Generate(param);
            }
            Emit(OpCodes.Newobj, constructor);
        }

        protected virtual void _Generate(AstNodeStmThrow astNodeStmThrow)
        {
            Generate(astNodeStmThrow.AstNodeExpr);
            Emit(OpCodes.Throw);
        }
    }
}