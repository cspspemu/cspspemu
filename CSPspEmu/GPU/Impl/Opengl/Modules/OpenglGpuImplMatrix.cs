using System.Numerics;
using CSharpPlatform;
using CSPspEmu.Core.Gpu.State;

namespace CSPspEmu.Core.Gpu.Impl.Opengl.Modules
{
    internal unsafe class OpenglGpuImplMatrix
    {
        internal static void PrepareStateMatrix(GpuStateStruct gpuState, out Matrix4x4 worldViewProjectionMatrix)
        {
            // DRAW BEGIN COMMON
            {
                if (gpuState.VertexState.Type.Transform2D)
                    //if (true)
                {
                    worldViewProjectionMatrix = Matrix4x4.CreateOrthographic(512, 272, 0, -0xFFFF);
                    //WorldViewProjectionMatrix = Matrix4f.Ortho(0, 480, 272, 0, 0, -0xFFFF);
                }
                else
                {
                    worldViewProjectionMatrix =
                        gpuState.VertexState.WorldMatrix * gpuState.VertexState.ViewMatrix *
                        gpuState.VertexState.ProjectionMatrix;  
                }
            }
        }
    }
}