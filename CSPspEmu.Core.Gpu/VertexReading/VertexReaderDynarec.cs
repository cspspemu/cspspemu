using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Codegen;
using CSharpUtils;
using CSPspEmu.Core.Gpu.State;

namespace CSPspEmu.Core.Gpu.VertexReading
{
	unsafe public delegate void VertexReaderDelegate(void* VertexData, VertexInfo* Vertex, int Index, int Count);

	unsafe public partial class VertexReaderDynarec
	{
		protected uint Offset;
		protected VertexTypeStruct VertexType;
		protected SafeILGenerator SafeILGenerator;
		protected LocalBuilder LocalColor;
		protected SafeArgument VertexDataArgument;
		protected SafeArgument VertexInfoArgument;
		protected SafeArgument IndexArgument;
		protected SafeArgument CountArgument;

		static public VertexReaderDelegate GenerateMethod(VertexTypeStruct VertexType)
		{
			return SafeILGenerator.Generate<VertexReaderDelegate>("VertexReaderDynarec.GenerateMethod", (Generator) =>
			{
				var VertexReaderDynarec = new VertexReaderDynarec(Generator, VertexType);
				VertexReaderDynarec.GenerateCode();
			}, CheckTypes: false);
		}

		protected VertexReaderDynarec(SafeILGenerator SafeILGenerator, VertexTypeStruct VertexType)
		{
			this.Offset = 0;
			this.SafeILGenerator = SafeILGenerator;
			this.VertexType = VertexType;
			this.VertexDataArgument = SafeILGenerator.DeclareArgument(typeof(void*), 0);
			this.VertexInfoArgument = SafeILGenerator.DeclareArgument(typeof(VertexInfo*), 0);
			this.IndexArgument = SafeILGenerator.DeclareArgument(typeof(int), 0);
			this.CountArgument = SafeILGenerator.DeclareArgument(typeof(int), 0);
			this.LocalColor = SafeILGenerator.DeclareLocal<uint>("LocalColor", false);
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
			int WeightCount = VertexType.RealSkinningWeightCount;
			for (int n = 0; n < WeightCount; n++)
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
			Read_Type0(VertexType.Texture, new[] { "Texture.X", "Texture.Y" }, Signed: false);
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
			Read_Type0(VertexType.Normal, new[] { "Normal.X", "Normal.Y", "Normal.Z" }, Signed: true);
		}

		private void Read_Position()
		{
			var Type = VertexType.Position;
			Read_Type0(Type, new[] { "Position.X", "Position.Y" }, Signed: true);
			Read_Type0(VertexType.Position, new[] { "Position.Z" }, Signed: !VertexType.Transform2D);
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
			_LoadIntegerAsInteger(Size: ColorFormat.TotalBytes, Signed: false);
			SafeILGenerator.StoreLocal(LocalColor);
			Read_Color_Component("Color.R", ColorFormat.Red);
			Read_Color_Component("Color.G", ColorFormat.Green);
			Read_Color_Component("Color.B", ColorFormat.Blue);
			Read_Color_Component("Color.A", ColorFormat.Alpha);

		}

		private void Read_Color_Component(String ComponentName, ColorFormat.Component ComponentInfo)
		{
			_SaveFloatField(ComponentName, () =>
			{
				SafeILGenerator.LoadLocal(LocalColor);
				SafeILGenerator.Push((int)ComponentInfo.Offset);
				SafeILGenerator.BinaryOperation(SafeBinaryOperator.ShiftRightUnsigned);
				SafeILGenerator.Push((int)BitUtils.CreateMask(ComponentInfo.Size));
				SafeILGenerator.BinaryOperation(SafeBinaryOperator.And);
				SafeILGenerator.ConvertTo<float>();
				SafeILGenerator.Push((float)ComponentInfo.Mask);
				SafeILGenerator.BinaryOperation(SafeBinaryOperator.DivideSigned);
			});
		}


		private void GenerateCode()
		{
			var LoopLabel = SafeILGenerator.DefineLabel("LoopLabel");

			SafeILGenerator.LoadStoreArgument(VertexDataArgument, () =>
			{
				SafeILGenerator.LoadArgument(IndexArgument);
				SafeILGenerator.Push((int)VertexType.GetVertexSize());
				SafeILGenerator.BinaryOperation(SafeBinaryOperator.MultiplySigned);
				SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned);
			});

			// :loop
			LoopLabel.Mark();
			{
				ReadAll();
			}

			// VertexData += VertexSize
			{
				SafeILGenerator.LoadStoreArgument(VertexDataArgument, () =>
				{
					SafeILGenerator.Push((int)VertexType.GetVertexSize());
					SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned);
				});
			}

			// Vertex++
			{
				SafeILGenerator.LoadStoreArgument(VertexInfoArgument, () =>
				{
					SafeILGenerator.Push(sizeof(VertexInfo));
					SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned);
				});
			}

			// Count--
			{
				SafeILGenerator.LoadStoreArgument(CountArgument, () =>
				{
					SafeILGenerator.Push(-1);
					SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned);
				});
			}

			SafeILGenerator.LoadArgument(CountArgument);
			SafeILGenerator.BranchIfTrue(LoopLabel);

			Offset = MathUtils.NextAligned(Offset, VertexType.GetMaxAlignment());

			Console.Error.WriteLine("{0}", VertexType);
			Console.Error.WriteLine("Get:{0}, Calculated:{1}", Offset, VertexType.GetVertexSize());
			Debug.Assert(Offset == VertexType.GetVertexSize());

			SafeILGenerator.Return(typeof(void));
			//Console.Error.WriteLine(DynamicMethod.GetMethodBody().GetILAsByteArray());
		}
	}
}
