using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using CSharpUtils;
using CSPspEmu.Core.Gpu.State;

namespace CSPspEmu.Core.Gpu.VertexReading
{
	unsafe public partial class VertexReaderDynarec
	{
		private void _LoadPointerAlignedTo(uint Alignment)
		{
			Offset = MathUtils.NextAligned(Offset, (int)Alignment);
			ILGenerator.Emit(OpCodes.Ldarg_0); // void* VertexData
			ILGenerator.Emit(OpCodes.Ldc_I4, Offset);
			ILGenerator.Emit(OpCodes.Add);
		}

		private void IncOffset(uint Size)
		{
			Offset += Size;
			Console.Error.WriteLine("{0} -> {1}", Offset - Size, Offset);
		}

		private void _LoadIntegerAsInteger(uint Size, bool Signed)
		{
			_LoadPointerAlignedTo(Alignment: Size);
			switch (Size)
			{
				case 1: ILGenerator.Emit(Signed ? OpCodes.Ldind_I1 : OpCodes.Ldind_U1); break;
				case 2: ILGenerator.Emit(Signed ? OpCodes.Ldind_I2 : OpCodes.Ldind_U2); break;
				case 4: ILGenerator.Emit(Signed ? OpCodes.Ldind_I4 : OpCodes.Ldind_U4); break;
				case 8: ILGenerator.Emit(Signed ? OpCodes.Ldind_I8 : OpCodes.Ldind_I8); break;
				default: throw (new NotImplementedException("_LoadIntegerScaled: " + Size));
			}
			IncOffset(Size);
		}

		private void _LoadSignedIntegerScaled(uint Size, float Scale, bool Signed)
		{
			_LoadIntegerAsInteger(Size, Signed);
			ILGenerator.Emit(OpCodes.Conv_R4);
			if (Scale != 1.0f)
			{
				ILGenerator.Emit(OpCodes.Ldc_R4, Scale);
				ILGenerator.Emit(OpCodes.Div);
			}
		}

		private void _LoadFloat()
		{
			_LoadPointerAlignedTo(Alignment: sizeof(float));
			ILGenerator.Emit(OpCodes.Ldind_R4);
			IncOffset(sizeof(float));
		}

		private void _LoadIntegerScaled(uint Size, bool Signed)
		{
			_LoadSignedIntegerScaled(
				Size: Size,
				Scale: (float)(Math.Pow(2, Size * 8) / 2),
				Signed : Signed
			);
		}

		private void _LoadIntegerScaledIfNotTransform2D(uint Size, bool Signed)
		{
			_LoadSignedIntegerScaled(
				Size: Size,
				Scale: VertexType.Transform2D ? 1.0f : ((float)(Math.Pow(2, Size * 8) / 2)),
				Signed : Signed
			);
		}

		private void _SaveFloatField(string FieldName, Action Action)
		{
			ILGenerator.Emit(OpCodes.Ldarg_1);
			ILGenerator.Emit(OpCodes.Ldflda, typeof(VertexInfo).GetField(FieldName));
			{
				Action();
			}
			ILGenerator.Emit(OpCodes.Stind_R4);
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

		private uint TypeSize(VertexTypeStruct.NumericEnum Type)
		{
			return VertexTypeStruct.TypeSizeTable[(int)Type];
		}
	}
}
