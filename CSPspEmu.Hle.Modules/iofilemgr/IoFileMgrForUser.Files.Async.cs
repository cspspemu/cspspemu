using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Hle.Vfs;

namespace CSPspEmu.Hle.Modules.iofilemgr
{
	unsafe public partial class IoFileMgrForUser
	{
		/// <summary>
		/// Open or create a file for reading or writing (asynchronous)
		/// </summary>
		/// <param name="FileName">Pointer to a string holding the name of the file to open</param>
		/// <param name="Flags">Libc styled flags that are or'ed together</param>
		/// <param name="Mode">File access mode.</param>
		/// <returns>A non-negative integer is a valid fd, anything else an error</returns>
		[HlePspFunction(NID = 0x89AA9906, FirmwareVersion = 150)]
		public int sceIoOpenAsync(string FileName, HleIoFlags Flags, SceMode Mode)
		{
			var FileHandle = sceIoOpen(FileName, Flags, Mode);
			var File = HleState.HleIoManager.HleIoDrvFileArgPool.Get(FileHandle);
			File.AsyncLastResult = FileHandle;
			return FileHandle;
		}

		/// <summary>
		/// Reposition read/write file descriptor offset (asynchronous)
		/// </summary>
		/// <param name="FileHandle">Opened file descriptor with which to seek</param>
		/// <param name="Offset">Relative offset from the start position given by whence (32 bits)</param>
		/// <param name="Whence">
		///		Set to SEEK_SET to seek from the start of the file, SEEK_CUR
		///		seek from the current position and SEEK_END to seek from the end.
		/// </param>
		/// <returns>
		///		Less than 0 on error.
		///		Actual value should be passed returned by the ::sceIoWaitAsync call.
		/// </returns>
		[HlePspFunction(NID = 0x1B385D8F, FirmwareVersion = 150)]
		public int sceIoLseek32Async(int FileHandle, uint Offset, SeekAnchor Whence)
		{
			return sceIoLseekAsync(FileHandle, (long)Offset, Whence);
		}

		/// <summary>
		/// Reposition read/write file descriptor offset (asynchronous)
		/// </summary>
		/// <param name="FileHandle">Opened file descriptor with which to seek</param>
		/// <param name="Offset">Relative offset from the start position given by whence</param>
		/// <param name="Whence">
		///		Set to SEEK_SET to seek from the start of the file, SEEK_CUR
		///		seek from the current position and SEEK_END to seek from the end.
		/// </param>
		/// <returns>
		///		Less than 0 on error.
		///		Actual value should be passed returned by the ::sceIoWaitAsync call.
		/// </returns>
		[HlePspFunction(NID = 0x71B19E77, FirmwareVersion = 150)]
		public int sceIoLseekAsync(int FileHandle, long Offset, SeekAnchor Whence)
		{
			var File = HleState.HleIoManager.HleIoDrvFileArgPool.Get(FileHandle);
			File.AsyncLastResult = sceIoLseek(FileHandle, Offset, Whence);
			return 0;
		}

		/// <summary>
		/// Delete a descriptor (asynchronous)
		/// </summary>
		/// <param name="FileHandle">File descriptor to close</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0xFF5940B6, FirmwareVersion = 150)]
		public int sceIoCloseAsync(int FileHandle)
		{
			var File = HleState.HleIoManager.HleIoDrvFileArgPool.Get(FileHandle);
			File.AsyncLastResult = 0;
			sceIoClose(FileHandle);

			//HleState.HleIoManager.HleIoDrvFileArgPool.Remove(FileHandle);
			
			return 0;
		}

		/// <summary>
		/// Read input (asynchronous)
		/// </summary>
		/// <example>
		/// bytes_read = sceIoRead(fd, data, 100);
		/// </example>
		/// <param name="FileHandle">Opened file descriptor to read from</param>
		/// <param name="OutputPointer">Pointer to the buffer where the read data will be placed</param>
		/// <param name="OutputSize">Size of the read in bytes</param>
		/// <returns>
		///		Less than 0 on error.
		///	</returns>
		[HlePspFunction(NID = 0xA0B5A7C2, FirmwareVersion = 150)]
		public int sceIoReadAsync(int FileHandle, byte* OutputPointer, int OutputSize)
		{
			var File = HleState.HleIoManager.HleIoDrvFileArgPool.Get(FileHandle);
			File.AsyncLastResult = sceIoRead(FileHandle, OutputPointer, OutputSize);
			return 0;
		}

