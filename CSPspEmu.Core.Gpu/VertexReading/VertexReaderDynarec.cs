using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using CSharpUtils;
using CSPspEmu.Core.Gpu.State;

namespace CSPspEmu.Core.Gpu.VertexReading
{
	unsafe public delegate void VertexReaderDelegate(void* VertexData, VertexInfo* Vertex, int Index, int Count);

	unsafe public partial class VertexReaderDynarec
	{
		protected uint Offset;
		protected VertexTypeStruct VertexType;
		protected DynamicMethod DynamicMethod;
		protected ILGenerator ILGenerator;
		protected LocalBuilder LocalColor;

		static public VertexReaderDelegate GenerateMethod(VertexTypeStruct VertexType)
		{
			return new VertexReaderDynarec(VertexType).GenerateDelegate();
		}

		protected VertexReaderDynarec(VertexTypeStruct VertexType)
		{
			this.VertexType = VertexType;
			this.Offset = 0;
			this.DynamicMethod = new DynamicMethod(
				String.Format("GenerateReaderMethod"),
				typeof(void),
				new[] { typeof(void*), typeof(VertexInfo*), typeof(int), typeof(int) },
				//typeof(VertexReaderDelegate).DeclaringMethod.GetParameters().Select(Item => Item.ParameterType).ToArray(),
				Assembly.GetExecutingAssembly().ManifestModule
			);
			this.ILGenerator = DynamicMethod.GetILGenerator();
			this.LocalColor = this.ILGenerator.DeclareLocal(typeof(uint));
		}

		private void AlignTo(int Alignment)
		{
			Offset = MathUtils.NextAligned(Offset, Alignment);
		}

		private void ReadAll()
		{
			//AlignTo(VertexType.GetMaxAlignment());
			Read_Weights();

			//AlignTo(VertexType.GetMaxAlignment());
			Read_TextureCoordinates();

			//AlignTo(VertexType.GetMaxAlignment());
			Read_Color();

			//AlignTo(VertexType.GetMaxAlignment());
			Read_Normal();

			//AlignTo(VertexType.GetMaxAlignment());
			Read_Position();
		}

		private void Read_Weights()
		{
			var Type = VertexType.Weight;
			if (Type == VertexTypeStruct.NumericEnum.Void) return;
			for (int n = 0; n < VertexType.SkinningWeightCount; n++)
			{
				_SaveFloatField("Weight" + n, () =>
				{
					switch (Type)
					{
						case VertexTypeStruct.NumericEnum.Byte: _LoadIntegerScaled(Size: 1, Signed: true); break;
						case VertexTypeStruct.NumericEnum.Short: _LoadIntegerScaled(Size: 2, Signed: true); break;
						case VertexTypeStruct.NumericEnum.Float: _LoadFloat(); break;
					}
				});
			}
		}

		private void Read_TextureCoordinates()
		{
			Read_Type0(VertexType.Texture, new[] { "U", "V" }, Signed: false);
		}

		private void Read_Color()
		{
			var Type = VertexType.Color;
			if (Type == State.VertexTypeStruct.ColorEnum.Void) return;
			switch (Type)
			{
				case VertexTypeStruct.ColorEnum.Color4444: Read_Color(ColorFormats.RGBA_4444); break;
				case VertexTypeStruct.ColorEnum.Color5551: Read_Color(ColorFormats.RGBA_5551); break;
				case VertexTypeStruct.ColorEnum.Color5650: Read_Color(ColorFormats.RGBA_5650); break;
				case VertexTypeStruct.ColorEnum.Color8888: Read_Color(ColorFormats.RGBA_8888); break;
				default: throw(new NotImplementedException());
			}
		}

		private void Read_Normal()
		{
			Read_Type0(VertexType.Normal, new[] { "NX", "NY", "NZ" }, Signed: true);
		}

		private void Read_Position()
		{
			var Type = VertexType.Position;
			Read_Type0(Type, new[] { "PX", "PY" }, Signed: true);
			Read_Type0(VertexType.Position, new[] { "PZ" }, Signed: !VertexType.Transform2D);
		}

