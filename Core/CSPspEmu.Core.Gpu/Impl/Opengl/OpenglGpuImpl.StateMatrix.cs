using System;
using CSPspEmu.Core.Gpu.State;
using CSharpPlatform.GL;
using CSharpPlatform.GL.Utils;
using CSharpPlatform;

namespace CSPspEmu.Core.Gpu.Impl.Opengl
{
	public sealed unsafe partial class OpenglGpuImpl
	{
		private void PrepareStateMatrix(GpuStateStruct* GpuState)
		{
			// DRAW BEGIN COMMON
			{
				if (GpuState->VertexState.Type.Transform2D)
				//if (true)
				{
					ModelViewProjectionMatrix = Matrix4f.Ortho(0, 480, 272, 0, 0, -0xFFFF);
				}
				else
				{
					if (float.IsNaN(GpuState->VertexState.WorldMatrix.Values[0]))
					{
						Console.Error.WriteLine("Invalid WorldMatrix");
						Console.Error.WriteLine("Projection:");
						GpuState->VertexState.ProjectionMatrix.Dump();
						Console.Error.WriteLine("View:");
						GpuState->VertexState.ViewMatrix.Dump();
						Console.Error.WriteLine("World:");
						GpuState->VertexState.WorldMatrix.Dump();
						//GpuState->VertexState.WorldMatrix.LoadIdentity();

						//throw (new Exception("Invalid WorldMatrix"));
					}

					GpuState->VertexState.ViewMatrix.SetLastColumn();
					GpuState->VertexState.WorldMatrix.SetLastColumn();

					ModelViewProjectionMatrix =
						Matrix4f.Identity
						.Multiply(GpuState->VertexState.ViewMatrix.Matrix4)
						.Multiply(GpuState->VertexState.WorldMatrix.Matrix4)
						.Multiply(GpuState->VertexState.ProjectionMatrix.Matrix4)
					;
				}
			}
		}
	}
}
