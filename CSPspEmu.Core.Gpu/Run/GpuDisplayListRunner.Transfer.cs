using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Gpu.State;

namespace CSPspEmu.Core.Gpu.Run
{
	unsafe sealed public partial class GpuDisplayListRunner
	{
		/**
		 * Image transfer using the GE
		 *
		 * @note Data must be aligned to 1 quad word (16 bytes)
		 *
		 * @par Example: Copy a fullscreen 32-bit image from RAM to VRAM
		 *
		 * <code>
		 *     sceGuCopyImage(GU_PSM_8888,0,0,480,272,512,pixels,0,0,512,(void*)(((unsigned int)framebuffer)+0x4000000));
		 * </code>
		 *
		 * @param psm    - Pixel format for buffer
		 * @param sx     - Source X
		 * @param sy     - Source Y
		 * @param width  - Image width
		 * @param height - Image height
		 * @param srcw   - Source buffer width (block aligned)
		 * @param src    - Source pointer
		 * @param dx     - Destination X
		 * @param dy     - Destination Y
		 * @param destw  - Destination buffer width (block aligned)
		 * @param dest   - Destination pointer
		 **/
		// void sceGuCopyImage(int psm, int sx, int sy, int width, int height, int srcw, void* src, int dx, int dy, int destw, void* dest);
		// sendCommandi(178/*OP_TRXSBP*/,((unsigned int)src) & 0xffffff);
		// sendCommandi(179/*OP_TRXSBW*/,((((unsigned int)src) & 0xff000000) >> 8)|srcw);
		// sendCommandi(235/*OP_TRXSPOS*/,(sy << 10)|sx);
		// sendCommandi(180/*OP_TRXDBP*/,((unsigned int)dest) & 0xffffff);
		// sendCommandi(181/*OP_TRXDBW*/,((((unsigned int)dest) & 0xff000000) >> 8)|destw);
		// sendCommandi(236/*OP_TRXDPOS*/,(dy << 10)|dx);
		// sendCommandi(238/*OP_TRXSIZE*/,((height-1) << 10)|(width-1));
		// sendCommandi(234/*OP_TRXKICK*/,(psm ^ 0x03) ? 0 : 1);

		/*struct TextureTransfer {
			uint srcAddress, dstAddress;
			ushort srcLineWidth, dstLineWidth;
			ushort srcX, srcY, dstX, dstY;
			ushort width, height;
		}*/

		/// <summary>
		/// TRansfer X Source Buffer Pointer
		/// </summary>
		public void OP_TRXSBP()
		{
			GpuState->TextureTransferState.SourceAddress.Low24 = Params24;
		}

		/// <summary>
		/// TRansfer X Source Width
		/// </summary>
		public void OP_TRXSBW()
		{
			GpuState->TextureTransferState.SourceAddress.High8 = Params24 << 8;
			GpuState->TextureTransferState.SourceLineWidth = (ushort)Extract(0, 16);
			GpuState->TextureTransferState.SourceX = 0;
			GpuState->TextureTransferState.SourceY = 0;
		}

		/// <summary>
		/// TRansfer X Source POSition
		/// </summary>
		public void OP_TRXSPOS()
		{
			GpuState->TextureTransferState.SourceX = (ushort)Extract(10 * 0, 10);
			GpuState->TextureTransferState.SourceY = (ushort)Extract(10 * 1, 10);
		}

		/// <summary>
		/// // TRansfer X Destination Buffer Pointer
		/// </summary>
		public void OP_TRXDBP()
		{
			GpuState->TextureTransferState.DestinationAddress.Low24 = Params24;
		}

		/// <summary>
		/// TRansfer X Destination Width
		/// </summary>
		public void OP_TRXDBW()
		{
			GpuState->TextureTransferState.DestinationAddress.High8 = Params24 << 8;
			GpuState->TextureTransferState.DestinationLineWidth = (ushort)Extract(0, 16);
			GpuState->TextureTransferState.DestinationX = 0;
			GpuState->TextureTransferState.DestinationY = 0;
		}

		/// <summary>
		/// TRansfer X Destination POSition
		/// </summary>
		public void OP_TRXDPOS()
		{
			GpuState->TextureTransferState.DestinationX = (ushort)Extract(10 * 0, 10);
			GpuState->TextureTransferState.DestinationY = (ushort)Extract(10 * 1, 10);
		}

		/// <summary>
		/// TRansfer X SIZE
		/// </summary>
		public void OP_TRXSIZE()
		{
			GpuState->TextureTransferState.Width = (ushort)(Extract(10 * 0, 10) + 1);
			GpuState->TextureTransferState.Height = (ushort)(Extract(10 * 1, 10) + 1);
		}

		/// <summary>
		/// TRansfer X KICKTRansfer X KICK
		/// </summary>
		public void OP_TRXKICK()
		{
			GpuState->TextureTransferState.TexelSize = (TextureTransferStateStruct.TexelSizeEnum)Extract(0, 1);
			GpuDisplayList.GpuProcessor.GpuImpl.Transfer(GpuDisplayList.GpuStateStructPointer);
		}
	}
}
