using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace CSPspEmu.Hle.Modules.iofilemgr
{
	unsafe public class IoFileMgrForUser : HleModuleHost
	{
		public enum SceMode { }

		/// <summary>
		/// Open a directory
		/// </summary>
		/// <example>
		///		int dfd;
		///		dfd = sceIoDopen("device:/");
		///		if (dfd >= 0) { Do something with the file descriptor }
		/// </example>
		/// <param name="DirectoryPath">The directory to open for reading.</param>
		/// <returns>If greater or equal to 0 then a valid file descriptor, otherwise a Sony error code.</returns>
		[HlePspFunction(NID = 0xB29DDF9C, FirmwareVersion = 150)]
		public int sceIoDopen(string DirectoryPath)
		{
			throw(new NotImplementedException());
			/*
			try {
				SceUID uid = openedDirectories.length + 1;
				openedDirectories[uid] = new DirectoryIterator(DirectoryName);
				return uid;
			} catch (Object o) {
				writefln("sceIoDopen: %s", o);
				return -1;
			}
			*/
		}

		/// <summary>
		/// Reads an entry from an opened file descriptor.
		/// </summary>
		/// <param name="FileHandle">Already opened file descriptor (using sceIoDopen)</param>
		/// <param name="dir">Pointer to an io_dirent_t structure to hold the file information</param>
		/// <returns>
		///		Read status
		///		Equal to   0 - No more directory entries left
		///		Great than 0 - More directory entired to go
		///		Less  than 0 - Error
		/// </returns>
		[HlePspFunction(NID = 0xE3EB004C, FirmwareVersion = 150)]
		public int sceIoDread(int FileHandle, /*SceIoDirent*/void *dir)
		{
			throw(new NotImplementedException());
			/*
			if (fd !in openedDirectories) return -1;
			auto cdir = openedDirectories[fd];
			uint lastLeft = cdir.left;
			if (lastLeft) {
				auto entry = cdir.extract;

				fillStats(&dir.d_stat, entry.stats);
				putStringz(dir.d_name, entry.name);
				dir.d_private = null;
				dir.dummy = 0;
				//writefln(""); writefln("sceIoDread:'%s':'%s'", entry.name, dir.d_name[0]);
			}
			return lastLeft;
			*/
		}

		/// <summary>
		/// Close an opened directory file descriptor
		/// </summary>
		/// <param name="FileHandle">Already opened file descriptor (using sceIoDopen)</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0xEB092469, FirmwareVersion = 150)]
		public int sceIoDclose(int FileHandle)
		{
			throw(new NotImplementedException());
			/*
			if (fd !in openedDirectories) return -1;
			openedDirectories.remove(fd);
			return 0;
			*/
		}

		/// <summary>
		/// Change the current directory.
		/// </summary>
		/// <param name="DirectoryPath">The path to change to.</param>
		/// <returns>less than 0 on error.</returns>
		[HlePspFunction(NID = 0x55F4717D, FirmwareVersion = 150)]
		public int sceIoChdir(string DirectoryPath)
		{
			throw (new NotImplementedException());
			/*
			try {
				fsroot.access(path);
				fscurdir = path;
				return 0;
			} catch (Object o) {
				writefln("sceIoChdir: %s", o);
				return -1;
			}
			*/
		}

		public enum EmulatorDevclEnum : int
		{
			GetHasDisplay = 1,
			SendOutput = 2,
			IsEmulator = 3,
		}

		/// <summary>
		/// Send a devctl command to a device.
		/// </summary>
		/// <example>
		///		Example: Sending a simple command to a device (not a real devctl)
		///		sceIoDevctl("ms0:", 0x200000, indata, 4, NULL, NULL); 
		/// </example>
		/// <param name="Device">String for the device to send the devctl to (e.g. "ms0:")</param>
		/// <param name="Command">The command to send to the device</param>
		/// <param name="InputPtr">A data block to send to the device, if NULL sends no data</param>
		/// <param name="InputLength">Length of indata, if 0 sends no data</param>
		/// <param name="OutputPtr">A data block to receive the result of a command, if NULL receives no data</param>
		/// <param name="OutputLength">Length of outdata, if 0 receives no data</param>
		/// <returns>0 on success, &lt; 0 on error</returns>
		[HlePspFunction(NID = 0x54F5FB11, FirmwareVersion = 150)]
		public int sceIoDevctl(string Device, int Command, byte* InputPtr, int InputLength, byte* OutputPtr, int OutputLength)
		{
			Console.WriteLine("sceIoDevctl('{0}', {1}, {2}, {3}, {4}, {5})", Device, Command, (uint)InputPtr, InputLength, (uint)OutputPtr, OutputLength);

			if (Device == "emulator:")
			{
				Console.WriteLine("     {0}", (EmulatorDevclEnum)Command);
				switch ((EmulatorDevclEnum)Command)
				{
					case EmulatorDevclEnum.GetHasDisplay:
						*((int*)OutputPtr) = HleState.CpuProcessor.PspConfig.HasDisplay ? 1 : 0;
						break;
					case EmulatorDevclEnum.SendOutput:
						Console.WriteLine("   OUTPUT:  {0}", new String((sbyte*)InputPtr, 0, InputLength, Encoding.ASCII));
						break;
					case EmulatorDevclEnum.IsEmulator:
						return 0;
				}
				return 0;
			}

			throw(new NotImplementedException());
			//return 0;
			/*
			try {
				return devices[dev].sceIoDevctl(cmd, (cast(ubyte*)indata)[0..inlen], (cast(ubyte*)outdata)[0..outlen]);
			} catch (Exception e) {
				writefln("sceIoDevctl: %s", e);
				return -1;
			}
			*/
		}

		/// <summary>
		/// Open or create a file for reading or writing
		/// </summary>
		/// <example>
		///		// Example1: Open a file for reading
		///		if (!(fd = sceIoOpen("device:/path/to/file", O_RDONLY, 0777)) {
		///			// error
		///		}
		///		
		///		// Example2: Open a file for writing, creating it if it doesnt exist
		///		if (!(fd = sceIoOpen("device:/path/to/file", O_WRONLY|O_CREAT, 0777)) {
		///			// error
		///		}
		/// </example>
		/// <param name="FileName">Pointer to a string holding the name of the file to open</param>
		/// <param name="Flags">Libc styled flags that are or'ed together</param>
		/// <param name="Mode">File access mode.</param>
		/// <returns>A non-negative integer is a valid fd, anything else an error</returns>
		[HlePspFunction(NID = 0x109F50BC, FirmwareVersion = 150)]
		public int sceIoOpen(string FileName, int Flags, SceMode Mode)
		{
			return 1;
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Write output
		/// </summary>
		/// <example>
		///		bytes_written = sceIoWrite(fd, data, 100);
		/// </example>
		/// <param name="FileHandler">Opened file descriptor to write to</param>
		/// <param name="DataPtr">Pointer to the data to write</param>
		/// <param name="DataSize">Size of data to write</param>
		/// <returns>The number of bytes written</returns>
		[HlePspFunction(NID = 0x42EC03AC, FirmwareVersion = 150)]
		public int sceIoWrite(int FileHandler, void* DataPtr, int DataSize) {
			return DataSize;
			/*
			if (fd < 0) return -1;
			if (data is null) return -1;
			auto stream = getStreamFromFD(fd);

			// Less than 256 MB.
			if (stream.position >= 256 * 1024 * 1024) {
				throw(new Exception(std.string.format("Write position over 256MB! There was a prolem with sceIoWrite: position(%d)", stream.position)));
			}

			try {
				return stream.write((cast(ubyte *)data)[0..size]);
			} catch (Object o) {
				throw(o);
				return -1;
			}
			*/
		}
	}
}
