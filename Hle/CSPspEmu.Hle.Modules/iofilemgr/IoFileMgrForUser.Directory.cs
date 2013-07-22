using System;
using System.IO;
using CSPspEmu.Hle.Vfs;

namespace CSPspEmu.Hle.Modules.iofilemgr
{
	public unsafe partial class IoFileMgrForUser
	{
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
		public SceUID sceIoDopen(string DirectoryPath)
		{
			try
			{
				var Info = HleIoManager.ParsePath(DirectoryPath);
				Info.HleIoDrvFileArg.HleIoDriver.IoDopen(Info.HleIoDrvFileArg, Info.LocalPath);
				return HleIoManager.HleIoDrvFileArgPool.Create(Info.HleIoDrvFileArg);
			}
			catch (DirectoryNotFoundException)
			{
			}
			catch (FileNotFoundException)
			{
			}
			catch (InvalidOperationException InvalidOperationException)
			{
				Console.Error.WriteLine(InvalidOperationException);
			}
			finally
			{
				_DelayIo(IoDelayType.Dopen);
			}
			throw (new SceKernelException(SceKernelErrors.ERROR_ERRNO_NOT_A_DIRECTORY));
		}

		/// <summary>
		/// Reads an entry from an opened file descriptor.
		/// </summary>
		/// <param name="FileId">Already opened file descriptor (using sceIoDopen)</param>
		/// <param name="IoDirent">Pointer to an io_dirent_t structure to hold the file information</param>
		/// <returns>
		///		Read status
		///		Equal to   0 - No more directory entries left
		///		Great than 0 - More directory entired to go
		///		Less  than 0 - Error
		/// </returns>
		[HlePspFunction(NID = 0xE3EB004C, FirmwareVersion = 150)]
		public int sceIoDread(SceUID FileId, HleIoDirent* IoDirent)
		{
			try
			{
				var HleIoDrvFileArg = GetFileArgFromHandle(FileId);
				return HleIoDrvFileArg.HleIoDriver.IoDread(HleIoDrvFileArg, IoDirent);
			}
			catch (Exception Exception)
			{
				Console.Error.WriteLine(Exception);
				return -1;
			}
			finally
			{
				_DelayIo(IoDelayType.Dread);
			}
		}

		/// <summary>
		/// Close an opened directory file descriptor
		/// </summary>
		/// <param name="FileId">Already opened file descriptor (using sceIoDopen)</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0xEB092469, FirmwareVersion = 150)]
		public int sceIoDclose(SceUID FileId)
		{
			var HleIoDrvFileArg = GetFileArgFromHandle(FileId);
			try
			{
				return HleIoDrvFileArg.HleIoDriver.IoDclose(HleIoDrvFileArg);
			}
			finally
			{
				_DelayIo(IoDelayType.Dclose);
			}
		}

		/// <summary>
		/// Change the current directory.
		/// </summary>
		/// <param name="DirectoryPath">The path to change to.</param>
		/// <returns>less than 0 on error.</returns>
		[HlePspFunction(NID = 0x55F4717D, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public int sceIoChdir(string DirectoryPath)
		{
			try
			{
				HleIoManager.Chdir(DirectoryPath);
				//var Info = HleIoManager.ParsePath(DirectoryPath);
				//return Info.HleIoDriver.IoChdir(Info.HleIoDrvFileArg, Info.LocalPath);
				return 0;
			}
			finally
			{
				_DelayIo(IoDelayType.Chdir);
			}
		}

		/// <summary>
		/// Make a directory file
		/// </summary>
		/// <param name="DirectoryPath"></param>
		/// <param name="AccessMode">Access mode.</param>
		/// <returns>Returns the value 0 if its succesful otherwise -1</returns>
		[HlePspFunction(NID = 0x06A70004, FirmwareVersion = 150)]
		public int sceIoMkdir(string DirectoryPath, SceMode AccessMode)
		{
			try
			{
				var Info = HleIoManager.ParsePath(DirectoryPath);
				Info.HleIoDrvFileArg.HleIoDriver.IoMkdir(Info.HleIoDrvFileArg, Info.LocalPath, AccessMode);
				return 0;
			}
			finally
			{
				_DelayIo(IoDelayType.Mkdir);
			}
		}

		/// <summary>
		/// Remove a directory file
		/// </summary>
		/// <param name="DirectoryPath">Removes a directory file pointed by the string path</param>
		/// <returns>Returns the value 0 if its succesful otherwise -1</returns>
		[HlePspFunction(NID = 0x1117C65F, FirmwareVersion = 150)]
		public int sceIoRmdir(string DirectoryPath)
		{
			try
			{
				var Info = HleIoManager.ParsePath(DirectoryPath);
				Info.HleIoDrvFileArg.HleIoDriver.IoRmdir(Info.HleIoDrvFileArg, Info.LocalPath);
				return 0;
			}
			finally
			{
				_DelayIo(IoDelayType.Rmdir);
			}
		}
	}
}
