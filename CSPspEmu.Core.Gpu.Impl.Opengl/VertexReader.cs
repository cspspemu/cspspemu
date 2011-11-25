using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Gpu.State;

namespace CSPspEmu.Core.Gpu.Impl.Opengl
{
	public struct VertexInfo
	{
		public byte R, G, B, A;
		public float PX, PY, PZ;
		public float NX, NY, NZ;
		public float U, V;

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

		public VertexReader()
		{
			ReadWeightsList = new Action[] { Void, ReadWeightByte, ReadWeightShort, ReadWeightFloat };
			ReadTextureCoordinatesList = new Action[] { Void, ReadTextureCoordinatesByte, ReadTextureCoordinatesShort, ReadTextureCoordinatesFloat };
			ReadColorList = new Action[] { Void, Invalid, Invalid, Invalid, ReadColor5650, ReadColor5651, ReadColor4444, ReadColor8888 };
			ReadNormalList = new Action[] { Void, ReadNormalByte, ReadNormalShort, ReadNormalFloat };
			ReadPositionList = new Action[] { Void, ReadPositionByte, ReadPositionShort, ReadPositionFloat };
		}

		public void SetVertexTypeStruct(VertexTypeStruct VertexTypeStruct, byte* BasePointer)
		{
			VertexSize = VertexTypeStruct.GetVertexSize();
			{
				ReadWeights = ReadWeightsList[(int)VertexTypeStruct.Weight];
				ReadTextureCoordinates = ReadTextureCoordinatesList[(int)VertexTypeStruct.Texture];
				ReadColor = ReadColorList[(int)VertexTypeStruct.Color];
				ReadNormal = ReadNormalList[(int)VertexTypeStruct.Normal];
				ReadPosition = ReadPositionList[(int)VertexTypeStruct.Position];
				switch (VertexAlignSize)
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

		protected void Align1() { }
		protected void Align2() { Pointer = (byte*)((uint)Pointer & unchecked((uint)~1)); }
		protected void Align4() { Pointer = (byte*)((uint)Pointer & unchecked((uint)~3)); }

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
			VertexInfo[0].U = (float)((byte*)Pointer)[0];
			VertexInfo[0].V = (float)((byte*)Pointer)[1];
			Pointer += sizeof(byte) * 2;
		}

		protected void ReadTextureCoordinatesShort()
		{
			Align2();
			VertexInfo[0].U = (float)((short*)Pointer)[0];
			VertexInfo[0].V = (float)((short*)Pointer)[1];
			Pointer += sizeof(short) * 2;
		}

		protected void ReadTextureCoordinatesFloat()
		{
			Align4();
			VertexInfo[0].U = (float)((float*)Pointer)[0];
			VertexInfo[0].V = (float)((float*)Pointer)[1];
			Pointer += sizeof(float) * 2;
		}

		protected void ReadColor5650()
		{
			throw (new NotImplementedException());
		}

		protected void ReadColor5651()
		{
			throw (new NotImplementedException());
		}

		protected void ReadColor4444()
		{
			throw (new NotImplementedException());
		}

		protected void ReadColor8888()
		{
			Align4();
			uint Value = *((uint*)Pointer);
			VertexInfo[0].R = (byte)((Value >> 0) & 0xFF);
			VertexInfo[0].G = (byte)((Value >> 8) & 0xFF);
			VertexInfo[0].B = (byte)((Value >> 16) & 0xFF);
			VertexInfo[0].A = (byte)((Value >> 24) & 0xFF);
			Pointer += sizeof(byte) * 4;
			//Console.WriteLine("{0}, {1}, {2}, {3}", VertexInfo[0].R, VertexInfo[0].G, VertexInfo[0].B, VertexInfo[0].A);
		}

		public void ReadPositionByte()
		{
			Align1();
			VertexInfo[0].PX = (float)((byte*)Pointer)[0];
			VertexInfo[0].PY = (float)((byte*)Pointer)[1];
			VertexInfo[0].PZ = (float)((byte*)Pointer)[2];
			Pointer += sizeof(byte) * 3;
		}

		public void ReadPositionShort()
		{
			Align2();
			VertexInfo[0].PX = (float)((short*)Pointer)[0];
			VertexInfo[0].PY = (float)((short*)Pointer)[1];
			VertexInfo[0].PZ = (float)((short*)Pointer)[2];
			Pointer += sizeof(short) * 3;
		}

		public void ReadPositionFloat()
		{
			Align4();
			VertexInfo[0].PX = (float)((float*)Pointer)[0];
			VertexInfo[0].PY = (float)((float*)Pointer)[1];
			VertexInfo[0].PZ = (float)((float*)Pointer)[2];
			Pointer += sizeof(float) * 3;
		}

		public void ReadWeightByte()
		{
			throw(new NotImplementedException());
		}

		public void ReadWeightShort()
		{
			throw (new NotImplementedException());
		}

		public void ReadWeightFloat()
		{
			throw (new NotImplementedException());
		}

		public void ReadNormalByte()
		{
			Align1();
			VertexInfo[0].NX = (float)((byte*)Pointer)[0];
			VertexInfo[0].NY = (float)((byte*)Pointer)[1];
			VertexInfo[0].NZ = (float)((byte*)Pointer)[2];
			Pointer += sizeof(byte) * 3;
		}

		public void ReadNormalShort()
		{
			Align2();
			VertexInfo[0].NX = (float)((short*)Pointer)[0];
			VertexInfo[0].NY = (float)((short*)Pointer)[1];
			VertexInfo[0].NZ = (float)((short*)Pointer)[2];
			Pointer += sizeof(short) * 3;
		}

		public void ReadNormalFloat()
		{
			Align4();
			VertexInfo[0].NX = (float)((float*)Pointer)[0];
			VertexInfo[0].NY = (float)((float*)Pointer)[1];
			VertexInfo[0].NZ = (float)((float*)Pointer)[2];
			Pointer += sizeof(float) * 3;
		}
	}
}
