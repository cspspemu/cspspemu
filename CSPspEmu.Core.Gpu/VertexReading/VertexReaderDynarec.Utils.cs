using System;
using CSharpUtils;
using CSPspEmu.Core.Gpu.State;
using SafeILGenerator;

namespace CSPspEmu.Core.Gpu.VertexReading
{
    public partial class VertexReaderDynarec
	{
		private void _LoadPointerAlignedTo(int Alignment)
		{
			Offset = MathUtils.NextAligned(Offset, (int)Alignment);
			SafeILGenerator.LoadArgument(VertexDataArgument);
			SafeILGenerator.Push((int)Offset);
			SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned);
		}

		private void IncOffset(int Size)
		{
			Offset += (uint)Size;
			Console.Error.WriteLine("{0} -> {1}", Offset - Size, Offset);
		}

		private void _LoadIntegerAsInteger(int Size, bool Signed)
		{
			_LoadPointerAlignedTo(Alignment: Size);
			SafeILGenerator.LoadIndirect(CSafeILGenerator.GetIntegralTypeByDescription(Size, Signed));
			IncOffset(Size);
		}

		private void _LoadSignedIntegerScaled(int Size, float Scale, bool Signed)
		{
			_LoadIntegerAsInteger(Size, Signed);
			SafeILGenerator.ConvertTo<float>();
			if (Scale != 1.0f)
			{
				SafeILGenerator.Push((float)Scale);
				SafeILGenerator.BinaryOperation(SafeBinaryOperator.DivideSigned);
			}
		}

		private void _LoadFloat()
		{
			_LoadPointerAlignedTo(Alignment: sizeof(float));
			SafeILGenerator.LoadIndirect<float>();
			IncOffset(sizeof(float));
		}

		private void _LoadIntegerScaled(int Size, bool Signed)
		{
			_LoadSignedIntegerScaled(
				Size: Size,
				Scale: (float)(Math.Pow(2, Size * 8) / 2),
				Signed : Signed
			);
		}

		private void _LoadIntegerScaledIfNotTransform2D(int Size, bool Signed)
		{
			_LoadSignedIntegerScaled(
				Size: Size,
				Scale: VertexType.Transform2D ? 1.0f : ((float)(Math.Pow(2, Size * 8) / 2)),
				Signed : Signed
			);
		}

		private void _SaveFloatField(string FieldName, Action Action)
		{
			SafeILGenerator.LoadArgument(VertexInfoArgument);
			var Field = typeof(VertexInfo).GetField(FieldName);
			if (Field == null) throw(new Exception("Can't find field '" + FieldName + "'"));
			SafeILGenerator.LoadFieldAddress(Field);
			{
				Action();
			}
			SafeILGenerator.StoreIndirect<float>();
		}

		private void _SaveFloatFields(string[] FieldNames, Action Action)
		{
			foreach (var FieldName in FieldNames)
			{
				_SaveFloatField(FieldName, Action);
			}
		}

		/*
		private void _LoadSignedByteScaled(ref int Offset, ILGenerator ILGenerator)
		{
			_LoadIntegerScaled(ref Offset, ILGenerator, Size: 1, Scale: 128f);
		}

		private void _LoadSignedShortScaled(ref int Offset, ILGenerator ILGenerator)
		{
			_LoadIntegerScaled(ref Offset, ILGenerator, Size: 2, Scale: 32768f);
		}
		*/

		private static int TypeSize(VertexTypeStruct.NumericEnum Type)
		{
			return VertexTypeStruct.TypeSizeTable[(int)Type];
		}
	}
}
