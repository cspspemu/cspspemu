using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Gpu.State;
using CSPspEmu.Core.Utils;

namespace CSPspEmu.Core.Gpu.Impl.Opengl
{
	/// <summary>
	/// 20 floats.
	/// </summary>
	public struct VertexInfo
	{
		public float R, G, B, A;
		public float PX, PY, PZ;
		public float NX, NY, NZ;
		public float U, V;
		public float Weight0, Weight1, Weight2, Weight3, Weight4, Weight5, Weight6, Weight7;

		public override string ToString()
		{
			return String.Format(
				"VertexInfo(Position=({0}, {1}, {2}), Normal=({3}, {4}, {5}), UV=({6}, {7}), COLOR=(R:{8}, G:{9}, B:{10}, A:{11}))",
				PX, PY, PZ,
				NX, NY, NZ,
				U, V,
				R, G, B, A
			);
		}
	}

	unsafe public class VertexReader
	{
		protected uint VertexAlignSize = 1;
		protected uint VertexSize;
		protected uint SkinningWeightCount;
		protected byte* BasePointer;
		protected byte* Pointer;
		protected VertexInfo* VertexInfo;

		// Lists
		protected Action[] ReadWeightsList;
		protected Action[] ReadTextureCoordinatesList;
		protected Action[] ReadColorList;
		protected Action[] ReadNormalList;
		protected Action[] ReadPositionList;

		// Current Actions
		protected Action VertexAlignment;
		protected Action ReadWeights;
		protected Action ReadTextureCoordinates;
		protected Action ReadColor;
		protected Action ReadNormal;
		protected Action ReadPosition;

		protected bool Transform2D;

		public VertexReader()
		{
			ReadWeightsList = new Action[] { Void, ReadWeightByte, ReadWeightShort, ReadWeightFloat };
			ReadTextureCoordinatesList = new Action[] { Void, ReadTextureCoordinatesByte, ReadTextureCoordinatesShort, ReadTextureCoordinatesFloat };
			ReadColorList = new Action[] { Void, Invalid, Invalid, Invalid, ReadColor5650, ReadColor5551, ReadColor4444, ReadColor8888 };
			ReadNormalList = new Action[] { Void, ReadNormalByte, ReadNormalShort, ReadNormalFloat };
			ReadPositionList = new Action[] { Void, ReadPositionByte, ReadPositionShort, ReadPositionFloat };
		}

		public void SetVertexTypeStruct(VertexTypeStruct VertexTypeStruct, byte* BasePointer)
		{
			Transform2D = VertexTypeStruct.Transform2D;
			
			//Console.Error.WriteLine("SetVertexTypeStruct: " + VertexTypeStruct);
			SkinningWeightCount = VertexTypeStruct.SkinningWeightCount;
			//Console.WriteLine(SkinningWeightCount);
			VertexSize = VertexTypeStruct.GetVertexSize();
			{
				ReadWeights = ReadWeightsList[(int)VertexTypeStruct.Weight];
				ReadTextureCoordinates = ReadTextureCoordinatesList[(int)VertexTypeStruct.Texture];
				ReadColor = ReadColorList[(int)VertexTypeStruct.Color];
				ReadNormal = ReadNormalList[(int)VertexTypeStruct.Normal];
				ReadPosition = ReadPositionList[(int)VertexTypeStruct.Position];

				switch (VertexTypeStruct.StructAlignment)
				{
					case 4: VertexAlignment = Align4; break;
					case 2: VertexAlignment = Align2; break;
					default: VertexAlignment = Align1; break;
				}
			}
			//public VertexTypeStruct VertexTypeStruct;
			this.BasePointer = BasePointer;
		}

		public void ReadVertex(int Index, VertexInfo* VertexInfo)
		{
			this.Pointer = &BasePointer[VertexSize * Index];
			this.VertexInfo = VertexInfo;
			
			// Vertex has to be aligned to the maxium size of any component. 
			VertexAlignment();
			
			ReadWeights();
			ReadTextureCoordinates();
			ReadColor();
			ReadNormal();
			ReadPosition();
		}

		protected void Align1()
		{
		}

#if false
		protected void Align2() { Pointer = (byte*)((uint)Pointer & unchecked((uint)~1)); }
		protected void Align4() { Pointer = (byte*)((uint)Pointer & unchecked((uint)~4)); }
#else
		protected void Align2()
		{
			if (((uint)Pointer & 1) != 0)
			{
				Pointer = (byte*)(((uint)Pointer + 2) & unchecked((uint)~1));
			}
		}
		protected void Align4()
		{
			if (((uint)Pointer & 3) != 0)
			{
				Pointer = (byte*)(((uint)Pointer + 4) & unchecked((uint)~3));
			}
		}
#endif

		protected void Void()
		{
		}

		protected void Invalid()
		{
			throw(new InvalidOperationException());
		}

		protected void ReadTextureCoordinatesByte()
		{
			Align1();

			VertexInfo->U = (float)((byte*)Pointer)[1];
			VertexInfo->V = (float)((byte*)Pointer)[1];

			if (!Transform2D)
			{
				VertexInfo->U /= 128.0f;
				VertexInfo->V /= 128.0f;
			}

			Pointer += sizeof(byte) * 2;
		}

		protected void ReadTextureCoordinatesShort()
		{
			Align2();
			VertexInfo->U = (float)((ushort*)Pointer)[0];
			VertexInfo->V = (float)((ushort*)Pointer)[1];
			
			if (!Transform2D)
			{
				VertexInfo->U /= 32768f;
				VertexInfo->V /= 32768f;
			}

			Pointer += sizeof(short) * 2;
		}

