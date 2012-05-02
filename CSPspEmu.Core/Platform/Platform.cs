using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using CSharpUtils;
using System.Diagnostics;

namespace CSPspEmu.Core
{
	unsafe public class Platform
	{
		static Logger Logger = Logger.GetLogger("Platform");

		public enum OS
		{
			Windows,
			Posix,
		}

		static public OS OperatingSystem;

		public static bool Is32Bit
		{
			get
			{
				return !Environment.Is64BitProcess;
			}
		}

		static private string _Architecture;

		static public string Architecture
		{
			get
			{
				if (_Architecture == null)
				{
					//Environment.OSVersion
					if (OperatingSystem == OS.Windows)
					{
						_Architecture = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
					}
					else
					{
						try
						{
							_Architecture = ProcessUtils.ExecuteCommand("uname", "-m");
							//_Architecture = Environment.GetEnvironmentVariable("HOSTTYPE");
							//_Architecture = Environment.GetEnvironmentVariable("MACHTYPE");
						}
						catch (Exception Exception)
						{
							Logger.Error(Exception);
							_Architecture = "Can't get arch";
						}
					}
				}
				return _Architecture;
			}
		}

		static Platform()
		{
			switch (Environment.OSVersion.Platform)
			{
				case PlatformID.Win32NT:
				case PlatformID.Win32Windows:
				case PlatformID.WinCE:
				case PlatformID.Win32S:
					OperatingSystem = OS.Windows;
					break;
				case PlatformID.MacOSX:
				case PlatformID.Unix:
					OperatingSystem = OS.Posix;
					break;
				default:
					throw (new PlatformNotSupportedException());
			}

			UnixStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		}

		public struct timespec
		{
			public long sec;
			public long usec;

			public long total_usec
			{
				get
				{
					return (long)usec + (long)sec * 1000 * 1000;
				}
			}
		}

		private class InternalUnix
		{
			[DllImport("libc")]
			internal static extern int* __errno_location();

			[DllImport("libc", EntryPoint = "mmap")]
			//internal static extern void* mmap(void* addr, uint len, uint prot, uint flags, uint off_t);
			internal static extern void* mmap(void* addr, uint len, uint prot, uint flags, int fildes, uint off);

			[DllImport("libc", EntryPoint = "munmap")]
			internal static extern int munmap(void* addr, uint len);

			[DllImport("libc", EntryPoint = "mprotect")]
			internal static extern int mprotect(void* start, ulong len, uint prot);
		}

