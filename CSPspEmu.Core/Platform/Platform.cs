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
		public enum OS
		{
			Windows,
			Posix,
		}

		static public OS OperatingSystem;


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
					throw(new NotImplementedException());
			}
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
			[DllImport("libc", EntryPoint = "mmap")]
			internal static extern void* mmap(void* addr, uint len, uint prot, uint flags, uint off_t);

			[DllImport("libc", EntryPoint = "mprotect")]
			internal static extern int mprotect(void* start, ulong len, uint prot);

			[DllImport("librt", EntryPoint = "clock_getres")]
			internal static extern int clock_getres(int clock_id, out timespec timespec);

			[DllImport("librt", EntryPoint = "clock_gettime")]
			internal static extern int clock_gettime(int clock_id, out timespec timespec);

			static public long GetCurrentMicroseconds()
			{
				long Counter;
				long Frequency;

				timespec timespec;

				timespec = default(timespec);
				InternalUnix.clock_gettime(0, out timespec);
				Counter = timespec.total_usec;

				timespec = default(timespec);
				InternalUnix.clock_getres(0, out timespec);
				Frequency = timespec.total_usec;

				return Counter * 1000 * 1000 / Frequency;
			}
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
			internal static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

			[DllImport("Kernel32.dll")]
			internal static extern bool QueryPerformanceFrequency(out long lpFrequency);

			[DllImport("Kernel32.dll")]
			internal static extern IntPtr GetConsoleWindow();

			[DllImport("user32.dll")]
			internal static extern Boolean ShowWindow(IntPtr hWnd, Int32 nCmdShow);

			static internal long GetCurrentMicroseconds()
			{
				long Counter;
				long Frequency;

				QueryPerformanceCounter(out Counter);
				QueryPerformanceFrequency(out Frequency);

				return Counter * 1000 * 1000 / Frequency;
			}
		}

		const uint MEM_RESERVE = 0x2000;
		const uint MEM_COMMIT = 0x1000;
		const uint PAGE_READWRITE = 0x04;

		const uint MEM_DECOMMIT = 0x4000;
		const uint MEM_RELEASE = 0x8000;

		const uint MAP_ANON = 1;
		const uint MAP_ANONYMOUS = 1;
		const uint MAP_FILE = 2;
		const uint MAP_PRIVATE = 4;
		const uint MAP_SHARED = 8;
		const uint PROT_NONE = 0;
		const uint PROT_READ = 1;
		const uint PROT_WRITE = 2;
		const uint PROT_EXEC = 4;

		static public void* AllocRange(void* Address, uint Size)
		{
			switch (OperatingSystem)
			{
				case OS.Windows:
					{
						var Pointer = InternalWindows.VirtualAlloc(Address, Size, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);

						if ((void*)Pointer != (void*)Address) throw (new Exception("Not allocated the desired address!"));
						PointerUtils.Memset(Pointer, 0, (int)Size);
						return Pointer;
					}
				case OS.Posix:
					{
						void* result = InternalUnix.mmap(Address, Size, PROT_NONE, MAP_PRIVATE | MAP_ANON, 0);
						int result3 = InternalUnix.mprotect(Address, Size, PROT_READ | PROT_WRITE);
						return result;
					}
			}
			throw (new NotImplementedException());
		}

		static public long GetCurrentMicroseconds()
		{
			if (OperatingSystem == OS.Windows)
			{
				return InternalWindows.GetCurrentMicroseconds();
			}
			else
			{
				return InternalUnix.GetCurrentMicroseconds();
			}
		}

		private const Int32 SW_HIDE = 0;

		public static void HideConsole()
		{
			IntPtr hwnd = InternalWindows.GetConsoleWindow();
			InternalWindows.ShowWindow(hwnd, SW_HIDE);
		}
	}
}
