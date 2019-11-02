//#define USE_VERTEX_READER_DYNAREC

using System;
using CSPspEmu.Core.Gpu.State;
using CSPspEmu.Core.Types;
using CSPspEmu.Utils.Utils;

namespace CSPspEmu.Core.Gpu.VertexReading
{
    public unsafe class VertexReader
    {
        protected int VertexAlignSize = 1;
        public int VertexSize;
        protected int SkinningWeightCount;
        protected byte* BasePointer;
        public int PointerOffset;

        protected byte* Pointer => BasePointer + PointerOffset;

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
            // ReSharper disable HeapView.DelegateAllocation
            ReadWeightsList = new Action[] {Void, ReadWeightByte, ReadWeightShort, ReadWeightFloat};
            ReadTextureCoordinatesList = new Action[]
                {Void, ReadTextureCoordinatesByte, ReadTextureCoordinatesShort, ReadTextureCoordinatesFloat};
            ReadColorList = new Action[]
                {Void, Invalid, Invalid, Invalid, ReadColor5650, ReadColor5551, ReadColor4444, ReadColor8888};
            ReadNormalList = new Action[] {Void, ReadNormalByte, ReadNormalShort, ReadNormalFloat};
            ReadPositionList = new Action[] {Void, ReadPositionByte, ReadPositionShort, ReadPositionFloat};
        }

        public void SetVertexTypeStruct(VertexTypeStruct vertexType, byte* basePointer)
        {
            VertexType = vertexType;
            Transform2D = vertexType.Transform2D;

            //Console.Error.WriteLine("SetVertexTypeStruct: " + VertexTypeStruct);
            SkinningWeightCount = vertexType.RealSkinningWeightCount;
            //Console.WriteLine(SkinningWeightCount);
            VertexSize = vertexType.GetVertexSize();
            {
                ReadWeights = ReadWeightsList[(int) vertexType.Weight];
                ReadTextureCoordinates = ReadTextureCoordinatesList[(int) vertexType.Texture];
                ReadColor = ReadColorList[(int) vertexType.Color];
                ReadNormal = ReadNormalList[(int) vertexType.Normal];
                ReadPosition = ReadPositionList[(int) vertexType.Position];

                switch (vertexType.StructAlignment)
                {
                    case 4:
                        VertexAlignment = Align4;
                        break;
                    case 2:
                        VertexAlignment = Align2;
                        break;
                    default:
                        VertexAlignment = Align1;
                        break;
                }
            }
            //public VertexTypeStruct VertexTypeStruct;
            BasePointer = basePointer;
            PointerOffset = 0;
        }

        public void ReadVertices(int index, VertexInfo* vertexInfo, int count)
        {
            //Console.WriteLine("ReadVertices: {0:X8} : {1}, {2}, {3}", new IntPtr(BasePointer), Index, Count, VertexSize);
            for (var n = 0; n < count; n++) ReadVertex(index + n, &vertexInfo[n]);
        }

        public void ReadVertex(int index, VertexInfo* vertexInfo)
        {
            PointerOffset = VertexSize * index;
            VertexInfo = vertexInfo;

            // Vertex has to be aligned to the maxium size of any component. 
            //VertexAlignment();

            ReadWeights();
            ReadTextureCoordinates();
            ReadColor();
            ReadNormal();
            ReadPosition();
        }

        public VertexInfo ReadVertex(int index)
        {
            var outVertexInfo = default(VertexInfo);
            ReadVertex(index, &outVertexInfo);
            return outVertexInfo;
        }

        protected void Align(int size)
        {
            // Fixme !
            while ((PointerOffset % size) != 0)
            {
                PointerOffset++;
            }
        }

        protected void Align1()
        {
        }

        protected void Align2() => Align(2);

        protected void Align4() => Align(4);

        protected void Void()
        {
        }

        protected void Invalid() => throw new InvalidOperationException();

        protected void ReadTextureCoordinatesByte()
        {
            Align1();

            VertexInfo->Texture.X = Pointer[0];
            VertexInfo->Texture.Y = Pointer[1];
            VertexInfo->Texture.Z = (VertexType.NormalCount > 2) ? Pointer[2] : 0.0f;

            if (!Transform2D)
            {
                VertexInfo->Texture.X *= 1.0f / 128.0f;
                VertexInfo->Texture.Y *= 1.0f / 128.0f;
                VertexInfo->Texture.Z *= 1.0f / 128.0f;
            }

            PointerOffset += sizeof(byte) * VertexType.NormalCount;
        }

        protected void ReadTextureCoordinatesShort()
        {
            Align2();
            VertexInfo->Texture.X = ((ushort*) Pointer)[0];
            VertexInfo->Texture.Y = ((ushort*) Pointer)[1];
            VertexInfo->Texture.Z = (VertexType.NormalCount > 2) ? ((ushort*) Pointer)[2] : 0.0f;

            if (!Transform2D)
            {
                VertexInfo->Texture.X *= 1.0f / 32768f;
                VertexInfo->Texture.Y *= 1.0f / 32768f;
                VertexInfo->Texture.Z *= 1.0f / 32768f;
            }

            PointerOffset += sizeof(short) * VertexType.NormalCount;
        }

        protected void ReadTextureCoordinatesFloat()
        {
            Align4();
            VertexInfo->Texture.X = ((float*) Pointer)[0];
            VertexInfo->Texture.Y = ((float*) Pointer)[1];
            VertexInfo->Texture.Z = (VertexType.NormalCount > 2) ? ((float*) Pointer)[2] : 0.0f;

            PointerOffset += sizeof(float) * VertexType.NormalCount;
        }

