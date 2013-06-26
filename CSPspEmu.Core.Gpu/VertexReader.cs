//#define USE_VERTEX_READER_DYNAREC

using System;
using CSPspEmu.Core.Gpu.State;
using CSPspEmu.Core.Types;
using CSPspEmu.Core.Utils;

namespace CSPspEmu.Core.Gpu
{
    public unsafe class VertexReader
	{
#if USE_VERTEX_READER_DYNAREC
		public Dictionary<uint, VertexReaderDelegate> Readers = new Dictionary<uint, VertexReaderDelegate>();
		VertexReaderDelegate CurrentReader;
		byte* BasePointer;

		public void SetVertexTypeStruct(VertexTypeStruct VertexTypeStruct, byte* BasePointer)
		{
			var Key = VertexTypeStruct.Value;
			if (!Readers.TryGetValue(Key, out CurrentReader))
			{
				this.CurrentReader = Readers[Key] = VertexReaderDynarec.GenerateMethod(VertexTypeStruct);
			}
			this.BasePointer = BasePointer;
		}

		public void ReadVertices(int Index, VertexInfo* VertexInfo, int Count)
		{
			CurrentReader(BasePointer, VertexInfo, Index, Count);
		}

		public void ReadVertex(int Index, VertexInfo* VertexInfo)
		{
			CurrentReader(BasePointer, VertexInfo, Index, 1);
		}

#else
		protected int VertexAlignSize = 1;
		protected int VertexSize;
		protected int SkinningWeightCount;
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

		protected VertexTypeStruct VertexType;

		public VertexReader()
		{
			ReadWeightsList = new Action[] { Void, ReadWeightByte, ReadWeightShort, ReadWeightFloat };
			ReadTextureCoordinatesList = new Action[] { Void, ReadTextureCoordinatesByte, ReadTextureCoordinatesShort, ReadTextureCoordinatesFloat };
			ReadColorList = new Action[] { Void, Invalid, Invalid, Invalid, ReadColor5650, ReadColor5551, ReadColor4444, ReadColor8888 };
			ReadNormalList = new Action[] { Void, ReadNormalByte, ReadNormalShort, ReadNormalFloat };
			ReadPositionList = new Action[] { Void, ReadPositionByte, ReadPositionShort, ReadPositionFloat };
		}

		public void SetVertexTypeStruct(VertexTypeStruct VertexType, byte* BasePointer)
		{
			this.VertexType = VertexType;
			Transform2D = VertexType.Transform2D;
			
			//Console.Error.WriteLine("SetVertexTypeStruct: " + VertexTypeStruct);
			SkinningWeightCount = VertexType.RealSkinningWeightCount;
			//Console.WriteLine(SkinningWeightCount);
			VertexSize = VertexType.GetVertexSize();
			{
				ReadWeights = ReadWeightsList[(int)VertexType.Weight];
				ReadTextureCoordinates = ReadTextureCoordinatesList[(int)VertexType.Texture];
				ReadColor = ReadColorList[(int)VertexType.Color];
				ReadNormal = ReadNormalList[(int)VertexType.Normal];
				ReadPosition = ReadPositionList[(int)VertexType.Position];

				switch (VertexType.StructAlignment)
				{
					case 4: VertexAlignment = Align4; break;
					case 2: VertexAlignment = Align2; break;
					default: VertexAlignment = Align1; break;
				}
			}
			//public VertexTypeStruct VertexTypeStruct;
			this.BasePointer = BasePointer;
		}

		public void ReadVertices(int Index, VertexInfo* VertexInfo, int Count)
		{
			for (int n = 0; n < Count; n++)
			{
				ReadVertex(Index + n, &VertexInfo[n]);
			}
		}

		public void ReadVertex(int Index, VertexInfo* VertexInfo)
		{
			this.Pointer = &BasePointer[VertexSize * Index];
			this.VertexInfo = VertexInfo;
			
			// Vertex has to be aligned to the maxium size of any component. 
			//VertexAlignment();
			
			ReadWeights();
			ReadTextureCoordinates();
			ReadColor();
			ReadNormal();
			ReadPosition();
		}

		public VertexInfo ReadVertex(int Index)
		{
			var OutVertexInfo = default(VertexInfo);
			ReadVertex(Index, &OutVertexInfo);
			return OutVertexInfo;
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

			VertexInfo->Texture.X = (float)((byte*)Pointer)[0];
			VertexInfo->Texture.Y = (float)((byte*)Pointer)[1];
			VertexInfo->Texture.Z = (VertexType.NormalCount > 2) ? (float)((byte*)Pointer)[2] : 0.0f;

			if (!Transform2D) VertexInfo->Texture *= 1.0f / 128.0f;

			Pointer += sizeof(byte) * VertexType.NormalCount;
		}

