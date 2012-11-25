using CSPspEmu.Core.Gpu.State;
using GLES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Core.Gpu.Impl.OpenglEs
{
	public unsafe partial class GpuImplOpenglEs
	{
		private void PrepareState_Common(GpuStateStruct* GpuState)
		{
			var VertexType = GpuState->VertexState.Type;
			var TextureState = &GpuState->TextureMappingState.TextureState;

			if (VertexType.Transform2D)
			{
				projectionMatrix.SetMatrix4(Matrix4.Ortho(0, 480, 272, 0, 0, -0xFFFF));
				worldMatrix.SetMatrix4(Matrix4.Identity);
				viewMatrix.SetMatrix4(Matrix4.Identity);
				textureMatrix.SetMatrix4(Matrix4.Identity.Scale(1f / 256f, 1f / 256f, 1));
				GL.glDepthRangef(0f, 1f);
			}
			else
			{
				GL.glDepthRangef(GpuState->DepthTestState.RangeNear, GpuState->DepthTestState.RangeFar);
				PrepareState_DepthTest(GpuState);
				projectionMatrix.SetMatrix4(GpuState->VertexState.ProjectionMatrix.Values);
				worldMatrix.SetMatrix4(GpuState->VertexState.WorldMatrix.Values);
				viewMatrix.SetMatrix4(GpuState->VertexState.ViewMatrix.Values);

				textureMatrix.SetMatrix4(Matrix4.Identity);
				/*
				textureMatrix.SetMatrix4(Matrix4.Identity
					.Translate(TextureState->OffsetU, TextureState->OffsetV, 0)
					.Scale(TextureState->ScaleU, TextureState->ScaleV, 1)
				);
				*/
			}
		}
	}
}
