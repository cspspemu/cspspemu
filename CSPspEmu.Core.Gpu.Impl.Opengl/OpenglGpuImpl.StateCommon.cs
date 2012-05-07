using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Gpu.State;

#if OPENTK
using OpenTK.Graphics.OpenGL;
#else
using MiniGL;
#endif

namespace CSPspEmu.Core.Gpu.Impl.Opengl
{
	sealed unsafe public partial class OpenglGpuImpl
	{
		private void PrepareStateCommon(GpuStateStruct* GpuState)
		{
			var Viewport = GpuState->Viewport;
			//ViewportStruct(
			//  Position=Vector3f(X=2048,Y=2048,Z=0.9999847),
			//  Scale=Vector3f(X=480,Y=-272,Z=-32768),
			//  RegionTopLeft=PointS(X=0,Y=0),
			//  RegionBottomRight=PointS(X=479,Y=271)
			//)
			//ViewportStruct(
			//  RegionSize=PointS(X=384,Y=240),
			//  Position=Vector3f(X=2048,Y=2048,Z=0),
			//  Scale=Vector3f(X=480,Y=-272,Z=0),
			//  RegionTopLeft=PointS(X=0,Y=0),
			//  RegionBottomRight=PointS(X=383,Y=239)
			//)
			//Console.Error.WriteLine(Viewport.ToString());

			GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Fastest);
			GL.Hint(HintTarget.LineSmoothHint, HintMode.Fastest);
			GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Fastest);
			GL.Hint(HintTarget.PointSmoothHint, HintMode.Fastest);

/*
				int halfHeight = Math.abs(context.viewport_height);
				int halfWidth = Math.abs(context.viewport_width);
				int viewportX = context.viewport_cx - halfWidth - context.offset_x;
				int viewportY = context.viewport_cy - halfHeight - context.offset_y;
				int viewportWidth = 2 * halfWidth;
				int viewportHeight = 2 * halfHeight;

				// For OpenGL, translate the viewportY from the upper left corner
				// to the lower left corner.
				viewportY = Screen.height - viewportY - viewportHeight;

				re.setViewport(viewportX, viewportY, viewportWidth, viewportHeight);
*/

			int ScreenWidth = 480;
			int ScreenHeight = 272;

			int ScaledWidth = (int)(((double)ScreenWidth / (double)Viewport.RegionSize.X) * (double)ScreenWidth);
			int ScaledHeight = (int)(((double)ScreenHeight / (double)Viewport.RegionSize.Y) * (double)ScreenHeight);

			GL.Viewport(
				(int)Viewport.RegionTopLeft.X,
				(int)Viewport.RegionTopLeft.Y,
				ScaledWidth,
				ScaledHeight
			);
		}
	}
}
