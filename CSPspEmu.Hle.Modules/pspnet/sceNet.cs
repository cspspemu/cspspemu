using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Hle.Attributes;
using CSharpUtils;
using CSharpUtils.Extensions;

namespace CSPspEmu.Hle.Modules.pspnet
{
	[HlePspModule(ModuleFlags = ModuleFlags.KernelMode | ModuleFlags.Flags0x00010011)]
	unsafe public partial class sceNet : HleModuleHost
	{
		/// <summary>
		/// Initialise the networking library
		/// </summary>
		/// <param name="MemoryPoolSize">Memory pool size (appears to be for the whole of the networking library).</param>
		/// <param name="calloutprio">Priority of the SceNetCallout thread.</param>
		/// <param name="calloutstack">Stack size of the SceNetCallout thread (defaults to 4096 on non 1.5 firmware regardless of what value is passed).</param>
		/// <param name="netintrprio">Priority of the SceNetNetintr thread.</param>
		/// <param name="netintrstack">Stack size of the SceNetNetintr thread (defaults to 4096 on non 1.5 firmware regardless of what value is passed).</param>
		/// <returns>
		///		0 on success
		///		less than 0 on error
		/// </returns>
		[HlePspFunction(NID = 0x39AF39A6, FirmwareVersion = 150)]
		public int sceNetInit(int MemoryPoolSize, int calloutprio, int calloutstack, int netintrprio, int netintrstack)
		{
			//throw(new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// Terminate the networking library
		/// </summary>
		/// <returns>
		///		0 on success
		/// </returns>
		[HlePspFunction(NID = 0x281928A9, FirmwareVersion = 150)]
		public int sceNetTerm()
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Free (delete) thread info/data
		/// </summary>
		/// <param name="thid">The thread id.</param>
		/// <returns>
		///		0 on success
		/// </returns>
		[HlePspFunction(NID = 0x50647530, FirmwareVersion = 150)]
		public int sceNetFreeThreadinfo(int thid)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Abort a thread
		/// </summary>
		/// <param name="thid">The thread id.</param>
		/// <returns>
		///		0 on success
		/// </returns>
		[HlePspFunction(NID = 0xAD6844c6, FirmwareVersion = 150)]
		public int sceNetThreadAbort(int thid)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Convert string to a Mac address
		/// </summary>
		/// <param name="name">The string to convert.</param>
		/// <param name="mac">Pointer to a buffer to store the result.</param>
		[HlePspFunction(NID = 0xD27961C9, FirmwareVersion = 150)]
		public void sceNetEtherStrton(string name, byte* mac)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Convert Mac address to a string
		/// </summary>
		/// <param name="MacAddress">The Mac address to convert.</param>
		/// <param name="OutputString">Pointer to a buffer to store the result.</param>
		[HlePspFunction(NID = 0x89360950, FirmwareVersion = 150)]
		public int sceNetEtherNtostr(byte* MacAddress, char* OutputString)
		{
			var Parts = new string[6];
			for (int n = 0; n < 6; n++)
			{
				Parts[n] = "%02X".Sprintf((uint)MacAddress[n]);
			}
			PointerUtils.StoreStringOnPtr(String.Join(":", Parts), Encoding.UTF8, (byte*)OutputString);
			
			return 0;
			//throw (new NotImplementedException());
		}

		/// <summary>
		/// Retrieve the local Mac address
		/// </summary>
		/// <param name="mac">Pointer to a buffer to store the result.</param>
		/// <returns>
		///		0 on success
		/// </returns>
		[HlePspFunction(NID = 0x0BF0A3AE, FirmwareVersion = 150)]
		public int sceNetGetLocalEtherAddr(byte* mac)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Retrieve the networking library memory usage
		/// </summary>
		/// <param name="stat">Pointer to a ::SceNetMallocStat type to store the result.</param>
		/// <returns>
		///		0 on success
		/// </returns>
		[HlePspFunction(NID = 0xCC393E48, FirmwareVersion = 150)]
		public int sceNetGetMallocStat(SceNetMallocStat* stat)
		{
			throw (new NotImplementedException());
		}

		//[HlePspFunction(NID = 0xF5805EFE, sceNetHtonl));
		//[HlePspFunction(NID = 0x39C1BF02, sceNetHtons));
		//[HlePspFunction(NID = 0x93C4AF7E, sceNetNtohl));
		//[HlePspFunction(NID = 0x4CE03207, sceNetNtohs));

		public struct SceNetMallocStat
		{
		}
	}
}
