using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Codegen
{
	public class SafeMethodTypeInfo
	{
		public Type ReturnType;
		public Type[] Parameters;
		public bool IsStatic;

		public SafeMethodTypeInfo()
		{
		}

		public SafeMethodTypeInfo(MethodInfo MethodInfo)
		{
			this.IsStatic = MethodInfo.IsStatic;
			this.ReturnType = MethodInfo.ReturnType;
			this.Parameters = MethodInfo.GetParameters().Select(Item => Item.ParameterType).ToArray();
		}
	}

	public partial class SafeILGenerator
	{
		public void UnaryOperation(SafeUnaryOperator Operator)
		{
			if (TrackStack)
			{
				var TypeRight = TypeStack.Pop();

				if (CheckTypes)
				{
				}

				TypeStack.Push(TypeRight);
			}

			if (DoEmit)
			{
				switch (Operator)
				{
					case SafeUnaryOperator.Negate: Emit(OpCodes.Neg); break;
					case SafeUnaryOperator.Not: Emit(OpCodes.Not); break;
					default: throw (new NotImplementedException());
				}
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("UnaryOperation({0}) :: Stack -> {1}", Operator, TypeStack.Count));
			}
		}

		public void StoreElement(Type Type)
		{
			if (TrackStack)
			{
				var StoreValueType = TypeStack.Pop();
				var StoreIndexValue = TypeStack.Pop();
				var StoreArrayValue = TypeStack.Pop();
			}

			if (DoEmit)
			{
				while (true)
				{
					if (Type == typeof(bool)) { Emit(OpCodes.Stelem_I); break; }
					if (Type == typeof(sbyte) || Type == typeof(byte)) { Emit(OpCodes.Stelem_I1); break; }
					if (Type == typeof(short) || Type == typeof(ushort)) { Emit(OpCodes.Stelem_I2); break; }
					if (Type == typeof(int) || Type == typeof(uint)) { Emit(OpCodes.Stelem_I4); break; }
					if (Type == typeof(float)) { Emit(OpCodes.Stelem_R4); break; }
					if (Type == typeof(double)) { Emit(OpCodes.Stelem_R8); break; }
					throw (new NotImplementedException());
				}
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("StoreElement({0}) :: Stack -> {1}", Type.Name, TypeStack.Count));
			}
		}

		public void StoreElement<TType>()
		{
			StoreElement(typeof(TType));
		}

		public void InitObject(Type Type)
		{
			if (TrackStack)
			{
				var StoreAddressValue = TypeStack.Pop();
				//TypeStack.Push(Type);
			}

			if (DoEmit)
			{
				Emit(OpCodes.Initobj, Type);
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("InitObject({0}) :: Stack -> {1}", Type.Name, TypeStack.Count));
			}
		}

		public void StackAlloc()
		{
			if (TrackStack)
			{
				var Count = TypeStack.Pop();
				TypeStack.Push(typeof(void*));
			}

			if (DoEmit)
			{
				Emit(OpCodes.Localloc);
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("StackAlloc :: Stack -> {0}", TypeStack.Count));
			}
		}

		public void StoreObject(Type Type)
		{
			if (TrackStack)
			{
				var StoreValueType = TypeStack.Pop();
				var StoreAddressValue = TypeStack.Pop();
				if (StoreValueType != Type) throw (new InvalidOperationException(String.Format("{0} != {1}", StoreValueType, Type)));
			}

			if (DoEmit)
			{
				Emit(OpCodes.Stobj, Type);
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("SotreObject({0}) :: Stack -> {1}", Type.Name, TypeStack.Count));
			}
		}

		public void StoreIndirect(Type Type)
		{
			if (TrackStack)
			{
				var StoreValueType = TypeStack.Pop();
				var StoreAddressValue = TypeStack.Pop();
				if (Type != StoreValueType)
				{
					if (Type.IsPointer && StoreValueType.IsPointer)
					{
					}
					else
					{
						//throw (new InvalidOperationException(String.Format("{0} != {1}", StoreValueType, Type)));
					}
				}
			}

			if (DoEmit)
			{
				while (true)
				{
					// Simple types.
					if (Type == typeof(bool)) { Emit(OpCodes.Stind_I); break; }
					if (Type == typeof(sbyte) || Type == typeof(byte)) { Emit(OpCodes.Stind_I1); break; }
					if (Type == typeof(short) || Type == typeof(ushort)) { Emit(OpCodes.Stind_I2); break; }
					if (Type == typeof(int) || Type == typeof(uint)) { Emit(OpCodes.Stind_I4); break; }
					if (Type == typeof(long) || Type == typeof(ulong)) { Emit(OpCodes.Stind_I8); break; }
					if (Type == typeof(float)) { Emit(OpCodes.Stind_R4); break; }
					if (Type == typeof(double)) { Emit(OpCodes.Stind_R8); break; }

					// Pointers
					//if (Type.IsPointer) { Emit(OpCodes.Stobj, Type); break; }
					if (Type.IsPointer) { Emit(OpCodes.Stind_I); break; }
					//if (Type.IsPointer) { Emit(OpCodes.Stind_Ref); break; }

					//if (Type.IsClass) { Emit(OpCodes.Stobj, Type); break; }
					if (true) { Emit(OpCodes.Stobj, Type); break; }

					throw (new NotImplementedException("Can't store indirectly type '" + Type.Name + "'"));
				}
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("StoreIndirect({0}) :: Stack -> {1}", Type.Name, TypeStack.Count));
			}
		}

		public void StoreIndirect<TType>()
		{
			StoreIndirect(typeof(TType));
		}

		public void StoreLocal(LocalBuilder Local)
		{
			if (TrackStack)
			{
				var StoreValueType = TypeStack.Pop();
				if (StoreValueType != Local.LocalType)
				{
#if false
					throw (new InvalidOperationException(String.Format("{0} != {1}", StoreValueType, Local.LocalType)));
#endif
				}
			}

			if (DoEmit)
			{
				int LocalIndex = Local.LocalIndex;
				switch (LocalIndex)
				{
					case 0: Emit(OpCodes.Stloc_0); break;
					case 1: Emit(OpCodes.Stloc_1); break;
					case 2: Emit(OpCodes.Stloc_2); break;
					case 3: Emit(OpCodes.Stloc_3); break;
					default: Emit(((int)(byte)LocalIndex == (int)LocalIndex) ? OpCodes.Stloc_S : OpCodes.Stloc, Local); break;
				}
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("StoreLocal({0}) :: Stack -> {1}", Local.LocalType, TypeStack.Count));
			}
		}

		public void StoreArgument(SafeArgument Argument)
		{
			StoreArgument(Argument.Type, Argument.Index);
		}

		public void StoreArgument<TType>(ushort ArgumentIndex)
		{
			StoreArgument(typeof(TType), ArgumentIndex);
		}

		public void StoreArgument(Type Type, ushort ArgumentIndex)
		{
			if (TrackStack)
			{
				var ArgumentIndexType = TypeStack.Pop();
				var ArgumentValueType = TypeStack.Pop();
			}

			if (DoEmit)
			{
				if ((ushort)(byte)ArgumentIndex == (ushort)ArgumentIndex)
				{
					Emit(OpCodes.Starg_S, (byte)ArgumentIndex);
				}
				else
				{
					Emit(OpCodes.Starg, (ushort)ArgumentIndex);
				}
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("StoreArgument<{0}>({1}) :: Stack -> {2}", Type.Name, ArgumentIndex, TypeStack.Count));
			}
		}

		public void Box<TType>()
		{
			Box(typeof(TType));
		}

		public void Box(Type Type)
		{
			if (TrackStack)
			{
				var TypeType = TypeStack.Pop();
				if (TypeType != Type) throw(new InvalidCastException());
				TypeStack.Push(typeof(object));
			}

			if (DoEmit)
			{
				Emit(OpCodes.Box, Type);
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("Box({0}) :: Stack -> {1}", Type, TypeStack.Count));
			}
		}

		public void Unbox(Type Type)
		{
			if (TrackStack)
			{
				var ObjectType = TypeStack.Pop();
				TypeStack.Push(Type);
			}

			if (DoEmit)
			{
				Emit(OpCodes.Unbox);
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("Unbox({0}) :: Stack -> {1}", Type, TypeStack.Count));
			}
		}

		public void Unbox<TType>()
		{
			Unbox(typeof(TType));
		}

		public void SetPointerAttributes(SafePointerAttributes Attributes)
		{
			if (TrackStack)
			{
				var AddressType = TypeStack.Pop();
				TypeStack.Push(AddressType); // Unaligned
			}

			if (DoEmit)
			{
				if ((Attributes & SafePointerAttributes.Unaligned) != 0)
				{
					Emit(OpCodes.Unaligned);
				}

				if ((Attributes & SafePointerAttributes.Volatile) != 0)
				{
					Emit(OpCodes.Volatile);
				}
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("SetPointerAttributes({0}) :: Stack -> {1}", Attributes, TypeStack.Count));
			}
		}

		/// <summary>
		/// Must precede a call type instruction.
		/// </summary>
		public void Tailcall()
		{
			//throw (new NotImplementedException());
			Emit(OpCodes.Tailcall);
		}

		public void Switch<TType>(IEnumerable<TType> ListEnumerable, Func<TType, int> IntKeySelector, Action<TType> CaseGenerate, Action DefaultGenerate)
		{
			var SwitchEndLabel = DefineLabel("SwitchEndLabel");
			var SwitchDefaultLabel = DefineLabel("SwitchDefaultLabel");
			var Labels = new List<SafeLabel>();
			var Dictionary = new Dictionary<int, SafeLabel>();
			var List = ListEnumerable.ToArray();

			// Generate dictionary and labels
			foreach (var Item in List)
			{
				var IntKey = IntKeySelector(Item);
				var Label = DefineLabel("SwitchCase" + Item);
				Labels.Add(Label);
				Dictionary.Add(IntKey, Label);
			}

			// Switch
			// {
			Switch(Dictionary, SwitchDefaultLabel);

			for (int n = 0; n < List.Length; n++)
			{
				// case Item:
				var Label = Labels[n];
				var Item = List[n];
				Label.Mark();
				{
					CaseGenerate(Item);
				}
				BranchAlways(SwitchEndLabel); // break;
			}

			// Default:
			SwitchDefaultLabel.Mark();
			{
				if (DefaultGenerate != null) DefaultGenerate();
			}
			BranchAlways(SwitchEndLabel); // break;

			// }
			SwitchEndLabel.Mark();
		}

		public void Switch(Dictionary<int, SafeLabel> Labels, SafeLabel DefaultLabel)
		{
#if true
			// SwitchReferenceValue = <expression>
			var SwitchReferenceValue = DeclareLocal<int>("SwitchReferenceValue");
			StoreLocal(SwitchReferenceValue);

			foreach (var Pair in Labels)
			{
				LoadLocal(SwitchReferenceValue);
				Push(Pair.Key);
				BranchBinaryComparison(SafeBinaryComparison.Equals, Pair.Value);
			}
			BranchAlways(DefaultLabel);
#else
			var MinKey = Labels.Keys.Min();
			var MaxKey = Labels.Keys.Max();
			var Length = MaxKey - MinKey;

			BranchAlways(DefaultLabel);
			throw (new NotImplementedException());
#endif
		}

		public void Switch(SafeLabel[] Labels)
		{
			if (TrackStack)
			{
				var IndexType = TypeStack.Pop();
			}

			if (DoEmit)
			{
				Emit(OpCodes.Switch, Labels);
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("Switch({0}) :: Stack -> {1}", Labels, TypeStack.Count));
			}
		}

		public void Sizeof(Type Type)
		{
			if (TrackStack)
			{
				//var BaseType = TypeStack.Pop();
				TypeStack.Push(typeof(int));
			}

			if (DoEmit)
			{
				Emit(OpCodes.Sizeof, Type);
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("Sizeof({0}) :: Stack -> {1}", Type, TypeStack.Count));
			}
		}

		public void NewObject(ConstructorInfo ConstructorInfo)
		{
			if (TrackStack)
			{
				//var ConstructorInfo = TypeStack.Pop();
				foreach (var Parameter in ConstructorInfo.GetParameters())
				{
					TypeStack.Pop();
				}
			}

			if (DoEmit)
			{
				__ILGenerator.Emit(OpCodes.Newobj, ConstructorInfo);
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("NewObject() :: Stack -> {0}", TypeStack.Count));
			}
		}

		public void Throw()
		{
			if (TrackStack)
			{
				var ExceptionType = TypeStack.Pop();
				if (typeof(Exception).IsAssignableFrom(ExceptionType))
				{
					throw(new InvalidOperationException(String.Format("Must throw an exception type but trying to throw {0}", ExceptionType)));
				}
			}

			if (DoEmit)
			{
				Emit(OpCodes.Throw);
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("Throw() :: Stack -> {0}", TypeStack.Count));
			}
		}

		public void Return(Type ReturnType)
		{
			if (TrackStack)
			{
				if (ReturnType != typeof(void))
				{
					var StackReturnType = TypeStack.Pop();
					if (StackReturnType != ReturnType)
					{
						//throw (new Exception(String.Format("Invalid return type {0} != {1}", StackReturnType, ReturnType)));
						ConvertTo(ReturnType);
					}
				}
			}

			if (DoEmit)
			{
				Emit(OpCodes.Ret);
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("Return() :: Stack -> {0}", TypeStack.Count));
			}
		}

		public void NoOperation()
		{
			if (DoEmit)
			{
				Emit(OpCodes.Nop);
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("NoOperation() :: Stack -> {0}", TypeStack.Count));
			}
		}

		public void BinaryOperation(SafeBinaryOperator Operator)
		{
			if (TrackStack)
			{
				var TypeRight = TypeStack.Pop();
				var TypeLeft = TypeStack.Pop();

				if (CheckTypes)
				{
					if (TypeLeft != TypeRight)
					{
						throw (new InvalidOperationException(String.Format(
							"Binary operation mismatch Left:{0} != Right:{1}",
							TypeLeft.Name,
							TypeRight.Name
						)));
					}
				}

				TypeStack.Push(TypeRight);
			}

			if (DoEmit)
			{
				switch (Operator)
				{
					case SafeBinaryOperator.AdditionSigned: Emit(OverflowCheck ? OpCodes.Add_Ovf : OpCodes.Add); break;
					case SafeBinaryOperator.AdditionUnsigned: Emit(OverflowCheck ? OpCodes.Add_Ovf_Un : OpCodes.Add); break;
					case SafeBinaryOperator.SubstractionSigned: Emit(OverflowCheck ? OpCodes.Sub_Ovf : OpCodes.Sub); break;
					case SafeBinaryOperator.SubstractionUnsigned: Emit(OverflowCheck ? OpCodes.Sub_Ovf_Un : OpCodes.Sub); break;
					case SafeBinaryOperator.DivideSigned: Emit(OpCodes.Div); break;
					case SafeBinaryOperator.DivideUnsigned: Emit(OpCodes.Div_Un); break;
					case SafeBinaryOperator.RemainingSigned: Emit(OpCodes.Rem); break;
					case SafeBinaryOperator.RemainingUnsigned: Emit(OpCodes.Rem_Un); break;
					case SafeBinaryOperator.MultiplySigned: Emit(OverflowCheck ? OpCodes.Mul_Ovf : OpCodes.Mul); break;
					case SafeBinaryOperator.MultiplyUnsigned: Emit(OverflowCheck ? OpCodes.Mul_Ovf_Un : OpCodes.Mul); break;
					case SafeBinaryOperator.And: Emit(OpCodes.And); break;
					case SafeBinaryOperator.Or: Emit(OpCodes.Or); break;
					case SafeBinaryOperator.Xor: Emit(OpCodes.Xor); break;
					case SafeBinaryOperator.ShiftLeft: Emit(OpCodes.Shl); break;
					case SafeBinaryOperator.ShiftRightSigned: Emit(OpCodes.Shr); break;
					case SafeBinaryOperator.ShiftRightUnsigned: Emit(OpCodes.Shr_Un); break;
					default: throw (new NotImplementedException());
				}
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("BinaryOperation({0}) :: Stack -> {1}", Operator, TypeStack.Count));
			}
		}

		public void Duplicate()
		{
			if (TrackStack)
			{
				var TypeToDuplicate = TypeStack.Pop();
				TypeStack.Push(TypeToDuplicate);
				TypeStack.Push(TypeToDuplicate);
			}

			if (DoEmit)
			{
				Emit(OpCodes.Dup);
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("Duplicate() :: Stack -> {0}", TypeStack.Count));
			}
		}

		public void CompareBinary(SafeBinaryComparison Comparison)
		{
			if (TrackStack)
			{
				var TypeRight = TypeStack.Pop();
				var TypeLeft = TypeStack.Pop();

				if (CheckTypes)
				{
					if (TypeLeft != TypeRight) throw (new InvalidOperationException("Binary operation mismatch"));
				}

				TypeStack.Push(typeof(bool));
			}

			if (DoEmit)
			{
				switch (Comparison)
				{
					case SafeBinaryComparison.Equals: Emit(OpCodes.Ceq); break;
					case SafeBinaryComparison.NotEquals: Emit(OpCodes.Ceq); Emit(OpCodes.Ldc_I4_0); Emit(OpCodes.Ceq); break;
					case SafeBinaryComparison.GreaterThanSigned: Emit(OpCodes.Cgt); break;
					case SafeBinaryComparison.GreaterThanUnsigned: Emit(OpCodes.Cgt_Un); break;
					case SafeBinaryComparison.GreaterOrEqualSigned: Emit(OpCodes.Clt); Emit(OpCodes.Ldc_I4_0); Emit(OpCodes.Ceq); break;
					case SafeBinaryComparison.GreaterOrEqualUnsigned: Emit(OpCodes.Clt_Un); Emit(OpCodes.Ldc_I4_0); Emit(OpCodes.Ceq); break;
					case SafeBinaryComparison.LessThanSigned: Emit(OpCodes.Clt); break;
					case SafeBinaryComparison.LessThanUnsigned: Emit(OpCodes.Clt_Un); break;
					case SafeBinaryComparison.LessOrEqualSigned: Emit(OpCodes.Cgt); Emit(OpCodes.Ldc_I4_0); Emit(OpCodes.Ceq); break;
					case SafeBinaryComparison.LessOrEqualUnsigned: Emit(OpCodes.Cgt_Un); Emit(OpCodes.Ldc_I4_0); Emit(OpCodes.Ceq); break;
					default: throw (new NotImplementedException());
				}
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("CompareBinary({0}) :: Stack -> {1}", Comparison, TypeStack.Count));
			}
		}

		public void ConvertTo<TType>()
		{
			ConvertTo(typeof(TType));
		}

		public void ConvertTo(Type Type)
		{
			Type PreviousType = null;

			if (TrackStack)
			{
				PreviousType = TypeStack.Pop();
				TypeStack.Push(Type);
			}

			if (DoEmit && (PreviousType != Type))
			{
				while (true)
				{
					if (PreviousType != null && PreviousType.IsPointer)
					{
						Emit(OpCodes.Conv_U);
					}

					/*
					if (PreviousType != null && PreviousType.IsPointer)
					{
						//typeof(IntPtr).GetMethods(
						//typeof(IntPtr).GetMethod(null, null, null, 

						var PointerToIntPtr = typeof(IntPtr).GetMethods().Where(Method => Method.Name == "op_Explicit" && Method.ReturnType == typeof(IntPtr) && Method.GetParameters()[0].ParameterType == typeof(void*)).First();
						var IntPtrToInt32 = typeof(IntPtr).GetMethods().Where(Method => Method.Name == "op_Explicit" && Method.ReturnType == typeof(int) && Method.GetParameters()[0].ParameterType == typeof(IntPtr)).First();
						Call(PointerToIntPtr);
						Call(IntPtrToInt32);
						// IL_0000: ldc.i4.0
						// IL_0001: conv.u
						// IL_0002: ldflda int32 ilcc.Runtime.Tests.CLibTest/MyStruct::c
						// IL_0007: conv.u
						// IL_0008: call native int [mscorlib]System.IntPtr::op_Explicit(void*)
						// IL_000d: call int32 [mscorlib]System.IntPtr::op_Explicit(native int)
						// IL_0012: ret
					}
					*/

					if (Type == typeof(bool))
					{
						Push(0);
						CompareBinary(SafeBinaryComparison.NotEquals);

						//Emit(OverflowCheck ? OpCodes.Conv_Ovf_I : OpCodes.Conv_I);
						break;
					}
					if (Type == typeof(sbyte)) { Emit(OverflowCheck ? OpCodes.Conv_Ovf_I1 : OpCodes.Conv_I1); break; }
					if (Type == typeof(byte)) { Emit(OverflowCheck ? OpCodes.Conv_Ovf_U1 : OpCodes.Conv_U1); break; }
					if (Type == typeof(short)) { Emit(OverflowCheck ? OpCodes.Conv_Ovf_I2 : OpCodes.Conv_I2); break; }
					if (Type == typeof(ushort)) { Emit(OverflowCheck ? OpCodes.Conv_Ovf_U2 : OpCodes.Conv_U2); break; }
					if (Type == typeof(int)) { Emit(OverflowCheck ? OpCodes.Conv_Ovf_I4 : OpCodes.Conv_I4); break; }
					if (Type == typeof(uint)) { Emit(OverflowCheck ? OpCodes.Conv_Ovf_U4 : OpCodes.Conv_U4); break; }
					if (Type == typeof(long)) { Emit(OverflowCheck ? OpCodes.Conv_Ovf_I8 : OpCodes.Conv_I8); break; }
					if (Type == typeof(ulong)) { Emit(OverflowCheck ? OpCodes.Conv_Ovf_U8 : OpCodes.Conv_U8); break; }
					if (Type == typeof(float)) { Emit(OpCodes.Conv_R4); break; }
					if (Type == typeof(double)) { Emit(OpCodes.Conv_R8); break; }

					if (Type.IsPointer)
					{
						Emit(OverflowCheck ? OpCodes.Conv_Ovf_U : OpCodes.Conv_U);
						// Do nothing?
						break;
					}

					//if (Type.IsClass)
					{
						Emit(OpCodes.Castclass, Type);
						break;
					}

					throw (new NotImplementedException(String.Format("Can't convert to type '{0}'", Type)));
				}
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("ConvertTo({0}) :: Stack -> {1}", Type, TypeStack.Count));
			}
		}

		public void CopyBlock()
		{
			if (TrackStack)
			{
				var CountType = TypeStack.Pop();
				var DestinationAddressType = TypeStack.Pop();
				var SourceAddressType = TypeStack.Pop();
			}

			if (DoEmit)
			{
				Emit(OpCodes.Cpblk);
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("CopyBlock() :: Stack -> {0}", TypeStack.Count));
			}

			throw (new NotImplementedException());
		}

		public void CopyObject()
		{
			if (DoEmit)
			{
				Emit(OpCodes.Cpobj);
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("CopyObject() :: Stack -> {0}", TypeStack.Count));
			}

			throw (new NotImplementedException());
		}

		public void Constrained()
		{
			if (DoEmit)
			{
				Emit(OpCodes.Constrained);
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("Constrained() :: Stack -> {0}", TypeStack.Count));
			}
		}

		public void CheckFinite()
		{
			if (TrackStack)
			{
				var Type = TypeStack.Pop();
			}

			if (DoEmit)
			{
				Emit(OpCodes.Ckfinite);
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("CheckFinite() :: Stack -> {0}", TypeStack.Count));
			}
		}

		public void BranchBinaryComparison(SafeBinaryComparison Comparison, SafeLabel Label)
		{
			if (TrackStack)
			{
				var TypeRight = TypeStack.Pop();
				var TypeLeft = TypeStack.Pop();

				if (CheckTypes)
				{
					if (TypeLeft != TypeRight) throw (new InvalidOperationException("Binary operation mismatch"));
				}
			}

			if (DoEmit)
			{
				switch (Comparison)
				{
					case SafeBinaryComparison.Equals: Emit(OpCodes.Beq, Label); break;
					case SafeBinaryComparison.NotEquals: Emit(OpCodes.Bne_Un, Label); break;

					case SafeBinaryComparison.GreaterOrEqualSigned: Emit(OpCodes.Bge, Label); break;
					case SafeBinaryComparison.GreaterOrEqualUnsigned: Emit(OpCodes.Bge_Un, Label); break;
					case SafeBinaryComparison.GreaterThanSigned: Emit(OpCodes.Bgt, Label); break;
					case SafeBinaryComparison.GreaterThanUnsigned: Emit(OpCodes.Bgt_Un, Label); break;

					case SafeBinaryComparison.LessOrEqualSigned: Emit(OpCodes.Ble, Label); break;
					case SafeBinaryComparison.LessOrEqualUnsigned: Emit(OpCodes.Ble_Un, Label); break;
					case SafeBinaryComparison.LessThanSigned: Emit(OpCodes.Blt, Label); break;
					case SafeBinaryComparison.LessThanUnsigned: Emit(OpCodes.Blt_Un, Label); break;

					default: throw (new NotImplementedException());
				}
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("BranchBinaryComparison({0}, {1}) :: Stack -> {2}", Comparison, Label, TypeStack.Count));
			}
		}

		public void BranchUnaryComparison(SafeUnaryComparison Comparison, SafeLabel Label, bool Short = false)
		{
			if (TrackStack)
			{
				var Type = TypeStack.Pop();

				if (CheckTypes)
				{
					if (Type != typeof(bool))
					{
						throw (new InvalidOperationException("Required boolean value"));
					}
				}
			}

			if (DoEmit)
			{
				switch (Comparison)
				{
					case SafeUnaryComparison.False: Emit(Short ? OpCodes.Brfalse_S : OpCodes.Brfalse, Label); break;
					case SafeUnaryComparison.True: Emit(Short ? OpCodes.Brtrue_S : OpCodes.Brtrue, Label); break;
					default: throw (new NotImplementedException());
				}
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("_BranchUnaryComparison({0}, {1}) :: Stack -> {2}", Comparison, Label, TypeStack.Count));
			}
		}

		public void BranchIfTrue(SafeLabel Label, bool Short = false)
		{
			BranchUnaryComparison(SafeUnaryComparison.True, Label, Short);
		}

		public void BranchIfFalse(SafeLabel Label, bool Short = false)
		{
			BranchUnaryComparison(SafeUnaryComparison.False, Label, Short);
		}

		static private Type GetCommonType(Type Type)
		{
			if (Type == typeof(byte)) return typeof(sbyte);
			if (Type == typeof(ushort)) return typeof(short);
			if (Type == typeof(uint)) return typeof(int);
			if (Type == typeof(ulong)) return typeof(long);
			if (Type.IsPointer) return typeof(void*);
			if (Type.IsEnum)  return typeof(int);
			return Type;
		}

		private void _Jmp_Call(OpCode OpCode, MethodInfo MethodInfo, SafeMethodTypeInfo SafeMethodTypeInfo, Type[] ParameterTypes)
		{
			if (SafeMethodTypeInfo == null)
			{
				SafeMethodTypeInfo = new SafeMethodTypeInfo(MethodInfo);
			}
			if (DoDebug)
			{
				Debug.WriteLine(String.Format("_Jmp_Call({0}, {1}) :: Stack -> {2}", OpCode.Name, MethodInfo, TypeStack.Count));
			}

			if (TrackStack)
			{
				int CurrentArgumentIndex = 0;
				foreach (var ParameterParameterType in SafeMethodTypeInfo.Parameters.Reverse())
				//foreach (var ParameterParameterType in SafeMethodTypeInfo.Parameters)
				{
					var FunctionParameterType = ParameterParameterType;
					var StackParameterType = TypeStack.Pop();
					
					if (FunctionParameterType == typeof(bool)) FunctionParameterType = typeof(int);
					if (StackParameterType == typeof(bool)) StackParameterType = typeof(int);

					if (CheckTypes)
					{
						if (FunctionParameterType != StackParameterType && (FunctionParameterType != typeof(object) && StackParameterType != typeof(object)))
						{
							if (GetCommonType(FunctionParameterType) != GetCommonType(StackParameterType))
							{
								throw (new InvalidOperationException(
									String.Format(
										"Type mismatch : Argument{0}. Expected: '{1}' but found on Stack: '{2}'",
										CurrentArgumentIndex, FunctionParameterType.Name, StackParameterType.Name
									)
								));
							}
						}
					}
					CurrentArgumentIndex++;
				}

				if (ParameterTypes != null)
				{
					foreach (var ParameterType in ParameterTypes) TypeStack.Pop();
				}

				if (!SafeMethodTypeInfo.IsStatic)
				{
					var ThisType = TypeStack.Pop();
				}

				if (SafeMethodTypeInfo.ReturnType != typeof(void))
				{
					TypeStack.Push(SafeMethodTypeInfo.ReturnType);
				}
			}

			if (DoEmit)
			{
				if (ParameterTypes != null)
				{
					__ILGenerator.EmitCall(OpCode, MethodInfo, ParameterTypes);
				}
				else
				{
					Emit(OpCode, MethodInfo);
				}
			}
		}

		public void ResetStack()
		{
			int StackCount = TypeStack.Count;

			for (int n = 0; n > StackCount; n++) Pop();
		}

		public void Jmp(MethodInfo MethodInfo, SafeMethodTypeInfo SafeMethodTypeInfo = null)
		{
			ResetStack();
			_Jmp_Call(OpCodes.Jmp, MethodInfo, SafeMethodTypeInfo, null);
		}

		public void Call(Delegate Delegate)
		{
			Call(Delegate.Method);
		}

		public void LoadFunctionPointer(MethodInfo MethodInfo, bool IsVirtual = false)
		{
			if (TrackStack)
			{
				if (IsVirtual) TypeStack.Pop();

				TypeStack.Push(typeof(void*));
			}

			if (DoEmit)
			{
				Emit(IsVirtual ? OpCodes.Ldvirtftn : OpCodes.Ldftn, MethodInfo);
			}
		}

		public void CallManagedFunction(CallingConventions CallingConventions, Type ReturnType, Type[] ParameterTypes, Type[] OptionalParameterTypes)
		{
			if (TrackStack)
			{
				// Parameters
				foreach (var ParameterType in ParameterTypes) TypeStack.Pop();

				// Function Pointer
				TypeStack.Pop();

				// Pushes the result
				if (ReturnType != typeof(void)) TypeStack.Push(ReturnType);
			}

			if (DoEmit)
			{
				__ILGenerator.EmitCalli(OpCodes.Calli, CallingConventions, ReturnType, ParameterTypes, OptionalParameterTypes);
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("CallCdecl({0}, {1}) :: Stack -> {0}", ReturnType, ParameterTypes, TypeStack.Count));
			}
		}

		public void CallUnmanagedFunction(CallingConvention CallingConvention, Type ReturnType, Type[] ParameterTypes)
		{
			if (TrackStack)
			{
				// Parameters
				foreach (var ParameterType in ParameterTypes) TypeStack.Pop();
				
				// Function Pointer
				TypeStack.Pop();

				// Pushes the result
				if (ReturnType != typeof(void)) TypeStack.Push(ReturnType);
			}

			if (DoEmit)
			{
				__ILGenerator.EmitCalli(OpCodes.Calli, CallingConvention, ReturnType, ParameterTypes);
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("CallCdecl({0}, {1}) :: Stack -> {0}", ReturnType, ParameterTypes, TypeStack.Count));
			}
		}

		public void Call(MethodInfo MethodInfo, SafeMethodTypeInfo SafeMethodTypeInfo = null, Type[] ParameterTypes = null)
		{
			_Jmp_Call(OpCodes.Call, MethodInfo, SafeMethodTypeInfo, ParameterTypes);
		}

		public void Break()
		{
			if (DoEmit)
			{
				Emit(OpCodes.Break);
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("Break() :: Stack -> {0}", TypeStack.Count));
			}
		}

		public void BranchAlways(SafeLabel Label, bool Short = false)
		{
			if (DoEmit)
			{
				Emit(Short ? OpCodes.Br_S : OpCodes.Br, Label);
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("BranchAlways() :: Stack -> {0}", TypeStack.Count));
			}
		}

		public void PopLeft()
		{
			while (TypeStack.Count > 0) Pop();
		}

		public void Pop()
		{
			if (TrackStack)
			{
				TypeStack.Pop();
			}

			if (DoEmit)
			{
				Emit(OpCodes.Pop);
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("Pop() :: Stack -> {0}", TypeStack.Count));
			}
		}

		public SafeArgument DeclareArgument(Type Type, ushort ArgumentIndex, string Name = "")
		{
			return new SafeArgument(Type, ArgumentIndex, Name);
		}

		public void LoadStoreArgument(SafeArgument Argument, Action Action)
		{
			LoadArgument(Argument);
			{
				Action();
			}
			StoreArgument(Argument);
		}

		public void LoadArgument(SafeArgument Argument)
		{
			LoadArgument(Argument.Type, Argument.Index);
		}

		public void LoadArgumentAddress(SafeArgument Argument)
		{
			LoadArgumentAddress(Argument.Type, Argument.Index);
		}

		public void LoadArgument<TType>(int ArgumentIndex)
		{
			LoadArgument(typeof(TType), ArgumentIndex);
		}

		public void LoadArgument(Type Type, int ArgumentIndex)
		{
			if (TrackStack)
			{
				TypeStack.Push(Type);
			}

			if (DoEmit)
			{
				switch (ArgumentIndex)
				{
					case 0: Emit(OpCodes.Ldarg_0); break;
					case 1: Emit(OpCodes.Ldarg_1); break;
					case 2: Emit(OpCodes.Ldarg_2); break;
					case 3: Emit(OpCodes.Ldarg_3); break;
					default:
						if ((ushort)(byte)ArgumentIndex == (ushort)ArgumentIndex)
						{
							Emit(OpCodes.Ldarg_S, (byte)ArgumentIndex);
						}
						else
						{
							Emit(OpCodes.Ldarg, (ushort)ArgumentIndex);
						}
					break;
				}
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("LoadArgument<{0}>({1}) :: Stack -> {2}", Type.Name, ArgumentIndex, TypeStack.Count));
			}
		}

		public void LoadArgumentAddress(Type Type, ushort ArgumentIndex)
		{
			if (TrackStack)
			{
				TypeStack.Push(Type.MakePointerType());
			}

			if (DoEmit)
			{
				if ((ushort)(byte)ArgumentIndex == (ushort)ArgumentIndex)
				{
					Emit(OpCodes.Ldarga_S, (byte)ArgumentIndex);
				}
				else
				{
					Emit(OpCodes.Ldarga, (ushort)ArgumentIndex);
				}
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("LoadArgumentAddress<{0}>({1}) :: Stack -> {2}", Type.Name, ArgumentIndex, TypeStack.Count));
			}
		}

		public void LoadArgumentFromIndexAtStack()
		{
			if (TrackStack)
			{
				var Type = TypeStack.Pop();
				
				// @check type is integer

				TypeStack.Push(typeof(object));
			}

			if (DoEmit)
			{
				Emit(OpCodes.Ldarg_S);
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("LoadArgumentFromIndexAtStack() :: Stack -> {0}", TypeStack.Count));
			}
		}

		private void _PushIntAs<TType>(int Value)
		{
			if (TrackStack)
			{
				TypeStack.Push(typeof(TType));
			}

			if (DoEmit)
			{
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
					default:
						if ((int)(sbyte)Value == (int)Value)
						{
							Emit(OpCodes.Ldc_I4_S, (sbyte)Value);
						}
						else
						{
							Emit(OpCodes.Ldc_I4, Value);
						}
						break;
				}
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("Push({0}) :: Stack -> {1}", Value, TypeStack.Count));
			}
		}

		public void Push(Type Value)
		{
			if (TrackStack)
			{
				TypeStack.Push(typeof(RuntimeTypeHandle));
			}

			if (DoEmit)
			{
				Emit(OpCodes.Ldtoken, Value);
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("Push({0}) :: Stack -> {1}", Value, TypeStack.Count));
			}
		}

		public void Push(MethodInfo Value)
		{
			if (TrackStack)
			{
				TypeStack.Push(typeof(MethodInfo));
			}

			if (DoEmit)
			{
				Emit(OpCodes.Ldtoken, Value);
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("Push({0}) :: Stack -> {1}", Value, TypeStack.Count));
			}
		}

		public void Push(FieldInfo Value)
		{
			if (TrackStack)
			{
				TypeStack.Push(typeof(FieldInfo));
			}

			if (DoEmit)
			{
				Emit(OpCodes.Ldtoken, Value);
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("Push({0}) :: Stack -> {1}", Value, TypeStack.Count));
			}
		}

		public void Push(bool Value)
		{
			_PushIntAs<bool>(Value ? 1 : 0);
		}

		public void Push(int Value)
		{
			_PushIntAs<int>(Value);
		}

		public void Push(char Value)
		{
			_PushIntAs<char>(Value);
		}

		public void Push(long Value)
		{
			if (TrackStack)
			{
				TypeStack.Push(typeof(long));
			}

			if (DoEmit)
			{
				Emit(OpCodes.Ldc_I8, Value);
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("Push({0}) :: Stack -> {1}", Value, TypeStack.Count));
			}
		}

		public void Push(float Value)
		{
			if (TrackStack)
			{
				TypeStack.Push(typeof(float));
			}

			if (DoEmit)
			{
				Emit(OpCodes.Ldc_R4, Value);
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("Push({0}) :: Stack -> {1}", Value, TypeStack.Count));
			}
		}

		public void Push(double Value)
		{
			if (TrackStack)
			{
				TypeStack.Push(typeof(float));
			}

			if (DoEmit)
			{
				Emit(OpCodes.Ldc_R8, Value);
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("Push({0}) :: Stack -> {1}", Value, TypeStack.Count));
			}
		}

		public void Push(string Value)
		{
			if (TrackStack)
			{
				TypeStack.Push(typeof(string));
			}

			if (DoEmit)
			{
				Emit(OpCodes.Ldstr, Value);
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("Push('{0}') :: Stack -> {1}", Value, TypeStack.Count));
			}
		}

		private void _LoadElement_Reference_Address(OpCode OpCode)
		{
			if (TrackStack)
			{
				var IndexType = TypeStack.Pop();
				var ArrayType = TypeStack.Pop();

				// @TODO: reference
				TypeStack.Push(typeof(object));
			}
			if (DoEmit)
			{
				Emit(OpCode);
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("_LoadElement_Reference_Address({0}) :: Stack -> {1}", OpCode.Name, TypeStack.Count));
			}
		}

		public void LoadElementAddress()
		{
			_LoadElement_Reference_Address(OpCodes.Ldelema);
		}

		public void LoadElementReference()
		{
			_LoadElement_Reference_Address(OpCodes.Ldelem_Ref);
		}

		public void StoreField(FieldInfo FieldInfo)
		{
			if (FieldInfo.IsStatic)
			{
				if (TrackStack)
				{
					var ValueType = TypeStack.Pop();
				}

				if (DoEmit)
				{
					Emit(OpCodes.Stsfld, FieldInfo);
				}

				if (DoDebug)
				{
					Debug.WriteLine(String.Format("StoreField({0}) :: Stack -> {1}", FieldInfo, TypeStack.Count));
				}
			}
			else
			{
				if (TrackStack)
				{
					var ValueType = TypeStack.Pop();
					var ObjectType = TypeStack.Pop();
				}

				if (DoEmit)
				{
					Emit(OpCodes.Stfld, FieldInfo);
				}

				if (DoDebug)
				{
					Debug.WriteLine(String.Format("StoreField({0}) :: Stack -> {1}", FieldInfo, TypeStack.Count));
				}
			}
		}

		private void _LoadField_Address(OpCode OpCode, FieldInfo FieldInfo, bool Address)
		{
			if (FieldInfo == null)
			{
				throw(new ArgumentNullException("FieldInfo can't be null"));
			}

			if (TrackStack)
			{
				//var FieldInfoType = TypeStack.Pop();
				var TypePointer = TypeStack.Pop();
				var ExpectedType = FieldInfo.FieldType;
				if (Address) ExpectedType = FieldInfo.DeclaringType.MakePointerType();
#if false
				if (TypePointer != ExpectedType)
				{
					throw (new InvalidOperationException(String.Format("{0} != {1}", TypePointer, ExpectedType)));
				}
#endif
				TypeStack.Push(ExpectedType);
			}
	
			if (DoEmit)
			{
				Emit(OpCode, FieldInfo);
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("_LoadField_Address({0}) :: Stack -> {1}", OpCode.Name, TypeStack.Count));
			}
		}

		public void LoadField(FieldInfo FieldInfo)
		{
			_LoadField_Address(FieldInfo.IsStatic ? OpCodes.Ldsfld : OpCodes.Ldfld, FieldInfo, Address: false);
		}

		public void LoadFieldAddress(FieldInfo FieldInfo, bool UseLoadFieldAddress = true)
		{
			if (UseLoadFieldAddress)
			{
				_LoadField_Address(FieldInfo.IsStatic ? OpCodes.Ldsflda : OpCodes.Ldflda, FieldInfo, Address: true);
			}
			else
			{
				Push((int)Marshal.OffsetOf(FieldInfo.DeclaringType, FieldInfo.Name));
				BinaryOperation(SafeBinaryOperator.AdditionSigned);
			}
		}

		public void LoadMethodAddress()
		{
			if (TrackStack)
			{
				var MethodInfoType = TypeStack.Pop();
				var ObjectType = TypeStack.Pop();

				// @TODO: Field reference
				TypeStack.Push(typeof(object));
			}

			if (DoEmit)
			{
				Emit(OpCodes.Ldftn);
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("LoadMethodAddress() :: Stack -> {0}", TypeStack.Count));
			}
			//OpCodes.Ldind_I
		}

		public void LoadLength()
		{
			if (TrackStack)
			{
				var ArrayType = TypeStack.Pop();
				TypeStack.Push(typeof(int));
			}

			if (DoEmit)
			{
				Emit(OpCodes.Ldlen); 
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("LoadLength() :: Stack -> {0}", TypeStack.Count));
			}
		}

		public void LoadLocal(LocalBuilder Local)
		{
			if (TrackStack)
			{
				TypeStack.Push(Local.LocalType);
			}

			if (DoEmit)
			{
				int LocalIndex = Local.LocalIndex;
				switch (Local.LocalIndex)
				{
					case 0: Emit(OpCodes.Ldloc_0); break;
					case 1: Emit(OpCodes.Ldloc_1); break;
					case 2: Emit(OpCodes.Ldloc_2); break;
					case 3: Emit(OpCodes.Ldloc_3); break;
					default: Emit(((int)(byte)LocalIndex == (int)LocalIndex) ? OpCodes.Ldloc_S : OpCodes.Ldloc, Local); break;
				}
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("LoadLocal({0}) :: Stack -> {1}", Local, TypeStack.Count));
			}
		}

		public void LoadLocalAddress(LocalBuilder Local)
		{
			if (TrackStack)
			{
				TypeStack.Push(Local.LocalType.MakePointerType());
			}

			if (DoEmit)
			{
				int LocalIndex = Local.LocalIndex;

				Emit(((int)(byte)LocalIndex == (int)LocalIndex) ? OpCodes.Ldloca_S : OpCodes.Ldloca, Local);
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("LoadLocalAddress({0}) :: Stack -> {1}", Local, TypeStack.Count));
			}
		}

		public void LoadNull()
		{
			if (TrackStack)
			{
				TypeStack.Push(typeof(DBNull));
			}

			if (DoEmit)
			{
				Emit(OpCodes.Ldnull);
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("LoadNull() :: Stack -> {0}", TypeStack.Count));
			}
		}

		public void LoadIndirect<TType>()
		{
			LoadIndirect(typeof(TType));
		}

		public void LoadIndirect(Type Type)
		{
			if (TrackStack)
			{
				var PointerType = TypeStack.Pop();
				TypeStack.Push(Type);
			}

			if (DoEmit)
			{
				while (true)
				{
					if (Type == typeof(bool)) { Emit(OpCodes.Ldind_I); break; }
					if (Type == typeof(sbyte)) { Emit(OpCodes.Ldind_I1); break; }
					if (Type == typeof(short)) { Emit(OpCodes.Ldind_I2); break; }
					if (Type == typeof(int)) { Emit(OpCodes.Ldind_I4); break; }
					if (Type == typeof(long)) { Emit(OpCodes.Ldind_I8); break; }
					if (Type == typeof(float)) { Emit(OpCodes.Ldind_R4); break; }
					if (Type == typeof(double)) { Emit(OpCodes.Ldind_R8); break; }
					if (Type == typeof(byte)) { Emit(OpCodes.Ldind_U1); break; }
					if (Type == typeof(ushort)) { Emit(OpCodes.Ldind_U2); break; }
					if (Type == typeof(uint)) { Emit(OpCodes.Ldind_U4); break; }
					//if (Type.IsPointer) { Emit(OpCodes.Ldind_Ref); break; }
					if (Type.IsPointer) { Emit(OpCodes.Ldind_I); break; }
					//Emit(OpCodes.Ldelem);
					
					//if (Type.IsClass) { Emit(OpCodes.Conv_U); Emit(OpCodes.Ldobj, Type); break; }
					if (true) { Emit(OpCodes.Conv_U); Emit(OpCodes.Ldobj, Type); break; }

					throw (new NotImplementedException("Can't load indirectly type '" + Type.Name + "'"));
				}
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("LoadIndirect({0}) :: Stack -> {1}", Type, TypeStack.Count));
			}
		}

		public void LoadElementFromArray(Type Type)
		{
			if (TrackStack)
			{
				var IndexType = TypeStack.Pop();
				var ArrayType = TypeStack.Pop();
				TypeStack.Push(Type);
			}

			if (DoEmit)
			{
				while (true)
				{
					if (Type == typeof(bool)) { Emit(OpCodes.Ldelem_I); break; }
					if (Type == typeof(sbyte)) { Emit(OpCodes.Ldelem_I1); break; }
					if (Type == typeof(short)) { Emit(OpCodes.Ldelem_I2); break; }
					if (Type == typeof(int)) { Emit(OpCodes.Ldelem_I4); break; }
					if (Type == typeof(long)) { Emit(OpCodes.Ldelem_I8); break; }
					if (Type == typeof(float)) { Emit(OpCodes.Ldelem_R4); break; }
					if (Type == typeof(double)) { Emit(OpCodes.Ldelem_R8); break; }
					if (Type == typeof(byte)) { Emit(OpCodes.Ldelem_U1); break; }
					if (Type == typeof(ushort)) { Emit(OpCodes.Ldelem_U2); break; }
					if (Type == typeof(uint)) { Emit(OpCodes.Ldelem_U4); break; }
					//Emit(OpCodes.Ldelem);
					throw (new NotImplementedException());
				}
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("LoadElementFromArray({0}) :: Stack -> {1}", Type, TypeStack.Count));
			}
		}

		public void LoadElementFromArray<TType>()
		{
			LoadElementFromArray(typeof(TType));
		}

		public void Push(uint Value)
		{
			Push(unchecked((int)Value));
		}

		public void Push(ulong Value)
		{
			Push(unchecked((long)Value));
		}

		public void EmitWriteLine(String Value)
		{
			EmitHookWriteLine(Value);
			__ILGenerator.EmitWriteLine(Value);
		}

		public void CastClass(Type Type)
		{
			if (TrackStack)
			{
				var ObjectType = TypeStack.Pop();
				TypeStack.Push(Type);
			}

			if (DoEmit)
			{
				Emit(OpCodes.Castclass, Type);
			}

			if (DoDebug)
			{
				Debug.WriteLine(String.Format("CastClass({0}) :: Stack -> {1}", Type, TypeStack.Count));
			}
		}

		static public Type GetIntegralTypeByDescription(int Size, bool Signed)
		{
			switch (Size)
			{
				case 1: return Signed ? typeof(sbyte) : typeof(byte);
				case 2: return Signed ? typeof(short) : typeof(ushort);
				case 4: return Signed ? typeof(int) : typeof(uint);
				case 8: return Signed ? typeof(long) : typeof(ulong);
				default: throw(new NotImplementedException("Not Integral Type"));
			}
		}

		public void SaveRestoreTypeStack(Action Action)
		{
			var OldTypeStack = TypeStack.Clone2();
			try
			{
				Action();
			}
			finally
			{
				TypeStack = OldTypeStack;
			}
		}

		public int GetOffsetIncrement(Action Action)
		{
			var ILOffsetStart = __ILGenerator.ILOffset;
			Action();
			var ILOffsetEnd = __ILGenerator.ILOffset;
			return ILOffsetEnd - ILOffsetStart;
		}

		public void MacroIf(Action IfAction)
		{
			var IfLabel = DefineLabel("If");
			var EndLabel = DefineLabel("End");

			BranchUnaryComparison(SafeUnaryComparison.False, EndLabel);
			// If
			IfLabel.Mark();
			{
				//SaveRestoreTypeStack(() => {
				IfAction();
				//});
			}
			EndLabel.Mark();
		}

		public void MacroIfElse(Action IfAction, Action ElseAction)
		{
			var IfLabel = DefineLabel("If");
			var ElseLabel = DefineLabel("Else");
			var EndLabel = DefineLabel("End");

			BranchUnaryComparison(SafeUnaryComparison.False, ElseLabel);
			// If
			IfLabel.Mark();
			{
				//SaveRestoreTypeStack(() => {
					IfAction();
				//});

				BranchAlways(EndLabel);
			}
			// Else
			ElseLabel.Mark();
			{
				//SaveRestoreTypeStack(() =>  {
					ElseAction();
				//});
			}
			// End
			EndLabel.Mark();
		}

		public IEnumerable<LocalBuilder> StackSave()
		{
			// TODO: Check if the stack fits or it should be a queue!
			var SavedStack = new Stack<LocalBuilder>();
			while (TypeStack.Count > 0)
			{
				var Type = TypeStack.Pop(); TypeStack.Push(Type);

				var Temp1 = DeclareLocal(Type);
				StoreLocal(Temp1);
				SavedStack.Push(Temp1);
			}
			return SavedStack;
		}

		public void StackRestore(IEnumerable<LocalBuilder> SavedStack)
		{
			foreach (var Local in SavedStack) LoadLocal(Local);
		}
	}

	public class SafeArgument
	{
		internal Type Type;
		internal ushort Index;
		internal string Name;

		internal SafeArgument(Type Type, ushort Index, string Name)
		{
			this.Type = Type;
			this.Index = Index;
			this.Name = Name;
		}
	}
}
