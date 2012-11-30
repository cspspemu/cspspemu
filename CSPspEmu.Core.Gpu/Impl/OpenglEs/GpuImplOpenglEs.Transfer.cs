//#define STORE_DEPTH_BUFFER

using CSharpUtils;
using CSPspEmu.Core.Gpu.State;
using CSPspEmu.Core.Utils;
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
		private void PrepareWrite(GpuStateStruct* GpuState)
		{
			//Console.WriteLine("PrepareWrite");
			var GlPixelFormat = GlPixelFormatList[(int)GpuState->DrawBufferState.Format];
			int Width = (int)GpuState->DrawBufferState.Width;
			if (Width == 0) Width = 512;
			int Height = 272;
			int ScanWidth = PixelFormatDecoder.GetPixelsSize(GlPixelFormat.GuPixelFormat, Width);
			int PixelSize = PixelFormatDecoder.GetPixelsSize(GlPixelFormat.GuPixelFormat, 1);
			//GpuState->DrawBufferState.Format
			var DrawAddress = (void*)Memory.PspAddressToPointerSafe(GpuState->DrawBufferState.Address);
			var DepthAddress = (void*)Memory.PspAddressToPointerSafe(GpuState->DepthBufferState.Address);

			//Console.WriteLine("{0}", GlPixelFormat.GuPixelFormat);

			//Console.WriteLine("{0:X}", GpuState->DrawBufferState.Address);

			fixed (void* _TempBufferPtr = &TempBuffer[0])
			{
				var Input = (byte*)_TempBufferPtr;
				var OutputDraw = (byte*)DrawAddress;
				var OutputDepth = (byte*)DepthAddress;

#if STORE_DEPTH_BUFFER
				GL.glPixelStorei(GL.GL_PACK_ALIGNMENT, 1);
				GL.glReadPixels(0, 0, Width, Height, GL.GL_DEPTH_COMPONENT, GL.GL_UNSIGNED_SHORT, _TempBufferPtr);
				//GL.glGetError();
				for (int Row = 0; Row < Height; Row++)
				{
					var ScanIn = (byte*)&Input[sizeof(ushort) * Width * Row];
					var ScanOut = (byte*)&OutputDepth[sizeof(ushort) * Width * (Height - Row - 1)];
					//Console.WriteLine("{0}:{1},{2},{3}", Row, PixelSize, Width, ScanWidth);
					PointerUtils.Memcpy(ScanOut, ScanIn, ScanWidth);
				}
#endif

				GL.glPixelStorei(GL.GL_PACK_ALIGNMENT, PixelSize);
				GL.glReadPixels(0, 0, Width, Height, GL.GL_RGBA, GlPixelFormat.OpenglPixelType, _TempBufferPtr);

				/*
				for (int n = 0; n < 512 * 272 * 4; n++)
				{
					if (TempBuffer[n] != 0)
					{
						Console.WriteLine(TempBuffer[n]);
					}
				}
				*/

				for (int Row = 0; Row < Height; Row++)
				{
					var ScanIn = (byte*)&Input[ScanWidth * Row];
					var ScanOut = (byte*)&OutputDraw[ScanWidth * (Height - Row - 1)];
					//Console.WriteLine("{0}:{1},{2},{3}", Row, PixelSize, Width, ScanWidth);
					PointerUtils.Memcpy(ScanOut, ScanIn, ScanWidth);
				}
			}
		}
	}
}
