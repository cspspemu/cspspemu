using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Gpu.State;
using OpenTK.Graphics.OpenGL;

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

			int ScaledWidth = (int)(((double)480 / (double)Viewport.RegionSize.X) * (double)480);
			int ScaledHeight = (int)(((double)272 / (double)Viewport.RegionSize.Y) * (double)272);

			GL.Viewport(
				(int)Viewport.RegionTopLeft.X,
				(int)Viewport.RegionTopLeft.Y,
				ScaledWidth,
				ScaledHeight
			);
		}
	}
}
