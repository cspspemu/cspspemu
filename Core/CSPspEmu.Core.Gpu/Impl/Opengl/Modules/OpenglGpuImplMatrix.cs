using CSharpPlatform;
using CSPspEmu.Core.Gpu.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Core.Gpu.Impl.Opengl.Modules
{
    internal unsafe class OpenglGpuImplMatrix
    {
        static internal void PrepareStateMatrix(GpuStateStruct* GpuState, ref Matrix4f WorldViewProjectionMatrix)
        {
            // DRAW BEGIN COMMON
            {
                if (GpuState->VertexState.Type.Transform2D)
                    //if (true)
                {
                    WorldViewProjectionMatrix = Matrix4f.Ortho(0, 512, 272, 0, 0, -0xFFFF);
                    //WorldViewProjectionMatrix = Matrix4f.Ortho(0, 480, 272, 0, 0, -0xFFFF);
                }
                else
                {
                    if (float.IsNaN(GpuState->VertexState.WorldMatrix.Values[0]))
                    {
                        //Console.Error.WriteLine("Invalid WorldMatrix");
                        //Console.Error.WriteLine("Projection:");
                        //GpuState->VertexState.ProjectionMatrix.Dump();
                        //Console.Error.WriteLine("View:");
                        //GpuState->VertexState.ViewMatrix.Dump();
                        //Console.Error.WriteLine("World:");
                        //GpuState->VertexState.WorldMatrix.Dump();
                    }

                    GpuState->VertexState.ViewMatrix.SetLastColumn();
                    GpuState->VertexState.WorldMatrix.SetLastColumn();

                    WorldViewProjectionMatrix =
                        Matrix4f.Identity
                            .Multiply(GpuState->VertexState.WorldMatrix.Matrix4)
                            .Multiply(GpuState->VertexState.ViewMatrix.Matrix4)
                            .Multiply(GpuState->VertexState.ProjectionMatrix.Matrix4)
                        ;
                }
            }
        }
    }
}