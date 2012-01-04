using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;
using CSPspEmu.Core.Memory;
using CSPspEmu.Hle.Attributes;

namespace CSPspEmu.Hle.Modules.mpeg
{
	[HlePspModule(ModuleFlags = ModuleFlags.KernelMode | ModuleFlags.Flags0x00010011)]
	unsafe public partial class sceMpeg : HleModuleHost
	{
		/// <summary>
		/// sceMpegInit
		/// </summary>
		/// <returns>0 if success.</returns>
		[HlePspFunction(NID = 0x682A619B, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegInit()
		{
			//throw (new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// sceMpegFinish
		/// </summary>
		[HlePspFunction(NID = 0x874624D6, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegFinish()
		{
			//throw (new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// sceMpegQueryMemSize
		/// </summary>
		/// <param name="Mode">Unknown, set to 0</param>
		/// <returns>
		///		Less than 0 if error else decoder data size.
		/// </returns>
		[HlePspFunction(NID = 0xC132E22F, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegQueryMemSize(int Mode)
		{
			//return sizeof(SceMpeg);
			return 0x10000;
		}

		/// <summary>
		/// sceMpegCreate
		/// </summary>
		/// <param name="Mpeg">will be filled</param>
		/// <param name="MpegData">pointer to allocated memory of size = sceMpegQueryMemSize()</param>
		/// <param name="MpegSize">size of data, should be = sceMpegQueryMemSize()</param>
		/// <param name="SceMpegRingbuffer">a ringbuffer</param>
		/// <param name="FrameWidth">display buffer width, set to 512 if writing to framebuffer</param>
		/// <param name="Mode">unknown, set to 0</param>
		/// <param name="DdrTop">unknown, set to 0</param>
		/// <returns>0 if success.</returns>
		[HlePspFunction(NID = 0xD8C5F121, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegCreate(SceMpeg* Mpeg, void* MpegData, int MpegSize, SceMpegRingbuffer* SceMpegRingbuffer, int FrameWidth, int Mode, int DdrTop)
		{
			//return -1;

			if (MpegSize < sceMpegQueryMemSize(0))
			{
				throw (new SceKernelException(SceKernelErrors.ERROR_MPEG_NO_MEMORY));
			}

			// Update the ring buffer struct.
			if (SceMpegRingbuffer->PacketSize == 0)
			{
				SceMpegRingbuffer->PacketsFree = 0;
			}
			else
			{
				SceMpegRingbuffer->PacketsFree = (int)((SceMpegRingbuffer->DataUpperBound.Address - SceMpegRingbuffer->Data.Address) / SceMpegRingbuffer->PacketSize);
			}
			SceMpegRingbuffer->SceMpeg = PspMemory.PointerToPspPointer(Mpeg);

			SceMpegData* SceMpegData = (SceMpegData*)&((byte*)MpegData)[0x30];

			Mpeg->SceMpegData = PspMemory.PointerToPspPointer(SceMpegData);

			PointerUtils.StoreStringOnPtr("LIBMPEG.001", Encoding.UTF8, SceMpegData->MagicBytes);
			SceMpegData->Unknown1 = -1;
			SceMpegData->RingBufferAddress = PspMemory.PointerToPspPointer(SceMpegRingbuffer);
			SceMpegData->RingBufferAddressDataUpper = SceMpegRingbuffer->DataUpperBound;
			SceMpegData->FrameWidth = FrameWidth;
			SceMpegData->SceMpegAvcMode.PixelFormat = Core.GuPixelFormats.RGBA_8888;
			SceMpegData->VideoFrameCount = 0;
			SceMpegData->AudioFrameCount = 0;

			SceMpegRingbuffer->Packets = 0;

			return 0;
		}

		/// <summary>
		/// sceMpegDelete
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
	    [HlePspFunction(NID = 0x606A4649, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegDelete(SceMpeg* Mpeg)
		{
			//throw(new NotImplementedException());

			return 0;
		}
	}
}
