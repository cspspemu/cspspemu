using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Dokan;

namespace CSharpUtils.VirtualFileSystem
{
	public class FileSystemProxyDokanOperations : DokanOperations
	{
		FileSystem FileSystem;

		public FileSystemProxyDokanOperations(FileSystem FileSystem)
		{
			this.FileSystem = FileSystem;
		}

		static void NotImplemented(string extra = "<unknown>", object info = null)
		{
			StackTrace stackTrace = new StackTrace();           // get call stack
			StackFrame[] stackFrames = stackTrace.GetFrames();  // get method calls (frames)
			Console.WriteLine("Not Implemented : " + stackFrames[1].GetMethod().Name + "('" + extra + "')('" + ((info != null) ? info.GetHashCode() : 0) + "')");
		}

		public static implicit operator FileSystemProxyDokanOperations(FileSystem FileSystem)
		{
			return new FileSystemProxyDokanOperations(FileSystem);
		}

		public int CreateFile(string filename, System.IO.FileAccess access, System.IO.FileShare share, System.IO.FileMode mode, System.IO.FileOptions options, Dokan.DokanFileInfo info)
		{
			try
			{
				info.Context = FileSystem.OpenFile(filename, mode);
				return 0;
			}
			catch (Exception e)
			{
				Console.WriteLine(" -- CreateFile Error: " + e.Message);
				info.Context = null;
				//return -1;
				return 0;
			}
			finally
			{
				NotImplemented(filename, info.Context);
			}
			//info.Context = FileSystem.OpenFile(filename, mode);
		}

		public int OpenDirectory(string filename, Dokan.DokanFileInfo info)
		{
			NotImplemented(filename);
			return 0;
		}

		public int CreateDirectory(string filename, Dokan.DokanFileInfo info)
		{
			NotImplemented(filename, info.Context);
			return -1;
		}

		public int Cleanup(string filename, Dokan.DokanFileInfo info)
		{
			info.Context = null;
			//NotImplemented();
			return 0;
		}

		public int CloseFile(string filename, Dokan.DokanFileInfo info)
		{
			try
			{
				((FileSystemFileStream)info.Context).Close();
				return 0;
			}
			catch (Exception e)
			{
				Console.WriteLine(" -- CloseFile Error: " + e.Message);
				return 0;
			}
			finally
			{
				NotImplemented(filename, info.Context);
			}
			//((FileSystemFileStream)info.Context).Close();
			//return 0;
			//NotImplemented();
		}

		public int ReadFile(string filename, byte[] buffer, ref uint readBytes, long offset, Dokan.DokanFileInfo info)
		{
			try
			{
				var FileSystemFileStream = (FileSystemFileStream)info.Context;
				FileSystemFileStream.Position = offset;
				readBytes = (uint)FileSystemFileStream.Read(buffer, 0, (int)readBytes);
				return 0;
			}
			catch (Exception e)
			{
				Console.WriteLine(" -- ReadFile Error: " + e.Message);
				return -1;
			}
			finally
			{
				NotImplemented(filename, info);
			}
		}

		public int WriteFile(string filename, byte[] buffer, ref uint writtenBytes, long offset, Dokan.DokanFileInfo info)
		{
			NotImplemented(filename, info);
			return -1;
		}

		public int FlushFileBuffers(string filename, Dokan.DokanFileInfo info)
		{
			NotImplemented();
			return -1;
		}

		static void FillFileInformationFromFileSystemEntry(FileInformation FileInformation, FileSystemEntry FileSystemEntry)
		{
			FileInformation.Attributes = System.IO.FileAttributes.Normal;
			if (FileSystemEntry.Type.HasFlag(FileSystemEntry.EntryType.Directory))
			{
				FileInformation.Attributes |= FileAttributes.Directory;
			}

			if (FileSystemEntry.ExtendedFlags.HasFlag(FileSystemEntry.ExtendedFlagsTypes.Compressed))
			{
				FileInformation.Attributes |= FileAttributes.Compressed;
			}

			if (FileSystemEntry.ExtendedFlags.HasFlag(FileSystemEntry.ExtendedFlagsTypes.Hidden))
			{
				FileInformation.Attributes |= FileAttributes.Hidden;
			}

			if (FileSystemEntry.ExtendedFlags.HasFlag(FileSystemEntry.ExtendedFlagsTypes.Encrypted))
			{
				FileInformation.Attributes |= FileAttributes.Encrypted;
			}

			if (FileSystemEntry.ExtendedFlags.HasFlag(FileSystemEntry.ExtendedFlagsTypes.System))
			{
				FileInformation.Attributes |= FileAttributes.System;
			}

			if (FileSystemEntry.ExtendedFlags.HasFlag(FileSystemEntry.ExtendedFlagsTypes.Archive))
			{
				FileInformation.Attributes |= FileAttributes.Archive;
			}

			if (FileSystemEntry.ExtendedFlags.HasFlag(FileSystemEntry.ExtendedFlagsTypes.Device))
			{
				FileInformation.Attributes |= FileAttributes.Device;
			}

			FileInformation.Length = FileSystemEntry.Size;
			FileInformation.FileName = FileSystemEntry.Name;
			FileInformation.CreationTime = FileSystemEntry.Time.CreationTime;
			FileInformation.LastWriteTime = FileSystemEntry.Time.LastWriteTime;
			FileInformation.LastAccessTime = FileSystemEntry.Time.LastAccessTime;
		}

