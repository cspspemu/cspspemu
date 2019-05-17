using CSharpPlatform;
using CSPspEmu.Core.Gpu.State;

namespace CSPspEmu.Core.Gpu.Impl.Opengl.Modules
{
    internal unsafe class OpenglGpuImplMatrix
    {
        internal static void PrepareStateMatrix(GpuStateStruct gpuState, out Matrix4F worldViewProjectionMatrix)
        {
            // DRAW BEGIN COMMON
            {
                if (gpuState.VertexState.Type.Transform2D)
                    //if (true)
                {
                    worldViewProjectionMatrix = Matrix4F.Ortho(0, 512, 272, 0, 0, -0xFFFF);
                    //WorldViewProjectionMatrix = Matrix4f.Ortho(0, 480, 272, 0, 0, -0xFFFF);
                }
                else
                {
                    if (float.IsNaN(gpuState.VertexState.WorldMatrix[0]))
                    {
                        //Console.Error.WriteLine("Invalid WorldMatrix");
                        //Console.Error.WriteLine("Projection:");
                        //GpuState->VertexState.ProjectionMatrix.Dump();
                        //Console.Error.WriteLine("View:");
                        //GpuState->VertexState.ViewMatrix.Dump();
                        //Console.Error.WriteLine("World:");
                        //GpuState->VertexState.WorldMatrix.Dump();
                    }

                    gpuState.VertexState.ViewMatrix.SetLastColumn();
                    gpuState.VertexState.WorldMatrix.SetLastColumn();

                    worldViewProjectionMatrix =
                        Matrix4F.Identity
                            .Multiply(gpuState.VertexState.WorldMatrix.Matrix4)
                            .Multiply(gpuState.VertexState.ViewMatrix.Matrix4)
                            .Multiply(gpuState.VertexState.ProjectionMatrix.Matrix4)
                        ;
                }
            }
        }
    }
}