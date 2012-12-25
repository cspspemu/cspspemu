using CSharpPlatform;
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
		static private Matrix4 Matrix4Ortho = Matrix4.Ortho(0, 480, 272, 0, 0, -0xFFFF);
		static private Matrix4 Matrix4Identity = Matrix4.Identity;

		private static void PrepareStateCommon(GpuStateStruct* GpuState)
		{
			var Viewport = GpuState->Viewport;

			//GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Fastest);
			//GL.Hint(HintTarget.LineSmoothHint, HintMode.Fastest);
			//GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Fastest);
			//GL.Hint(HintTarget.PointSmoothHint, HintMode.Fastest);

			int ScreenWidth = 480;
			int ScreenHeight = 272;

			int ScaledWidth = (int)(((double)ScreenWidth / (double)Viewport.RegionSize.X) * (double)ScreenWidth);
			int ScaledHeight = (int)(((double)ScreenHeight / (double)Viewport.RegionSize.Y) * (double)ScreenHeight);

			GL.glViewport(
				(int)Viewport.RegionTopLeft.X,
				(int)Viewport.RegionTopLeft.Y,
				ScaledWidth,
				ScaledHeight
			);
		}
	}
}
