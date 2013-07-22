using System.Text;
using CSharpUtils;
using CSPspEmu.Hle.Attributes;
using CSPspEmu.Core.Types;
using System;

namespace CSPspEmu.Hle.Modules.mpeg
{
	[HlePspModule(ModuleFlags = ModuleFlags.KernelMode | ModuleFlags.Flags0x00010011)]
	public unsafe partial class sceMpeg : HleModuleHost
	{
		[Inject]
		HleConfig HleConfig;

		private class MpegAu
		{
		}

		private class Mpeg
		{
			public SceMpegPointer* _Mpeg;
			public SceMpeg* Data;
			public MpegAu AvcAu;
			public MpegAu AtracAu;

			public void Delete()
			{
			}
		}

		private Mpeg SingleInstance = new Mpeg();

		private Mpeg GetMpeg(SceMpegPointer* Mpeg)
		{
			return SingleInstance;
		}

		/// <summary>
		/// sceMpegInit
		/// </summary>
		/// <returns>0 if success.</returns>
		[HlePspFunction(NID = 0x682A619B, FirmwareVersion = 150)]
		public int sceMpegInit()
		{
			//throw (new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// sceMpegFinish
		/// </summary>
		[HlePspFunction(NID = 0x874624D6, FirmwareVersion = 150)]
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
		public int sceMpegQueryMemSize(int Mode)
		{
			return sizeof(SceMpeg);
		}

		/// <summary>
		/// sceMpegCreate
		/// </summary>
		/// <param name="Mpeg">Will be filled</param>
		/// <param name="MpegData">Pointer to allocated memory of size = sceMpegQueryMemSize()</param>
		/// <param name="MpegSize">Size of data, should be = sceMpegQueryMemSize()</param>
		/// <param name="SceMpegRingbuffer">A ringbuffer</param>
		/// <param name="FrameWidth">Display buffer width, set to 512 if writing to framebuffer</param>
		/// <param name="Mode">Unknown, set to 0</param>
		/// <param name="DdrTop">Unknown, set to 0</param>
		/// <returns>0 if successful.</returns>
		[HlePspFunction(NID = 0xD8C5F121, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegCreate(SceMpegPointer* Mpeg, void* MpegData, int MpegSize, SceMpegRingbuffer* SceMpegRingbuffer, int FrameWidth, int Mode, int DdrTop)
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
			SceMpegRingbuffer->SceMpeg = Memory.PointerToPspPointer(Mpeg);

			SceMpeg* SceMpegData = (SceMpeg*)&((byte*)MpegData)[0x30];

			Mpeg->SceMpeg = Memory.PointerToPspPointer(SceMpegData);

			PointerUtils.StoreStringOnPtr("LIBMPEG.001", Encoding.UTF8, SceMpegData->MagicBytes);
			SceMpegData->Unknown1 = -1;
			SceMpegData->RingBufferAddress = Memory.PointerToPspPointer(SceMpegRingbuffer);
			SceMpegData->RingBufferAddressDataUpper = SceMpegRingbuffer->DataUpperBound;
			SceMpegData->FrameWidth = FrameWidth;
			SceMpegData->SceMpegAvcMode.Mode = -1;
			SceMpegData->SceMpegAvcMode.PixelFormat = GuPixelFormats.RGBA_8888;
			SceMpegData->VideoFrameCount = 0;
			SceMpegData->AudioFrameCount = 0;

			SceMpegRingbuffer->Packets = 0;

			GetMpeg(Mpeg)._Mpeg = Mpeg;
			GetMpeg(Mpeg).Data = SceMpegData;

			return 0;
		}

		/// <summary>
		/// sceMpegDelete
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
	    [HlePspFunction(NID = 0x606A4649, FirmwareVersion = 150)]
		public int sceMpegDelete(SceMpegPointer* Mpeg)
		{
			GetMpeg(Mpeg).Delete();

			return 0;
		}

		public enum QueryYCbCrSizeModeEnum : int
		{
			LoadedFromFile = 1,
			LoadedFromMemory = 2,
		}
	}
}
