using System.Collections.Generic;
using System.Numerics;
using CSPspEmu.Core.Gpu.State;
using CSharpPlatform;
using CSPspEmu.Utils;

namespace CSPspEmu.Core.Gpu.Formats
{
    public unsafe class PspWavefrontObjWriter
    {
        readonly WavefrontObjWriter _wavefrontObjWriter;

        public PspWavefrontObjWriter(WavefrontObjWriter wavefrontObjWriter) => _wavefrontObjWriter = wavefrontObjWriter;
        public void End() => _wavefrontObjWriter.End();

        private GuPrimitiveType _currentPrimitiveType;
        private readonly List<int> _primitiveIndices = new List<int>();
        private Matrix4x4 _modelMatrix = Matrix4x4.Identity;
        private GpuStateStruct _gpuState;
        private VertexTypeStruct _vertexType;

        public void StartPrimitive(GpuStateStruct gpuState, GuPrimitiveType primitiveType, uint vertexAddress,
            int vertexCount, ref VertexTypeStruct vertexType)
        {
            _gpuState = gpuState;
            _vertexType = vertexType;
            var viewMatrix = gpuState.VertexState.ViewMatrix;
            var worldMatrix = gpuState.VertexState.WorldMatrix;
            _modelMatrix *= viewMatrix;
            _modelMatrix *= worldMatrix;

            _currentPrimitiveType = primitiveType;
            _wavefrontObjWriter.StartComment(
                $"Start: {_currentPrimitiveType} : VertexAddress: 0x{vertexAddress:X} : {vertexCount} : {_vertexType}");
            _primitiveIndices.Clear();

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
            if (_gpuState.ClearingMode) return;
            var vector = vertexInfo.Position.ToVector3();
            if (!_vertexType.Transform2D)
            {
                //Vector = GLVector3.Transform(Vector, ModelMatrix);
            }

            _primitiveIndices.Add(_wavefrontObjWriter.AddVertex(vector));
            //throw new NotImplementedException();
        }

        public void EndPrimitive()
        {
            switch (_currentPrimitiveType)
            {
                case GuPrimitiveType.Sprites:
                    _wavefrontObjWriter.AddFaces(4, _primitiveIndices);
                    break;
                case GuPrimitiveType.Triangles:
                    _wavefrontObjWriter.AddFaces(3, _primitiveIndices);
                    break;
                case GuPrimitiveType.TriangleStrip:
                {
                    var indices = _primitiveIndices.ToArray();
                    var triangleCount = indices.Length - 2;
                    for (var n = 0; n < triangleCount; n++)
                    {
                        _wavefrontObjWriter.AddFace(indices[n + 0], indices[n + 1], indices[n + 2]);
                    }
                }
                    break;
                default:
                    _wavefrontObjWriter.StartComment("Can't handle primitive type: " + _currentPrimitiveType);
                    break;
            }
            _wavefrontObjWriter.StartComment("End: " + _currentPrimitiveType);
            //throw new NotImplementedException();
        }
    }
}