		private class InternalWindows
		{
			[DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
			internal static extern byte* VirtualAlloc(void* lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

			[DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
			internal static extern bool VirtualFree(void* lpAddress, uint dwSize, uint dwFreeType);

			[DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
			internal static extern uint GetLastError();

			[DllImport("Kernel32.dll")]
			internal static extern IntPtr GetConsoleWindow();

			[DllImport("user32.dll")]
			internal static extern Boolean ShowWindow(IntPtr hWnd, Int32 nCmdShow);
		}

		const uint MEM_RESERVE = 0x2000;
		const uint MEM_COMMIT = 0x1000;
		const uint PAGE_READWRITE = 0x04;

		const uint MEM_DECOMMIT = 0x4000;
		const uint MEM_RELEASE = 0x8000;

		/*
		const uint MAP_ANON = 1;
		const uint MAP_ANONYMOUS = 1;
		const uint MAP_FILE = 2;
		const uint MAP_PRIVATE = 4;
		const uint MAP_SHARED = 8;
		const uint MAP_FIXED = 0x10;
		*/

		const uint PROT_NONE = 0;
		const uint PROT_READ = 1;
		const uint PROT_WRITE = 2;
		const uint PROT_EXEC = 4;

		const uint MAP_SHARED	= 0x01;		// Share changes
		const uint MAP_PRIVATE = 0x02;		// Changes are private

		// #elif defined(__i386__) || defined(__s390__) || defined(__x86_64__)

		const uint MAP_FIXED = 0x10; // Interpret addr exactly
		const uint MAP_ANONYMOUS= 	0x20	; // don't use a file
		const uint MAP_GROWSDOWN= 	0x0100	; // stack-like segment
		const uint MAP_DENYWRITE= 	0x0800	; // ETXTBSY
		const uint MAP_EXECUTABLE= 	0x1000	; // mark it as an executable
		const uint MAP_LOCKED	= 0x2000	; // pages are locked
		const uint MAP_NORESERVE = 0x4000;	  //  don't check for reservations

		// #define MAP_FIXED	0x10		/* Interpret addr exactly */
		// #define MAP_ANONYMOUS	0x20		/* don't use a file */
		// #define MAP_GROWSDOWN	0x0100		/* stack-like segment */
		// #define MAP_DENYWRITE	0x0800		/* ETXTBSY */
		// #define MAP_EXECUTABLE	0x1000		/* mark it as an executable */
		// #define MAP_LOCKED	0x2000		/* pages are locked */
		// #define MAP_NORESERVE	0x4000		/* don't check for reservations */
		// #define MS_ASYNC	1		/* sync memory asynchronously */
		// #define MS_INVALIDATE	2		/* invalidate the caches */
		// #define MS_SYNC		4		/* synchronous memory sync */
		// #define MCL_CURRENT	1		/* lock all current mappings */
		// #define MCL_FUTURE	2		/* lock all future mappings */
		// #define MADV_NORMAL	0x0		/* default page-in behavior */
		// #define MADV_RANDOM	0x1		/* page-in minimum required */
		// #define MADV_SEQUENTIAL	0x2		/* read-ahead aggressively */
		// #define MADV_WILLNEED	0x3		/* pre-fault pages */
		// #define MADV_DONTNEED	0x4		/* discard these pages */

		static public void Free(void* Address, uint Size)
		{
			switch (OperatingSystem)
			{
				case OS.Windows: InternalWindows.VirtualFree(Address, 0, MEM_RELEASE); break;
				case OS.Posix: InternalUnix.munmap(Address, Size); break;
			}
			throw (new NotImplementedException());
		}

		static public void* AllocRange(void* Address, uint Size)
		{
			switch (OperatingSystem)
			{
				case OS.Windows:
					{
						var Pointer = InternalWindows.VirtualAlloc(Address, Size, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);

						if (Pointer != null)
						{
							if ((void*)Pointer != (void*)Address)
							{
								throw (new Exception(String.Format("Not allocated the desired address! Expected {0:X}, Allocated: {1:X}", new IntPtr(Address), new IntPtr(Pointer))));
							}
							PointerUtils.Memset(Pointer, 0, (int)Size);
						}
						return Pointer;
					}
				case OS.Posix:
					{
						void* result = InternalUnix.mmap(
							Address,
							Size,
							PROT_READ | PROT_WRITE,
							MAP_PRIVATE | MAP_ANONYMOUS | MAP_FIXED,
							-1,
							0
						);
						if (result == (void*)-1) result = null;

						if (result == null)
						{
							Console.Error.WriteLine("errno: {0}", *InternalUnix.__errno_location());
						}

						if (result != Address)
						{
							Console.WriteLine("Alloc pointer mismatch! {0}, {1}", new IntPtr(result), new IntPtr(Address));
							//Console.ReadKey();
						}
						int result3 = InternalUnix.mprotect(Address, Size, PROT_READ | PROT_WRITE);
						return result;
					}
			}
			throw (new NotImplementedException());
		}

		static public DateTime UnixStart;

		static public long GetCurrentUnixMicroseconds()
		{
			return (DateTime.UtcNow - UnixStart).Ticks / (TimeSpan.TicksPerMillisecond / 1000);
		}

		private const Int32 SW_HIDE = 0;

		public static void HideConsole()
		{
			IntPtr hwnd = InternalWindows.GetConsoleWindow();
			InternalWindows.ShowWindow(hwnd, SW_HIDE);
		}
	}
}
