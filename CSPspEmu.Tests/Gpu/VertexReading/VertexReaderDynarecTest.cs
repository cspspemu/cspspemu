using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSPspEmu.Core.Gpu.VertexReading;
using CSPspEmu.Core.Gpu.State;
using CSPspEmu.Core.Gpu;
using CSharpUtils;

namespace CSPspEmu.Core.Tests.Gpu.VertexReading
{
	/// <summary>
	/// Read_Weights();
	/// Read_TextureCoordinates();
	/// Read_Color();
	/// Read_Normal();
	/// Read_Position();
	/// </summary>
	[TestClass]
	unsafe public class VertexReaderDynarecTest
	{
		VertexInfo[] VertexInfoList = new VertexInfo[16];
		VertexReaderDelegate ReadVertices;

		public struct VertexType1
		{
			public float X, Y, Z;
		}

		VertexTypeStruct VertexType1Info = new VertexTypeStruct()
		{
			Position = VertexTypeStruct.NumericEnum.Float,
		};

		VertexType1[] VertexType1List = new[]
		{
			new VertexType1() { X = 1f, Y = 2f, Z = -1f },
			new VertexType1() { X = 3f, Y = 4f, Z = -2f },
			new VertexType1() { X = 5f, Y = 6f, Z = -3f },
			new VertexType1() { X = 7f, Y = 8f, Z = -4f },
		};

		[TestMethod]
		public void VertexType1_Test1()
		{
			this.ReadVertices = VertexReaderDynarec.GenerateMethod(VertexType1Info);

			fixed (void* VertexData = VertexType1List)
			fixed (VertexInfo* VertexInfoPtr = &VertexInfoList[0])
			{
				ReadVertices(VertexData, VertexInfoPtr, 0, 1);
			}
			Assert.AreEqual("FVector3d(X=1,Y=2,Z=-1)", VertexInfoList[0].Position.ToString());

			fixed (void* VertexData = VertexType1List)
			fixed (VertexInfo* VertexInfoPtr = &VertexInfoList[0])
			{
				ReadVertices(VertexData, VertexInfoPtr, 1, 1);
			}
			Assert.AreEqual("FVector3d(X=3,Y=4,Z=-2)", VertexInfoList[0].Position.ToString());
		}

		[TestMethod]
		public void VertexType1_Test2()
		{
			this.ReadVertices = VertexReaderDynarec.GenerateMethod(VertexType1Info);

			fixed (void* VertexData = VertexType1List)
			fixed (VertexInfo* VertexInfoPtr = &VertexInfoList[0])
			{
				ReadVertices(VertexData, VertexInfoPtr, 1, 3);
			}

			Assert.AreEqual("FVector3d(X=3,Y=4,Z=-2)", VertexInfoList[0].Position.ToString());
			Assert.AreEqual("FVector3d(X=5,Y=6,Z=-3)", VertexInfoList[1].Position.ToString());
			Assert.AreEqual("FVector3d(X=7,Y=8,Z=-4)", VertexInfoList[2].Position.ToString());
		}

		public struct VertexType2
		{
			public sbyte U, V;
			public sbyte NX, NY, NZ;
			public short PX, PY, PZ;
		}

		VertexTypeStruct VertexType2Info = new VertexTypeStruct()
		{
			Texture = VertexTypeStruct.NumericEnum.Byte,
			Normal = VertexTypeStruct.NumericEnum.Byte,
			Position = VertexTypeStruct.NumericEnum.Short,
			Transform2D = false,
		};

		VertexType2[] VertexType2List = new[]
		{
			new VertexType2() { U = 64, V = 32, NX = -40, NY = -80, PX = 32000, PY = 16000, PZ = -100 },
			new VertexType2() { U = 32, V = 64, NX = -80, NY = -40, PX = 16000, PY = 32000, PZ = -200 },
		};

		[TestMethod]
		public void VertexType2_Test1()
		{
			this.ReadVertices = VertexReaderDynarec.GenerateMethod(VertexType2Info);

			fixed (void* VertexData = VertexType2List)
			fixed (VertexInfo* VertexInfoPtr = &VertexInfoList[0])
			{
				ReadVertices(VertexData, VertexInfoPtr, 0, 2);
			}

			Assert.AreEqual(
				"VertexInfo(Position=(0,9765625, 0,4882813, -0,003051758), Normal=(-0,3125, -0,625, 0), UV=(0,5, 0,25), COLOR=(R:0, G:0, B:0, A:0))",
				VertexInfoList[0].ToString()
			);
			Assert.AreEqual(
				"VertexInfo(Position=(0,4882813, 0,9765625, -0,006103516), Normal=(-0,625, -0,3125, 0), UV=(0,25, 0,5), COLOR=(R:0, G:0, B:0, A:0))",
				VertexInfoList[1].ToString()
			);
		}

		public struct VertexType3
		{
			public ushort Color;
			public FVector3d Position;
		}

		VertexTypeStruct VertexType3Info = new VertexTypeStruct()
		{
			Color = VertexTypeStruct.ColorEnum.Color4444,
			Position = VertexTypeStruct.NumericEnum.Float,
			Transform2D = true,
		};

		VertexType3[] VertexType3List = new[]
		{
			new VertexType3() { Color = (ushort)ColorFormats.RGBA_4444.Encode(0xFF, 0x7F, 0x3C, 0xA0), Position = new FVector3d(1.0f, 2.0f, 3.0f) },
			new VertexType3() { Color = (ushort)ColorFormats.RGBA_4444.Encode(0x11, 0x22, 0x33, 0x44), Position = new FVector3d(4.0f, 5.0f, 6.0f) },
		};

		[TestMethod]
		public void VertexType3_Test1()
		{
			this.ReadVertices = VertexReaderDynarec.GenerateMethod(VertexType3Info);

			fixed (void* VertexData = VertexType3List)
			fixed (VertexInfo* VertexInfoPtr = &VertexInfoList[0])
			{
				ReadVertices(VertexData, VertexInfoPtr, 0, 2);
			}

			Assert.AreEqual(
				"VertexInfo(Position=(1, 2, 3), Normal=(0, 0, 0), UV=(0, 0), COLOR=(R:1, G:0,4666667, B:0,2, A:0,6))",
				VertexInfoList[0].ToString()
			);
		}
	}
}
