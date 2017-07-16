using System;
using System.Collections.Generic;
using CSPspEmu.Core.Gpu.State;
using CSharpPlatform;

namespace CSPspEmu.Core.Gpu.Formats
{
    public unsafe class PspWavefrontObjWriter
    {
        WavefrontObjWriter WavefrontObjWriter;

        public PspWavefrontObjWriter(WavefrontObjWriter wavefrontObjWriter) => WavefrontObjWriter = wavefrontObjWriter;
        public void End() => WavefrontObjWriter.End();

        GuPrimitiveType _currentPrimitiveType;
        List<int> PrimitiveIndices = new List<int>();
        Matrix4F _modelMatrix = default(Matrix4F);
        GpuStateStruct* _gpuState;
        VertexTypeStruct _vertexType;

        public void StartPrimitive(GpuStateStruct* gpuState, GuPrimitiveType primitiveType, uint vertexAddress,
            int vertexCount, ref VertexTypeStruct vertexType)
        {
            _gpuState = gpuState;
            _vertexType = vertexType;
            var viewMatrix = gpuState->VertexState.ViewMatrix.Matrix4;
            var worldMatrix = gpuState->VertexState.WorldMatrix.Matrix4;
            _modelMatrix.Multiply(viewMatrix);
            _modelMatrix.Multiply(worldMatrix);

            _currentPrimitiveType = primitiveType;
            WavefrontObjWriter.StartComment(
                $"Start: {_currentPrimitiveType} : VertexAddress: 0x{vertexAddress:X} : {vertexCount} : {this._vertexType}");
            PrimitiveIndices.Clear();

            //throw new NotImplementedException();
            /*
            Console.WriteLine("ViewMatrix (DEMO):");
            Console.WriteLine("{0}", Matrix4.Translation(new Vector3(0f, 0f, -3.5f)));
            Console.WriteLine("ViewMatrix:");
            Console.WriteLine("{0}", ViewMatrix);
            */
        }

        public void PutVertex(ref VertexInfo vertexInfo)
        {
            //GpuState.VertexState.ViewMatrix.Matrix
            if (_gpuState->ClearingMode) return;
            var vector = vertexInfo.Position.ToVector3();
            if (!_vertexType.Transform2D)
            {
                //Vector = GLVector3.Transform(Vector, ModelMatrix);
            }

            PrimitiveIndices.Add(WavefrontObjWriter.AddVertex(vector));
            //throw new NotImplementedException();
        }

        public void EndPrimitive()
        {
            switch (_currentPrimitiveType)
            {
                case GuPrimitiveType.Sprites:
                    WavefrontObjWriter.AddFaces(4, PrimitiveIndices);
                    break;
                case GuPrimitiveType.Triangles:
                    WavefrontObjWriter.AddFaces(3, PrimitiveIndices);
                    break;
                case GuPrimitiveType.TriangleStrip:
                {
                    var indices = PrimitiveIndices.ToArray();
                    var triangleCount = indices.Length - 2;
                    for (var n = 0; n < triangleCount; n++)
                    {
                        WavefrontObjWriter.AddFace(indices[n + 0], indices[n + 1], indices[n + 2]);
                    }
                }
                    break;
                default:
                    WavefrontObjWriter.StartComment("Can't handle primitive type: " + _currentPrimitiveType);
                    break;
            }
            WavefrontObjWriter.StartComment("End: " + _currentPrimitiveType);
            //throw new NotImplementedException();
        }
    }
}