		public int GetFileInformation(string filename, Dokan.FileInformation fileinfo, Dokan.DokanFileInfo info)
		{
			NotImplemented(filename, info.Context);
			try
			{
				var FileSystemEntry = FileSystem.GetFileInfo(filename);
				FillFileInformationFromFileSystemEntry(fileinfo, FileSystemEntry);
				return 0;
			}
			catch (Exception)
			{
				return -1;
				//return 0;
			}
		}

		public int FindFiles(string filename, LinkedList<FileInformation> files, Dokan.DokanFileInfo info)
		{
			Console.WriteLine("FindFiles: " + filename + ":" + info);
			//foreach (var Item in FileSystem.FindFiles(filename.Replace(@"\", "/")))
			foreach (var Item in FileSystem.FindFiles(filename))
			{
				var FileInformation = new FileInformation();
				FillFileInformationFromFileSystemEntry(FileInformation, Item);

				//FileInformation.CreationTime.

				/*
				Console.WriteLine("------------------------------------------------");
				Console.WriteLine("CreationTime  : " + FileInformation.CreationTime);
				Console.WriteLine("LastWriteTime : " + FileInformation.LastWriteTime);
				Console.WriteLine("LastAccessTime: " + FileInformation.LastAccessTime);
				Console.WriteLine("------------------------------------------------");
				*/

				//var AvailableDate = Item.Time.CreationTime ?? Item.Time.LastWriteTime ?? Item.Time.LastAccessTime;

				/*
				FileInformation.CreationTime = DateTime.Now;
				FileInformation.LastAccessTime = DateTime.Now;
				FileInformation.LastWriteTime = DateTime.Now;
				*/
				//FileInformation.Length = 10001;
				//Console.WriteLine(Item);
				files.AddLast(FileInformation);
			}
			//Thread.Sleep(400);
			return 0;
			//NotImplemented();
		}

		public int SetFileAttributes(string filename, System.IO.FileAttributes attr, Dokan.DokanFileInfo info)
		{
			NotImplemented();
			return -1;
		}

		public int SetFileTime(string filename, DateTime ctime, DateTime atime, DateTime mtime, Dokan.DokanFileInfo info)
		{
			NotImplemented();
			return -1;
		}

		public int DeleteFile(string filename, Dokan.DokanFileInfo info)
		{
			NotImplemented();
			return -1;
		}

		public int DeleteDirectory(string filename, Dokan.DokanFileInfo info)
		{
			NotImplemented();
			return -1;
		}

		public int MoveFile(string filename, string newname, bool replace, Dokan.DokanFileInfo info)
		{
			NotImplemented();
			return -1;
		}

		public int SetEndOfFile(string filename, long length, Dokan.DokanFileInfo info)
		{
			NotImplemented();
			return -1;
		}

		public int SetAllocationSize(string filename, long length, Dokan.DokanFileInfo info)
		{
			NotImplemented();
			return -1;
		}

		public int LockFile(string filename, long offset, long length, Dokan.DokanFileInfo info)
		{
			NotImplemented();
			return 0;
		}

		public int UnlockFile(string filename, long offset, long length, Dokan.DokanFileInfo info)
		{
			NotImplemented();
			return 0;
		}

		public int GetDiskFreeSpace(ref ulong freeBytesAvailable, ref ulong totalBytes, ref ulong totalFreeBytes, Dokan.DokanFileInfo info)
		{
			freeBytesAvailable = 512 * 1024 * 1024;
			totalBytes = 1024 * 1024 * 1024;
			totalFreeBytes = 512 * 1024 * 1024;
			return 0;
		}

		public int Unmount(Dokan.DokanFileInfo info)
		{
			NotImplemented();
			return 0;
		}
	}
}
