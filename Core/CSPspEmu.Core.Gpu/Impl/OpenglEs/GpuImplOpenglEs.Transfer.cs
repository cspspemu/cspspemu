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
		private void TransferToFrameBuffer(GpuStateStruct* GpuState)
		{
			var TextureTransferState = GpuState->TextureTransferState;

			var GlPixelFormat = GlPixelFormatList[(int)GpuState->DrawBufferState.Format];

			Console.Error.WriteLine("TransferToFrameBuffer not implemented");


			//GL.glReadPixels
			//GL.glPixelZoom(1, -1);
			//GL.glWindowPos2(TextureTransferState.DestinationX, 272 - TextureTransferState.DestinationY);
			////GL.PixelZoom(1, -1);
			////GL.PixelZoom(1, 1);
			//GL.glPixelStore(PixelStoreParameter.UnpackAlignment, TextureTransferState.BytesPerPixel);
			//GL.glPixelStore(PixelStoreParameter.UnpackRowLength, TextureTransferState.SourceLineWidth);
			//GL.glPixelStore(PixelStoreParameter.UnpackSkipPixels, TextureTransferState.SourceX);
			//GL.glPixelStore(PixelStoreParameter.UnpackSkipRows, TextureTransferState.SourceY);
			//
			//{
			//	GL.glDrawPixels(
			//		TextureTransferState.Width,
			//		TextureTransferState.Height,
			//		GL.GL_RGBA,
			//		GlPixelFormat.OpenglPixelType,
			//		new IntPtr(Memory.PspAddressToPointerSafe(
			//			TextureTransferState.SourceAddress,
			//			TextureTransferState.Width * TextureTransferState.Height * 4
			//		))
			//	);
			//}
			//
			//GL.glPixelStore(PixelStoreParameter.UnpackAlignment, 1);
			//GL.glPixelStore(PixelStoreParameter.UnpackRowLength, 0);
			//GL.glPixelStore(PixelStoreParameter.UnpackSkipPixels, 0);
			//GL.glPixelStore(PixelStoreParameter.UnpackSkipRows, 0);
		}

		private void TransferGeneric(GpuStateStruct* GpuState)
		{
			var TextureTransferState = GpuState->TextureTransferState;

			var SourceX = TextureTransferState.SourceX;
			var SourceY = TextureTransferState.SourceY;
			var DestinationX = TextureTransferState.DestinationX;
			var DestinationY = TextureTransferState.DestinationY;
			var BytesPerPixel = TextureTransferState.BytesPerPixel;

			var SourceTotalBytes = TextureTransferState.SourceLineWidth * TextureTransferState.Height * BytesPerPixel;
			var DestinationTotalBytes = TextureTransferState.DestinationLineWidth * TextureTransferState.Height * BytesPerPixel;

			var SourcePointer = (byte*)Memory.PspAddressToPointerSafe(TextureTransferState.SourceAddress.Address, SourceTotalBytes);
			var DestinationPointer = (byte*)Memory.PspAddressToPointerSafe(TextureTransferState.DestinationAddress.Address, DestinationTotalBytes);

			for (uint y = 0; y < TextureTransferState.Height; y++)
			{
				var RowSourceOffset = (uint)(
					(TextureTransferState.SourceLineWidth * (y + SourceY)) + SourceX
				);
				var RowDestinationOffset = (uint)(
					(TextureTransferState.DestinationLineWidth * (y + DestinationY)) + DestinationX
				);
				PointerUtils.Memcpy(
					DestinationPointer + RowDestinationOffset * BytesPerPixel,
					SourcePointer + RowSourceOffset * BytesPerPixel,
					TextureTransferState.Width * BytesPerPixel
				);
			}

			/*
			// Generic implementation.
			with (gpu.state.textureTransfer) {
				auto srcAddressHost = cast(ubyte*)gpu.memory.getPointer(srcAddress);
				auto dstAddressHost = cast(ubyte*)gpu.memory.getPointer(dstAddress);

				if (gpu.state.drawBuffer.isAnyAddressInBuffer([srcAddress, dstAddress])) {
					gpu.performBufferOp(BufferOperation.STORE, BufferType.COLOR);
				}

				for (int n = 0; n < height; n++) {
					int srcOffset = ((n + srcY) * srcLineWidth + srcX) * bpp;
					int dstOffset = ((n + dstY) * dstLineWidth + dstX) * bpp;
					(dstAddressHost + dstOffset)[0.. width * bpp] = (srcAddressHost + srcOffset)[0.. width * bpp];
					//writefln("%08X <- %08X :: [%d]", dstOffset, srcOffset, width * bpp);
				}
				//std.file.write("buffer", dstAddressHost[0..512 * 272 * 4]);
			
				if (gpu.state.drawBuffer.isAnyAddressInBuffer([dstAddress])) {
					//gpu.impl.test();
					//gpu.impl.test("trxkick");
					gpu.markBufferOp(BufferOperation.LOAD, BufferType.COLOR);
				}
				//gpu.impl.test();
			}
			*/
		}

		public override void Transfer(GpuStateStruct* GpuState)
		{
			//return;
			var TextureTransferState = GpuState->TextureTransferState;

			if (
				(TextureTransferState.DestinationAddress.Address == GpuState->DrawBufferState.Address) &&
				(TextureTransferState.DestinationLineWidth == GpuState->DrawBufferState.Width) &&
				(TextureTransferState.BytesPerPixel == GpuState->DrawBufferState.BytesPerPixel)
			)
			{
				//Console.Error.WriteLine("Writting to DrawBuffer");
				TransferToFrameBuffer(GpuState);
			}
			else
			{
				Console.Error.WriteLine("NOT Writting to DrawBuffer");
				TransferGeneric(GpuState);
				/*
				base.Transfer(GpuStateStruct);
				PrepareWrite(GpuStateStruct);
				{

				}
				PrepareRead(GpuStateStruct);
				*/
			}
			Console.Error.WriteLine("GpuImpl.Transfer Not Implemented!! : {0}", GpuState->TextureTransferState.ToStringDefault());
		}

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