		protected void ReadTextureCoordinatesFloat()
		{
			Align4();
			VertexInfo->U = (float)((float*)Pointer)[0];
			VertexInfo->V = (float)((float*)Pointer)[1];

			Pointer += sizeof(float) * 2;
		}

		protected void ReadColor5650()
		{
			Align2();
			var Value = *((ushort*)Pointer);
			_SetVertexInfoColor(PixelFormatDecoder.Decode_RGBA_5650_Pixel(Value));
			Pointer += sizeof(ushort);
		}

		protected void ReadColor5551()
		{
			Align2();
			var Value = *((ushort*)Pointer);
			OutputPixel Color;
			PixelFormatDecoder.Decode_RGBA_5551_Pixel(Value, out Color);
			_SetVertexInfoColor(Color);
			Pointer += sizeof(ushort);
		}

		protected void ReadColor4444()
		{
			Align2();
			var Value = *((ushort*)Pointer);
			_SetVertexInfoColor(PixelFormatDecoder.Decode_RGBA_4444_Pixel(Value));
			Pointer += sizeof(ushort);
		}

		protected void ReadColor8888()
		{
			Align4();
			var Value = *((uint*)Pointer);
			_SetVertexInfoColor(PixelFormatDecoder.Decode_RGBA_8888_Pixel(Value));
			Pointer += sizeof(uint);
			//Console.WriteLine("{0}, {1}, {2}, {3}", VertexInfo->R, VertexInfo->G, VertexInfo->B, VertexInfo->A);
		}

		protected void _SetVertexInfoColor(OutputPixel Color)
		{
			VertexInfo->R = (float)(Color.R) / 255.0f;
			VertexInfo->G = (float)(Color.G) / 255.0f;
			VertexInfo->B = (float)(Color.B) / 255.0f;
			VertexInfo->A = (float)(Color.A) / 255.0f;
		}

		public void ReadPositionByte()
		{
			Align1();
			VertexInfo->PX = (float)((sbyte*)Pointer)[0];
			VertexInfo->PY = (float)((sbyte*)Pointer)[1];
			if (Transform2D)
			{
				VertexInfo->PZ = (float)((byte*)Pointer)[2];
			}
			else
			{
				VertexInfo->PZ = (float)((sbyte*)Pointer)[2];
			}

			if (!Transform2D)
			{
				VertexInfo->PX /= 128f;
				VertexInfo->PY /= 128f;
				VertexInfo->PZ /= 128f;
			}

			//Console.Error.WriteLine(VertexInfo->PZ);

			Pointer += sizeof(byte) * 3;
		}

		public void ReadPositionShort()
		{
			Align2();
			VertexInfo->PX = (float)((short*)Pointer)[0];
			VertexInfo->PY = (float)((short*)Pointer)[1];
			if (Transform2D)
			{
				VertexInfo->PZ = (float)((ushort*)Pointer)[2];
			}
			else
			{
				VertexInfo->PZ = (float)((short*)Pointer)[2];
			}
	
			if (!Transform2D)
			{
				VertexInfo->PX /= 32768f;
				VertexInfo->PY /= 32768f;
				VertexInfo->PZ /= 32768f;
			}

			//Console.Error.WriteLine(VertexInfo->PZ);

			Pointer += sizeof(short) * 3;
		}

		public void ReadPositionFloat()
		{
			Align4();
			VertexInfo->PX = (float)((float*)Pointer)[0];
			VertexInfo->PY = (float)((float*)Pointer)[1];
			VertexInfo->PZ = (float)((float*)Pointer)[2];
			Pointer += sizeof(float) * 3;
		}

		public void ReadWeightByte()
		{
			var Weights = &VertexInfo->Weight0;
			for (int n = 0; n < SkinningWeightCount; n++)
			{
				Weights[n] = (float)((sbyte*)Pointer)[n] / 128f;
			}
			Pointer += sizeof(sbyte) * SkinningWeightCount;
		}

		public void ReadWeightShort()
		{
			var Weights = &VertexInfo->Weight0;
			for (int n = 0; n < SkinningWeightCount; n++)
			{
				Weights[n] = (float)((short*)Pointer)[n] / 32768f;
			}
			Pointer += sizeof(short) * SkinningWeightCount;
		}

		public void ReadWeightFloat()
		{
			var Weights = &VertexInfo->Weight0;
			for (int n = 0; n < SkinningWeightCount; n++)
			{
				Weights[n] = (float)((float*)Pointer)[n];
			}
			Pointer += sizeof(float) * SkinningWeightCount;
		}

		public void ReadNormalByte()
		{
			Align1();
			VertexInfo->NX = (float)((byte*)Pointer)[0];
			VertexInfo->NY = (float)((byte*)Pointer)[1];
			VertexInfo->NZ = (float)((byte*)Pointer)[2];
			if (!Transform2D)
			{
				VertexInfo->NX /= 128f;
				VertexInfo->NY /= 128f;
				VertexInfo->NZ /= 128f;
			}

			Pointer += sizeof(byte) * 3;
		}

		public void ReadNormalShort()
		{
			Align2();
			VertexInfo->NX = (float)((short*)Pointer)[0];
			VertexInfo->NY = (float)((short*)Pointer)[1];
			VertexInfo->NZ = (float)((short*)Pointer)[2];
			if (!Transform2D)
			{
				VertexInfo->NX /= 32768f;
				VertexInfo->NY /= 32768f;
				VertexInfo->NZ /= 32768f;
			}
			Pointer += sizeof(short) * 3;
		}

		public void ReadNormalFloat()
		{
			Align4();
			VertexInfo->NX = (float)((float*)Pointer)[0];
			VertexInfo->NY = (float)((float*)Pointer)[1];
			VertexInfo->NZ = (float)((float*)Pointer)[2];
			Pointer += sizeof(float) * 3;
		}
	}
}
