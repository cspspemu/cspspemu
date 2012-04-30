using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Hle.Attributes;
using CSPspEmu.Core.Rtc;
using CSPspEmu.Core;
using System.Runtime.InteropServices;

namespace CSPspEmu.Hle.Modules.utils
{
	[HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
	unsafe public class UtilsForUser : HleModuleHost
	{
		[Inject]
		PspRtc PspRtc;

		[Inject]
		CpuProcessor CpuProcessor;

		public struct TimeValStruct
		{
			public uint Seconds;
			public uint Microseconds;
		}

		public struct TimeZoneStruct
		{
			public int MinutesWest;
			public int DstTime;
		}

		public enum time_t : uint
		{
		}

		public struct clock_t
		{
			public uint Value;
		}

		protected Dictionary<uint, Random> Randoms = new Dictionary<uint, Random>();

		public UtilsForUser() : base()
		{
		}

		/// <summary>
		/// Get the current time of time and time zone information
		/// </summary>
		/// <param name="TimeVal"></param>
		/// <param name="TimeZone"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x71EC4271, FirmwareVersion = 150, SkipLog = true)]
		//[HlePspNotImplemented]
		public int sceKernelLibcGettimeofday(TimeValStruct* TimeVal, TimeZoneStruct* TimeZone)
		{
			if (TimeVal != null)
			{
				PspRtc.Update();
				ulong MicroSeconds = (ulong)(PspRtc.Elapsed.TotalMilliseconds * 1000);
				const ulong MicroSecondsInASecond = 1000 * 1000;
				TimeVal->Seconds = (uint)(MicroSeconds / MicroSecondsInASecond);
				TimeVal->Microseconds = (uint)(MicroSeconds % MicroSecondsInASecond);
				//TimeVal->Seconds = 0;
				//TimeVal->Microseconds = 0;
				//Console.Error.WriteLine("aaaaaaaaaaaaaaaaaaaaaaaaa {0}:{1}", TimeVal->Seconds, TimeVal->Microseconds);
			}
			if (TimeZone != null)
			{
			}
			//throw(new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// Write back the data cache to memory
		/// </summary>
		[HlePspFunction(NID = 0x79D1C3FA, FirmwareVersion = 150)]
		public void sceKernelDcacheWritebackAll()
		{
			CpuProcessor.sceKernelDcacheWritebackAll();
		}

		/// <summary>
		/// Invalidate a range of addresses in data cache
		/// </summary>
		/// <param name="Pointer"></param>
		/// <param name="Size"></param>
		[HlePspFunction(NID = 0xBFA98062, FirmwareVersion = 150)]
		public void sceKernelDcacheInvalidateRange(uint Pointer, uint Size)
		{
			CpuProcessor.sceKernelDcacheInvalidateRange(Pointer, Size);
		}

		/// <summary>
		/// Write back and invalidate a range of addresses in data cache
		/// </summary>
		/// <param name="Pointer"></param>
		/// <param name="Size"></param>
		[HlePspFunction(NID = 0x34B9FA9E, FirmwareVersion = 150)]
		public void sceKernelDcacheWritebackInvalidateRange(uint Pointer, uint Size)
		{
			CpuProcessor.sceKernelDcacheWritebackInvalidateRange(Pointer, Size);
		}

		/// <summary>
		/// Write back a range of addresses from data cache to memory
		/// </summary>
		/// <param name="Pointer"></param>
		/// <param name="Size"></param>
		[HlePspFunction(NID = 0xB435DEC5, FirmwareVersion = 150)]
		public void sceKernelDcacheWritebackRange(uint Pointer, uint Size)
		{
			CpuProcessor.sceKernelDcacheWritebackRange(Pointer, Size);
		}

		/// <summary>
		/// Write back and invalidate the data cache
		/// </summary>
		[HlePspFunction(NID = 0x3EE30821, FirmwareVersion = 150)]
		public void sceKernelDcacheWritebackInvalidateAll()
		{
			CpuProcessor.sceKernelDcacheWritebackInvalidateAll();
		}

		/// <summary>
		/// Function to initialise a mersenne twister context.
		/// </summary>
		/// <param name="Context">Pointer to a context</param>
		/// <param name="Seed">A seed for the random function.</param>
		/// <example>
		/// SceKernelUtilsMt19937Context ctx;
		/// sceKernelUtilsMt19937Init(&ctx, time(NULL));
		/// u32 rand_val = sceKernelUtilsMt19937UInt(&ctx);
		/// </example>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0x27CC57F0, FirmwareVersion = 150)]
		[HlePspFunction(NID = 0xE860E75E, FirmwareVersion = 150)]
		public int sceKernelUtilsMt19937Init(out SceKernelUtilsMt19937Context Context, uint Seed)
		{
			fixed (uint* State = Context.State)
			{
				State[0] = Seed;

				for (int n = 1; n < SceKernelUtilsMt19937Context.MT_N; n++)
				{
					State[n] = (uint)(1812433253 * (State[n - 1] ^ (State[n - 1] >> 30)) + n);
				}

				Context.Count = 0;

				for (int n = 0; n < SceKernelUtilsMt19937Context.MT_N; n++)
				{
					sceKernelUtilsMt19937UInt(ref Context);
				}

				return 0;
			}
		}

		/// <summary>
		/// Function to return a new psuedo random number.
		/// </summary>
		/// <param name="Context">Pointer to a pre-initialised context.</param>
		/// <returns>A pseudo random number (between 0 and MAX_INT).</returns>
		[HlePspFunction(NID = 0x06FB8A63, FirmwareVersion = 150, SkipLog = true)]
		public uint sceKernelUtilsMt19937UInt(ref SceKernelUtilsMt19937Context Context)
		{
			fixed (uint* State = Context.State)
			{
				uint* CurrentPointer;
				uint Current, Next;
				uint v0;
				bool currentPosIsLess = Context.Count < (SceKernelUtilsMt19937Context.MT_N - SceKernelUtilsMt19937Context.MT_M);
				CurrentPointer = &State[Context.Count];

				// Current Value
				Current = *CurrentPointer;

				// Next Value
				Context.Count = (Context.Count + 1) % SceKernelUtilsMt19937Context.MT_N;
				Next = State[Context.Count];

				v0 = Current ^ (Current >> 11);
				v0 ^= ((v0 << 7) & 0x9D2C5680);
				v0 ^= ((v0 << 15) & 0xEFC60000);

				var Mix = ((Current & 0x80000000) | (Next & 0x7FFFFFFF)) >> 1;

				if ((Next & 1) != 0) Current ^= 0x9908B0DF;
				Mix ^= *(
					currentPosIsLess
					? &CurrentPointer[+SceKernelUtilsMt19937Context.MT_M]
					: &CurrentPointer[-(SceKernelUtilsMt19937Context.MT_N - SceKernelUtilsMt19937Context.MT_M)]
				);
				*CurrentPointer = Mix;

				return v0 ^ (v0 >> 18);
			}
		}

		/// <summary>
		/// Get the time in seconds since the epoc (1st Jan 1970)
		/// </summary>
		/// <param name="Time"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x27CC57F0, FirmwareVersion = 150)]
		public time_t sceKernelLibcTime(time_t* Time)
		{
			PspRtc.Update();

			var CalculatedTime = (time_t)PspRtc.UnixTimeStamp;

			if (Time != null)
			{
				*Time = CalculatedTime;
			}
			return CalculatedTime;
		}

		/// <summary>
		/// Get the processor clock used since the start of the process
		/// </summary>
		/// <returns></returns>
		[HlePspFunction(NID = 0x91E4F6A7, FirmwareVersion = 150)]
		public uint sceKernelLibcClock()
		{
			PspRtc.Update();
			return (uint)(PspRtc.ElapsedTime.TotalMicroseconds);
		}

		/**
		  * Function to perform an MD5 digest of a data block.
		  *
		  * @param data - Pointer to a data block to make a digest of.
		  * @param size - Size of the data block.
		  * @param digest - Pointer to a 16byte buffer to store the resulting digest
		  *
		  * @return < 0 on error.
		  */
		[HlePspFunction(NID = 0xC8186A58, FirmwareVersion = 150)]
		public int sceKernelUtilsMd5Digest(byte* Data, uint Size, byte* Digest)
		{
			throw(new NotImplementedException());
			/*
			if (Data   == null) return -1;
			if (digest == null) return -1;
			MD5_CTX context;
			ubyte[16] _digest;
			context.start();
			context.update(Data[0..size]);
			context.finish(_digest);
			digest[0..16] = _digest;
			return 0;
			*/
		}

		/// <summary>
		/// Function to SHA1 hash a data block. 
		/// </summary>
		/// <param name="Data">The data to hash.</param>
		/// <param name="Size">The size of the data.</param>
		/// <param name="Digest">Pointer to a 20 byte array for storing the digest</param>
		/// <returns>&lt; 0 on error</returns>
		[HlePspFunction(NID = 0x840259F1, FirmwareVersion = 150)]
		public int sceKernelUtilsSha1Digest(byte* Data, uint Size, byte* Digest)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Invalidate a range of addresses in instruction cache
		/// </summary>
		/// <param name="Address"></param>
		/// <param name="Size"></param>
		[HlePspFunction(NID = 0xC2DF770E, FirmwareVersion = 150)]
		public void sceKernelIcacheInvalidateRange(CpuThreadState CpuThreadState, uint Address, uint Size)
		{
			CpuThreadState.CpuProcessor.sceKernelIcacheInvalidateRange(Address, Size);
			// Unimplemented cache.	
		}

		/// <summary>
		/// Invalidate the entire instruction cache
		/// </summary>
		[HlePspFunction(NID = 0x920F104A, FirmwareVersion = 150)]
		public void sceKernelIcacheInvalidateAll(CpuThreadState CpuThreadState)
		{
			CpuThreadState.CpuProcessor.sceKernelIcacheInvalidateAll();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x6AD345D7, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelSetGPO(int value)
		{
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[HlePspFunction(NID = 0x37FB5C42, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public int sceKernelGetGPI()
		{
			return 0;
		}

		/// <summary>
		/// Decompress gzip'd data (requires kernel mode).
		/// </summary>
		/// <param name="dest">pointer to destination buffer</param>
		/// <param name="destSize">size of destination buffer</param>
		/// <param name="src">pointer to source (compressed) data</param>
		/// <param name="unknown">unknown, pass NULL</param>
		/// <returns>size decompressed on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x78934841, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelGzipDecompress(byte* dest, uint destSize, byte * src, uint unknown)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Decompress RLZ data (requires kernel mode)
		/// </summary>
		/// <param name="dest">pointer to destination buffer</param>
		/// <param name="destSize">size of destination buffer</param>
		/// <param name="src">pointer to source (compressed) data</param>
		/// <param name="unknown">unknown, pass NULL</param>
		/// <returns>size decompressed on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x7DD07271, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelLzrcDecode(byte *dest, uint destSize, byte *src, void *unknown)
		{
			throw (new NotImplementedException());
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	unsafe public struct SceKernelUtilsMt19937Context
	{
		public const int MT_N = 624;
		public const int MT_M = 397;

		public uint Count;
		public fixed uint State[MT_N];
	}
}