		/// <summary>
		/// Change the priority of the asynchronous thread.
		/// </summary>
		/// <param name="FileHandle">The opened fd on which the priority should be changed.</param>
		/// <param name="Priority">The priority of the thread.</param>
		/// <returns>Less than 0 on error.</returns>
		[HlePspFunction(NID = 0xB293727F, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public int sceIoChangeAsyncPriority(SceUID FileHandle, int Priority)
		{
			return 0;
			//throw(new NotImplementedException());
			/*
			unimplemented_notice();
			//return -1;
			return 0;
			*/
		}

		public int _sceIoWaitAsyncCB(CpuThreadState CpuThreadState, int FileHandle, long* Result, bool HandleCallbacks)
		{
			var File = HleState.HleIoManager.HleIoDrvFileArgPool.Get(FileHandle);
			*Result = File.AsyncLastResult;
			CpuThreadState.LO = FileHandle;
			return 0;
			/*
			logInfo("_sceIoWaitAsyncCB(fd=%d, callbacks=%d)", FileHandle, HandleCallbacks);
			FileHandle fileHandle = uniqueIdFactory.get!FileHandle(FileHandle);
			*Result = fileHandle.lastOperationResult;
			if (HandleCallbacks) {
				hleEmulatorState.callbacksHandler.executeQueued(currentThreadState);
			}
			currentRegisters.LO = FileHandle;
			return FileHandle;
			*/
		}

		/// <summary>
		/// Wait for asyncronous completion.
		/// </summary>
		/// <param name="FileHandle">The file descriptor which is current performing an asynchronous action.</param>
		/// <param name="Result">The result of the async action.</param>
		/// <returns>The given fd or a negative value on error.</returns>
		[HlePspFunction(NID = 0xE23EEC33, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceIoWaitAsync(CpuThreadState CpuThreadState, int FileHandle, long* Result)
		{
			//throw(new NotImplementedException());
			return _sceIoWaitAsyncCB(CpuThreadState, FileHandle, Result, HandleCallbacks: false);
		}

		/// <summary>
		/// Wait for asyncronous completion.
		/// </summary>
		/// <param name="FileHandle">The file descriptor which is current performing an asynchronous action.</param>
		/// <param name="Result">The result of the async action.</param>
		/// <returns>The given fd or a negative value on error.</returns>
		[HlePspFunction(NID = 0x35DBD746, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public int sceIoWaitAsyncCB(CpuThreadState CpuThreadState, int FileHandle, long* Result)
		{
			//throw (new NotImplementedException());
			return _sceIoWaitAsyncCB(CpuThreadState, FileHandle, Result, HandleCallbacks: true);
		}

		/// <summary>
		/// Poll for asyncronous completion.
		/// </summary>
		/// <param name="FileHandle">The file descriptor which is current performing an asynchronous action.</param>
		/// <param name="Result">The result of the async action.</param>
		/// <returns>
		///		Return 1 on busy.
		///		Return 0 on ready.
		/// </returns>
		[HlePspFunction(NID = 0x3251EA56, FirmwareVersion = 150)]
		public int sceIoPollAsync(int FileHandle, long* Result)
		{
			var File = HleState.HleIoManager.HleIoDrvFileArgPool.Get(FileHandle);
			*Result = File.AsyncLastResult;

			return 0;
		}

		/// <summary>
		/// Perform an ioctl on a device. (asynchronous)
		/// </summary>
		/// <param name="FileHandle">Opened file descriptor to ioctl to</param>
		/// <param name="Command">The command to send to the device</param>
		/// <param name="InputPointer">A data block to send to the device, if NULL sends no data</param>
		/// <param name="InputLength">Length of indata, if 0 sends no data</param>
		/// <param name="OutputPointer">A data block to receive the result of a command, if NULL receives no data</param>
		/// <param name="OutputLength">Length of outdata, if 0 receives no data</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0xE95A012B, FirmwareVersion = 150)]
		public int sceIoIoctlAsync(int FileHandle, uint Command, byte* InputPointer, int InputLength, byte* OutputPointer, int OutputLength)
		{
			var File = HleState.HleIoManager.HleIoDrvFileArgPool.Get(FileHandle);
			File.AsyncLastResult = sceIoIoctl(FileHandle, Command, InputPointer, InputLength, OutputPointer, OutputLength);
			return 0;
		}


		/// <summary>
		/// Write output (asynchronous)
		/// </summary>
		/// <param name="fd">Opened file descriptor to write to</param>
		/// <param name="data">Pointer to the data to write</param>
		/// <param name="size">Size of data to write</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0x0FACAB19, FirmwareVersion = 150)]
		public int sceIoWriteAsync(SceUID fd, void* data, SceSize size)
		{
			throw(new NotImplementedException());
			/*
			unimplemented();
			return -1;
			*/
		}

		/// <summary>
		/// Cancel an asynchronous operation on a file descriptor.
		/// </summary>
		/// <param name="id">The file descriptor to perform cancel on.</param>
		/// <returns>less than 0 on error.</returns>
		[HlePspFunction(NID = 0xE8BC6571, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceIoCancel(int id)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Sets a callback for the asynchronous action.
		/// </summary>
		/// <param name="id">The filedescriptor currently performing an asynchronous action.</param>
		/// <param name="cbid">The UID of the callback created with ::sceKernelCreateCallback</param>
		/// <param name="notifyArg">Pointer to an argument to pass to the callback.</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0xA12A0514, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceIoSetAsyncCallback(int id, int cbid, int notifyArg)
		{
			return 0;
		}
	}
}