		private void Read_Type0(VertexTypeStruct.NumericEnum Type, string[] Fields, bool Signed = true)
		{
			if (Type == VertexTypeStruct.NumericEnum.Void) return;
			var Size = TypeSize(Type);
			_SaveFloatFields(Fields, () =>
			{
				switch (Type)
				{
					case VertexTypeStruct.NumericEnum.Byte:
					case VertexTypeStruct.NumericEnum.Short:
						_LoadIntegerScaledIfNotTransform2D(Size: Size, Signed: Signed);
						break;
					case VertexTypeStruct.NumericEnum.Float:
						_LoadFloat();
						break;
				}
			});
		}

		private void Read_Color(ColorFormat ColorFormat)
		{
			_LoadIntegerAsInteger(Size: (uint)ColorFormat.TotalBytes, Signed: false);
			ILGenerator.Emit(OpCodes.Stloc, LocalColor);
			Read_Color_Component("R", ColorFormat.Red);
			Read_Color_Component("G", ColorFormat.Green);
			Read_Color_Component("B", ColorFormat.Blue);
			Read_Color_Component("A", ColorFormat.Alpha);

		}

		private void Read_Color_Component(String ComponentName, ColorFormat.Component ComponentInfo)
		{
			_SaveFloatField(ComponentName, () =>
			{
				ILGenerator.Emit(OpCodes.Ldloc, LocalColor);
				ILGenerator.Emit(OpCodes.Ldc_I4, ComponentInfo.Offset);
				ILGenerator.Emit(OpCodes.Shr_Un);
				ILGenerator.Emit(OpCodes.Ldc_I4, BitUtils.CreateMask(ComponentInfo.Size));
				ILGenerator.Emit(OpCodes.And);
				ILGenerator.Emit(OpCodes.Conv_R4);
				ILGenerator.Emit(OpCodes.Ldc_R4, (float)ComponentInfo.Mask);
				ILGenerator.Emit(OpCodes.Div);
			});
		}


		private VertexReaderDelegate GenerateDelegate()
		{
			var LoopLabel = ILGenerator.DefineLabel();

			ILGenerator.Emit(OpCodes.Ldarg_0); // void* VertexData
			ILGenerator.Emit(OpCodes.Ldarg_2); // int Index
			ILGenerator.Emit(OpCodes.Ldc_I4, VertexType.GetVertexSize());
			ILGenerator.Emit(OpCodes.Mul);
			ILGenerator.Emit(OpCodes.Add);
			ILGenerator.Emit(OpCodes.Starg, 0);

			// :loop
			ILGenerator.MarkLabel(LoopLabel);
			{
				ReadAll();
			}

			// VertexData += VertexSize
			{
				ILGenerator.Emit(OpCodes.Ldarg_0); // void* VertexData
				ILGenerator.Emit(OpCodes.Ldc_I4, VertexType.GetVertexSize());
				ILGenerator.Emit(OpCodes.Add);
				ILGenerator.Emit(OpCodes.Starg, 0);
			}

			// Vertex++
			{
				ILGenerator.Emit(OpCodes.Ldarg_1); // VertexInfo* Vertex
				ILGenerator.Emit(OpCodes.Ldc_I4, sizeof(VertexInfo));
				ILGenerator.Emit(OpCodes.Add);
				ILGenerator.Emit(OpCodes.Starg, 1);
			}

			// Count--
			{
				ILGenerator.Emit(OpCodes.Ldarg_3); // int Count
				ILGenerator.Emit(OpCodes.Ldc_I4, -1);
				ILGenerator.Emit(OpCodes.Add);
				ILGenerator.Emit(OpCodes.Starg, 3);
			}

			ILGenerator.Emit(OpCodes.Ldarg_3);
			ILGenerator.Emit(OpCodes.Brtrue, LoopLabel);

			Offset = MathUtils.NextAligned(Offset, VertexType.GetMaxAlignment());

			Console.Error.WriteLine("{0}", VertexType);
			Console.Error.WriteLine("Get:{0}, Calculated:{1}", Offset, VertexType.GetVertexSize());
			Debug.Assert(Offset == VertexType.GetVertexSize());

			ILGenerator.Emit(OpCodes.Ret);
			//Console.Error.WriteLine(DynamicMethod.GetMethodBody().GetILAsByteArray());
			var Delegate = (VertexReaderDelegate)DynamicMethod.CreateDelegate(typeof(VertexReaderDelegate));
			return Delegate;
		}
	}
}
