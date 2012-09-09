using System;
using CSPspEmu.Core.Gpu.State;

#if OPENTK
using OpenTK.Graphics.OpenGL;
#else
using MiniGL;
#endif

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
					GL.MatrixMode(MatrixMode.Projection); GL.LoadIdentity();
					GL.Ortho(0, 480, 272, 0, 0, -0xFFFF);

					GL.MatrixMode(MatrixMode.Modelview); GL.LoadIdentity();
				}
				else
				{
					GL.MatrixMode(MatrixMode.Projection); GL.LoadIdentity();
					GL.MultMatrix(GpuState->VertexState.ProjectionMatrix.Values);

					GL.MatrixMode(MatrixMode.Modelview); GL.LoadIdentity();
					GpuState->VertexState.ViewMatrix.SetLastColumn();
					GpuState->VertexState.WorldMatrix.SetLastColumn();
					GL.MultMatrix(GpuState->VertexState.ViewMatrix.Values);
					GL.MultMatrix(GpuState->VertexState.WorldMatrix.Values);

					if (GpuState->VertexState.WorldMatrix.Values[0] == float.NaN)
					{
						throw (new Exception("Invalid WorldMatrix"));
					}

					//GpuState->VertexState.ViewMatrix.Dump();
					//GpuState->VertexState.WorldMatrix.Dump();

					//Console.WriteLine("NO Transform2D");
				}
			}
		}
	}
}