		protected void ReadTextureCoordinatesShort()
		{
			Align2();
			VertexInfo->Texture.X = (float)((ushort*)Pointer)[0];
			VertexInfo->Texture.Y = (float)((ushort*)Pointer)[1];
			VertexInfo->Texture.Z = (VertexType.NormalCount > 2) ? (float)((ushort*)Pointer)[2] : 0.0f;

			if (!Transform2D) VertexInfo->Texture *= 1.0f / 32768f;

			Pointer += sizeof(short) * VertexType.NormalCount;
		}

		protected void ReadTextureCoordinatesFloat()
		{
			Align4();
			VertexInfo->Texture.X = (float)((float*)Pointer)[0];
			VertexInfo->Texture.Y = (float)((float*)Pointer)[1];
			VertexInfo->Texture.Z = (VertexType.NormalCount > 2) ? (float)((float*)Pointer)[2] : 0.0f;

			Pointer += sizeof(float) * VertexType.NormalCount;
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
			VertexInfo->Color.X = (float)(Color.R) / 255.0f;
			VertexInfo->Color.Y = (float)(Color.G) / 255.0f;
			VertexInfo->Color.Z = (float)(Color.B) / 255.0f;
#if true
			VertexInfo->Color.W = (float)(Color.A) / 255.0f;
#else
			VertexInfo->Color.W = 1.0f;
#endif
		}

		public void ReadPositionByte()
		{
			Align1();
			VertexInfo->Position.X = (float)((sbyte*)Pointer)[0];
			VertexInfo->Position.Y = (float)((sbyte*)Pointer)[1];
			VertexInfo->Position.Z = Transform2D ? (float)((byte*)Pointer)[2] : (float)((sbyte*)Pointer)[2];

			if (!Transform2D) VertexInfo->Position *= 1.0f / 127f;

			//Console.Error.WriteLine(VertexInfo->PZ);

			Pointer += sizeof(byte) * 3;
		}

		public void ReadPositionShort()
		{
			Align2();
			VertexInfo->Position.X = (float)((short*)Pointer)[0];
			VertexInfo->Position.Y = (float)((short*)Pointer)[1];
			VertexInfo->Position.Z = Transform2D ? (float)((ushort*)Pointer)[2] : (float)((short*)Pointer)[2];

			if (!Transform2D) VertexInfo->Position *= 1.0f / 32767f;
			//Console.Error.WriteLine(VertexInfo->PZ);

			Pointer += sizeof(short) * 3;
		}

		public void ReadPositionFloat()
		{
			Align4();
			VertexInfo->Position.X = (float)((float*)Pointer)[0];
			VertexInfo->Position.Y = (float)((float*)Pointer)[1];
			VertexInfo->Position.Z = (float)((float*)Pointer)[2];
			Pointer += sizeof(float) * 3;
		}

		public void ReadWeightByte()
		{
			for (int n = 0; n < SkinningWeightCount; n++)
			{
				VertexInfo->Weights[n] = (float)((sbyte*)Pointer)[n] / 128f;
			}
			Pointer += sizeof(sbyte) * SkinningWeightCount;
		}

		public void ReadWeightShort()
		{
			for (int n = 0; n < SkinningWeightCount; n++)
			{
				VertexInfo->Weights[n] = (float)((short*)Pointer)[n] / 32768f;
			}
			Pointer += sizeof(short) * SkinningWeightCount;
		}

		public void ReadWeightFloat()
		{
			for (int n = 0; n < SkinningWeightCount; n++)
			{
				VertexInfo->Weights[n] = (float)((float*)Pointer)[n];
			}
			Pointer += sizeof(float) * SkinningWeightCount;
		}

		public void ReadNormalByte()
		{
			Align1();
			VertexInfo->Normal.X = (float)((byte*)Pointer)[0];
			VertexInfo->Normal.Y = (float)((byte*)Pointer)[1];
			VertexInfo->Normal.Z = (float)((byte*)Pointer)[2];
			if (!Transform2D) VertexInfo->Normal *= 1.0f / 127f;
			Pointer += sizeof(byte) * 3;
		}

		public void ReadNormalShort()
		{
			Align2();
			VertexInfo->Normal.X = (float)((short*)Pointer)[0];
			VertexInfo->Normal.Y = (float)((short*)Pointer)[1];
			VertexInfo->Normal.Z = (float)((short*)Pointer)[2];
			if (!Transform2D) VertexInfo->Normal *= 1.0f / 32767f;
			Pointer += sizeof(short) * 3;
		}

		public void ReadNormalFloat()
		{
			Align4();
			VertexInfo->Normal.X = (float)((float*)Pointer)[0];
			VertexInfo->Normal.Y = (float)((float*)Pointer)[1];
			VertexInfo->Normal.Z = (float)((float*)Pointer)[2];
			Pointer += sizeof(float) * 3;
		}
#endif
	}
}
