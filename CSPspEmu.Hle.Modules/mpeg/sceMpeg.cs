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
		public bool EnableMpeg
		{
			get
			{
				//return true;
				return HleState.PspConfig.StoredConfig.EnableMpeg;
			}
		}

		protected void CheckEnabledMpeg()
		{
			if (!EnableMpeg) throw (new SceKernelException(SceKernelErrors.ERROR_MPEG_NO_DATA));
		}

		/// <summary>
		/// sceMpegInit
		/// </summary>
		/// <returns>0 if success.</returns>
		[HlePspFunction(NID = 0x682A619B, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegInit()
		{
			CheckEnabledMpeg();
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
			CheckEnabledMpeg();

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

		public enum QueryYCbCrSizeModeEnum : int
		{
			LoadedFromFile = 1,
			LoadedFromMemory = 2,
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Mpeg"></param>
		/// <param name="Mode"></param>
		/// <param name="Width">480</param>
		/// <param name="Height">272</param>
		/// <param name="ResultAddr">Where to store the result</param>
		/// <returns></returns>
	    [HlePspFunction(NID = 0x211A057C, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegAvcQueryYCbCrSize(SceMpeg* Mpeg, QueryYCbCrSizeModeEnum Mode, int Width, int Height, int* Result)
		{
			if ((Width & 15) != 0 || (Height & 15) != 0 || Width > 480 || Height > 272)
			{
				throw(new SceKernelException(SceKernelErrors.ERROR_MPEG_INVALID_VALUE));
			}

			*Result = (Width / 2) * (Height / 2) * 6 + 128;
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Mpeg"></param>
		/// <param name="source_addr"></param>
		/// <param name="range_addr"></param>
		/// <param name="frameWidth"></param>
		/// <param name="dest_addr"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x31BD0272, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegAvcCsc(SceMpeg* Mpeg, int source_addr, int range_addr, int frameWidth, int dest_addr)
		{
			CheckEnabledMpeg();

			//throw(new NotImplementedException());
			return -1;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="mpeg"></param>
		/// <param name="mode"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="ycbcr_addr"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x67179B1B, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegAvcInitYCbCr(SceMpeg* Mpeg, int mode, int width, int height, int ycbcr_addr)
		{
			CheckEnabledMpeg();

			//throw (new NotImplementedException());
			//return -1;
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Mpeg"></param>
		/// <param name="au_addr"></param>
		/// <param name="buffer_addr"></param>
		/// <param name="init_addr"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xF0EB1125, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegAvcDecodeYCbCr(SceMpeg* Mpeg, int au_addr, int buffer_addr, int init_addr)
		{
			CheckEnabledMpeg();

			//throw (new NotImplementedException());
			return -1;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Mpeg"></param>
		/// <param name="buffer_addr"></param>
		/// <param name="status_addr"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xF2930C9C, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegAvcDecodeStopYCbCr(SceMpeg* Mpeg, int buffer_addr, int status_addr)
		{
			//throw (new NotImplementedException());
			return -1;
		}
	}
}
