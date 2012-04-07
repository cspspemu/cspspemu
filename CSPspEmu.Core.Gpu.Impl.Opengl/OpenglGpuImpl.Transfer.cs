using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;
using CSPspEmu.Core.Gpu.State;
using CSharpUtils.Extensions;

#if OPENTK
using OpenTK.Graphics.OpenGL;
#else
using MiniGL;
#endif

namespace CSPspEmu.Core.Gpu.Impl.Opengl
{
	sealed unsafe public partial class OpenglGpuImpl
	{
		private void TransferToFrameBuffer(GpuStateStruct* GpuState)
		{
			var TextureTransferState = GpuState->TextureTransferState;

			var GlPixelFormat = GlPixelFormatList[(int)GpuState->DrawBufferState.Format];

			GL.PixelZoom(1, -1);
			GL.WindowPos2(TextureTransferState.DestinationX, 272 - TextureTransferState.DestinationY);
			//GL.PixelZoom(1, -1);
			//GL.PixelZoom(1, 1);
			GL.PixelStore(PixelStoreParameter.UnpackAlignment, TextureTransferState.BytesPerPixel);
			GL.PixelStore(PixelStoreParameter.UnpackRowLength, TextureTransferState.SourceLineWidth);
			GL.PixelStore(PixelStoreParameter.UnpackSkipPixels, TextureTransferState.SourceX);
			GL.PixelStore(PixelStoreParameter.UnpackSkipRows, TextureTransferState.SourceY);

			{
				GL.DrawPixels(
					TextureTransferState.Width,
					TextureTransferState.Height,
					PixelFormat.Rgba,
					GlPixelFormat.OpenglPixelType,
					new IntPtr(Memory.PspAddressToPointerSafe(TextureTransferState.SourceAddress))
				);
			}

			GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
			GL.PixelStore(PixelStoreParameter.UnpackRowLength, 0);
			GL.PixelStore(PixelStoreParameter.UnpackSkipPixels, 0);
			GL.PixelStore(PixelStoreParameter.UnpackSkipRows, 0);
		}

		private void TransferGeneric(GpuStateStruct* GpuState)
		{
			var TextureTransferState = GpuState->TextureTransferState;

			var SourcePointer = (byte*)Memory.PspAddressToPointer(TextureTransferState.SourceAddress.Address);
			var DestinationPointer = (byte*)Memory.PspAddressToPointer(TextureTransferState.DestinationAddress.Address);
			var SourceX = TextureTransferState.SourceX;
			var SourceY = TextureTransferState.SourceY;
			var DestinationX = TextureTransferState.DestinationX;
			var DestinationY = TextureTransferState.DestinationY;
			var BytesPerPixel = TextureTransferState.BytesPerPixel;

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
	}
}