        protected void ReadColor5650()
        {
            Align2();
            var value = *((ushort*) Pointer);
            _SetVertexInfoColor(PixelFormatDecoder.Decode_RGBA_5650_Pixel(value));
            PointerOffset += sizeof(ushort);
        }

        protected void ReadColor5551()
        {
            Align2();
            var value = *((ushort*) Pointer);
            var color = PixelFormatDecoder.Decode_RGBA_5551_Pixel(value);
            _SetVertexInfoColor(color);
            PointerOffset += sizeof(ushort);
        }

        protected void ReadColor4444()
        {
            Align2();
            var value = *((ushort*) Pointer);
            _SetVertexInfoColor(PixelFormatDecoder.Decode_RGBA_4444_Pixel(value));
            PointerOffset += sizeof(ushort);
        }

        protected void ReadColor8888()
        {
            Align4();
            var value = *((uint*) Pointer);
            _SetVertexInfoColor(PixelFormatDecoder.Decode_RGBA_8888_Pixel(value));
            PointerOffset += sizeof(uint);
            //Console.WriteLine("{0}, {1}, {2}, {3}", VertexInfo->R, VertexInfo->G, VertexInfo->B, VertexInfo->A);
        }

        protected void _SetVertexInfoColor(OutputPixel color)
        {
            VertexInfo->Color.X = color.R / 255.0f;
            VertexInfo->Color.Y = color.G / 255.0f;
            VertexInfo->Color.Z = color.B / 255.0f;
#if true
            VertexInfo->Color.W = color.A / 255.0f;
#else
			VertexInfo->Color.W = 1.0f;
#endif
        }

        public void ReadPositionByte()
        {
            Align1();
            VertexInfo->Position.X = ((sbyte*) Pointer)[0];
            VertexInfo->Position.Y = ((sbyte*) Pointer)[1];
            VertexInfo->Position.Z = Transform2D ? Pointer[2] : (float) ((sbyte*) Pointer)[2];
            VertexInfo->Position.W = 1f;

            if (!Transform2D)
            {
                VertexInfo->Position.X *= 1.0f / 127f;
                VertexInfo->Position.Y *= 1.0f / 127f;
                VertexInfo->Position.Z *= 1.0f / 127f;
            }

            //Console.Error.WriteLine(VertexInfo->PZ);

            PointerOffset += sizeof(byte) * 3;
        }

        public void ReadPositionShort()
        {
            Align2();
            VertexInfo->Position.X = ((short*) Pointer)[0];
            VertexInfo->Position.Y = ((short*) Pointer)[1];
            VertexInfo->Position.Z = Transform2D ? ((ushort*) Pointer)[2] : (float) ((short*) Pointer)[2];
            VertexInfo->Position.W = 1f;

            if (!Transform2D)
            {
                VertexInfo->Position.X *= 1.0f / 32767f;
                VertexInfo->Position.Y *= 1.0f / 32767f;
                VertexInfo->Position.Z *= 1.0f / 32767f;
            }
            //Console.Error.WriteLine(VertexInfo->PZ);

            PointerOffset += sizeof(short) * 3;
        }

        public void ReadPositionFloat()
        {
            Align4();
            VertexInfo->Position.X = ((float*) Pointer)[0];
            VertexInfo->Position.Y = ((float*) Pointer)[1];
            VertexInfo->Position.Z = ((float*) Pointer)[2];
            VertexInfo->Position.W = 1f;
            PointerOffset += sizeof(float) * 3;
        }

        public void ReadWeightByte()
        {
            for (int n = 0; n < SkinningWeightCount; n++)
            {
                VertexInfo->Weights[n] = ((sbyte*) Pointer)[n] / 128f;
            }
            PointerOffset += sizeof(sbyte) * SkinningWeightCount;
        }

        public void ReadWeightShort()
        {
            for (int n = 0; n < SkinningWeightCount; n++)
            {
                VertexInfo->Weights[n] = ((short*) Pointer)[n] / 32768f;
            }
            PointerOffset += sizeof(short) * SkinningWeightCount;
        }

        public void ReadWeightFloat()
        {
            for (int n = 0; n < SkinningWeightCount; n++)
            {
                VertexInfo->Weights[n] = ((float*) Pointer)[n];
            }
            PointerOffset += sizeof(float) * SkinningWeightCount;
        }

        public void ReadNormalByte()
        {
            Align1();
            VertexInfo->Normal.X = Pointer[0];
            VertexInfo->Normal.Y = Pointer[1];
            VertexInfo->Normal.Z = Pointer[2];
            if (!Transform2D)
            {
                VertexInfo->Normal.X *= 1.0f / 127f;
                VertexInfo->Normal.Y *= 1.0f / 127f;
                VertexInfo->Normal.Z *= 1.0f / 127f;
            }
            PointerOffset += sizeof(byte) * 3;
        }

        public void ReadNormalShort()
        {
            Align2();
            VertexInfo->Normal.X = ((short*) Pointer)[0];
            VertexInfo->Normal.Y = ((short*) Pointer)[1];
            VertexInfo->Normal.Z = ((short*) Pointer)[2];
            if (!Transform2D)
            {
                VertexInfo->Normal.X *= 1.0f / 32767f;
                VertexInfo->Normal.Y *= 1.0f / 32767f;
                VertexInfo->Normal.Z *= 1.0f / 32767f;
            }
            PointerOffset += sizeof(short) * 3;
        }

        public void ReadNormalFloat()
        {
            Align4();
            VertexInfo->Normal.X = ((float*) Pointer)[0];
            VertexInfo->Normal.Y = ((float*) Pointer)[1];
            VertexInfo->Normal.Z = ((float*) Pointer)[2];
            PointerOffset += sizeof(float) * 3;
        }
    }
}