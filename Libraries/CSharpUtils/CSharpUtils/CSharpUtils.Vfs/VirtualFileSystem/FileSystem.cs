using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using CSharpUtils;
using System.Diagnostics;

namespace CSharpUtils.VirtualFileSystem
{
	// http://msdn.microsoft.com/en-us/library/system.io.isolatedstorage.isolatedstoragefile.aspx
	// http://vfs.codeplex.com/
	abstract public partial class FileSystem : IDisposable
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileName"></param>
		/// <param name="FileMode"></param>
		/// <returns></returns>
		/// <see cref="http://docs.python.org/whatsnew/2.6.html#pep-343-the-with-statement"/>
		/// <see cref="http://msdn.microsoft.com/en-us/library/yh598w02(v=VS.100).aspx"/>
		abstract protected FileSystemFileStream ImplOpenFile(String FileName, FileMode FileMode);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileName"></param>
		/// <param name="FileMode"></param>
		/// <param name="Action"></param>
		public void OpenFileScope(String FileName, FileMode FileMode, Action<Stream> Action)
		{
			using (var Stream = OpenFile(FileName, FileMode))
			{
				Action(Stream);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileName"></param>
		/// <param name="FileMode"></param>
		/// <param name="Action"></param>
		public void OpenFileReadScope(String FileName, Action<Stream> Action)
		{
			using (var Stream = OpenFileRead(FileName))
			{
				Action(Stream);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileName"></param>
		/// <param name="FileMode"></param>
		/// <param name="Action"></param>
		public void OpenFileRWScope(String FileName, Action<Stream> Action)
		{
			using (var Stream = OpenFileRW(FileName))
			{
				Action(Stream);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileName"></param>
		/// <param name="FileMode"></param>
		/// <param name="Action"></param>
		public void OpenFileCreateScope(String FileName, Action<Stream> Action)
		{
			using (var Stream = OpenFileCreate(FileName))
			{
				Action(Stream);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileName"></param>
		/// <param name="FileMode"></param>
		/// <returns></returns>
		public FileSystemFileStream OpenFile(String FileName, FileMode FileMode)
		{
			FileSystem NewFileSystem; String NewFileName; Access(FileName, out NewFileSystem, out NewFileName);
			return NewFileSystem.ImplOpenFile(NewFileName, FileMode);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileName"></param>
		/// <returns></returns>
		public FileSystemFileStream OpenFileCreate(String FileName)
		{
			return OpenFile(FileName, FileMode.Create);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileName"></param>
		/// <returns></returns>
		public FileSystemFileStream OpenFileRW(String FileName)
		{
			return OpenFile(FileName, FileMode.Open);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileName"></param>
		/// <returns></returns>
		public FileSystemFileStream OpenFileRead(String FileName)
		{
			return OpenFile(FileName, FileMode.Open);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileName"></param>
		/// <returns></returns>
		public bool Exists(string FileName)
		{
			try
			{
				var Info = this.GetFileInfo(FileName);
				return true;
			}
			catch (FileNotFoundException)
			{
				return false;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileName"></param>
		/// <param name="Buffer"></param>
		public void WriteAllBytes(string FileName, byte[] Buffer)
		{
			using (var File = OpenFile(FileName, FileMode.Create))
			{
				File.Write(Buffer, 0, Buffer.Length);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileName"></param>
		/// <returns></returns>
		public byte[] ReadAllBytes(string FileName)
		{
			using (var File = OpenFile(FileName, FileMode.Open))
			{
				var Bytes = new byte[File.Length];
				File.Read(Bytes, 0, Bytes.Length);
				return Bytes;
			}

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Path"></param>
		/// <param name="FileTime"></param>
		abstract protected void ImplSetFileTime(String Path, FileSystemEntry.FileTime FileTime);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Path"></param>
		/// <param name="FileTime"></param>
		public void SetFileTime(String Path, FileSystemEntry.FileTime FileTime)
		{
			FileSystem NewFileSystem; String NewPath; Access(Path, out NewFileSystem, out NewPath);
			NewFileSystem.ImplSetFileTime(NewPath, FileTime);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Path"></param>
		/// <returns></returns>
		abstract protected FileSystemEntry ImplGetFileInfo(String Path);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Path"></param>
		/// <returns></returns>
		public FileSystemEntry GetFileInfo(String Path)
		{
			FileSystem NewFileSystem; String NewPath; Access(Path, out NewFileSystem, out NewPath);
			return NewFileSystem.ImplGetFileInfo(NewPath);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Path"></param>
		/// <returns></returns>
		public FileSystemEntry.FileTime GetFileTime(String Path)
		{
			return this.GetFileInfo(Path).Time;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Path"></param>
		abstract protected void ImplDeleteFile(String Path);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Path"></param>
		public void DeleteFile(String Path)
		{
			FileSystem NewFileSystem; String NewPath; Access(Path, out NewFileSystem, out NewPath);
			NewFileSystem.ImplDeleteFile(NewPath);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Path"></param>
		abstract protected void ImplDeleteDirectory(String Path);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Path"></param>
		public void DeleteDirectory(String Path)
		{
			FileSystem NewFileSystem; String NewPath; Access(Path, out NewFileSystem, out NewPath);
			NewFileSystem.ImplDeleteDirectory(NewPath);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Path"></param>
		/// <param name="Mode"></param>
		abstract protected void ImplCreateDirectory(String Path, int Mode = 0777);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Path"></param>
		/// <param name="Mode"></param>
		/// <param name="ThrowErrorIfNotExists"></param>
		public void CreateDirectory(String Path, int Mode = 0777, bool ThrowErrorIfNotExists = true)
		{
			FileSystem NewFileSystem; String NewPath; Access(Path, out NewFileSystem, out NewPath);
			if (ThrowErrorIfNotExists)
			{
				NewFileSystem.ImplCreateDirectory(NewPath, Mode);
			}
			else
			{
				try
				{
					NewFileSystem.ImplCreateDirectory(NewPath, Mode);
				}
				catch
				{
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ExistingFileName"></param>
		/// <param name="NewFileName"></param>
		/// <param name="ReplaceExisiting"></param>
		abstract protected void ImplMoveFile(String ExistingFileName, String NewFileName, bool ReplaceExisiting);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ExistingFileName"></param>
		/// <param name="NewFileName"></param>
		/// <param name="ReplaceExisiting"></param>
		public void MoveFile(String ExistingFileName, String NewFileName, bool ReplaceExisiting)
		{
			FileSystem NewFileSystem1; String NewPath1;
			FileSystem NewFileSystem2; String NewPath2;

			Access(ExistingFileName, out NewFileSystem1, out NewPath1);
			Access(NewFileName, out NewFileSystem2, out NewPath2);

			if (NewFileSystem1 != NewFileSystem2)
			{
				throw(new Exception("Can't MoveFile in different file systems"));
			}

			NewFileSystem1.ImplMoveFile(NewPath1, NewPath2, ReplaceExisiting);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="NewPath"></param>
		/// <returns></returns>
		public IEnumerable<FileSystemEntry> FindMountedFiles(String NewPath)
		{
			foreach (var MountedFileSystemPath in MountedFileSystems.Keys)
			{
				if (MountedFileSystemPath.StartsWith(NewPath))
				{
					var Components = MountedFileSystemPath.Substring(NewPath.Length).TrimStart('/').Split('/');
					FileSystemEntry FileSystemEntry = new FileSystemEntry(this, Components[0]);
					FileSystemEntry.Type = VirtualFileSystem.FileSystemEntry.EntryType.Directory;
					yield return FileSystemEntry;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileSystemEntry"></param>
		/// <returns></returns>
		virtual protected FileSystemEntry FilterFileSystemEntry(FileSystemEntry FileSystemEntry)
		{
			return FileSystemEntry;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Path"></param>
		/// <returns></returns>
		abstract protected IEnumerable<FileSystemEntry> ImplFindFiles(String Path);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Pointer"></param>
		/// <param name="Pointee"></param>
		abstract protected void ImplCreateSymLink(String Pointer, String Pointee);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Pointer"></param>
		/// <param name="Pointee"></param>
		public void CreateSymLink(String Pointer, String Pointee)
		{
			FileSystem _PointerFileSystem; String _Pointer;
			FileSystem _PointeeFileSystem; String _Pointee;

			Access(Pointer, out _PointerFileSystem, out _Pointer);
			Access(Pointee, out _PointeeFileSystem, out _Pointee);

			if (_PointerFileSystem != _PointeeFileSystem)
			{
				throw (new Exception("Can't Create Sym links in different file systems"));
			}

			_PointerFileSystem.ImplCreateSymLink(_Pointer, _Pointee);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Path"></param>
		/// <returns></returns>
		public IEnumerable<FileSystemEntry> FindFiles(String Path)
		{
			FileSystem NewFileSystem; String NewPath; Access(Path, out NewFileSystem, out NewPath);

			return NewFileSystem.FindMountedFiles(NewPath)
				.Concat(NewFileSystem.ImplFindFiles(NewPath))
				.DistinctByKey(FileSystemEntry => FileSystemEntry.Name)
				.Select(FileSystemEntry => FilterFileSystemEntry(FileSystemEntry))
			;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SourceFileSystem"></param>
		/// <param name="SourcePath"></param>
		/// <param name="DestFileSystem"></param>
		/// <param name="DestPath"></param>
		static public void CopyFile(FileSystem SourceFileSystem, String SourcePath, FileSystem DestFileSystem, String DestPath)
		{
			DestFileSystem.OpenFileCreateScope(DestPath, (OutStream) =>
			{
				SourceFileSystem.OpenFileReadScope(SourcePath, (InStream) =>
				{
					InStream.CopyToFast(OutStream);
				});
			});
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="SourceFileSystem"></param>
		/// <param name="SourcePath"></param>
		/// <param name="DestFileSystem"></param>
		/// <param name="DestPath"></param>
		/// <param name="ActionValidate"></param>
		/// <param name="ActionProgress"></param>
		static public void CopyTree(FileSystem SourceFileSystem, String SourcePath, FileSystem DestFileSystem, String DestPath, Func<FileSystemEntry, bool> ActionValidate = null, Action<FileSystemEntry, long, long> ActionProgress = null, byte[] Buffer = null)
		{
			if (ActionValidate == null) ActionValidate = ((Entry) => true);
			if (ActionProgress == null) ActionProgress = (Entry, Current, Max) => { };
			//if (Buffer == null) Buffer = new byte[16 * 1024 * 1024];
			if (Buffer == null) Buffer = new byte[2 * 1024 * 1024];

			foreach (var SourceEntry in SourceFileSystem.FindFiles(SourcePath))
			{
				var SourceFullPath = SourcePath + "/" + SourceEntry.Name;
				var DestFullPath = DestPath + "/" + SourceEntry.Name;

				if (!ActionValidate(SourceEntry)) continue;

				if (SourceEntry.Type == FileSystemEntry.EntryType.Directory)
				{
					DestFileSystem.CreateDirectory(DestFullPath, 0777, false);
					CopyTree(SourceFileSystem, SourceFullPath, DestFileSystem, DestFullPath, ActionValidate, ActionProgress, Buffer);
				}
				else
				{
					//
					if (
						!DestFileSystem.Exists(DestFullPath) ||
						(SourceFileSystem.GetFileInfo(SourceFullPath).Size != DestFileSystem.GetFileInfo(DestFullPath).Size)
					)
					{
						using (var SourceStream = SourceFileSystem.OpenFileRead(SourceFullPath))
						using (var DestStream = DestFileSystem.OpenFile(DestFullPath, FileMode.Create))
						{
							SourceStream.CopyToFast(DestStream, Buffer, (Current, Max) =>
							{
								ActionProgress(SourceEntry, Current, Max);
							});
						}
					}
				}
			}

			Buffer = null;
		}

		public void ReplaceFileWithStream(String Path, Stream NewStream, Action<long, long> Progress = null)
		{
            if (Progress == null) Progress = (_Current, _Total) => { };

			this.OpenFileRWScope(Path, (OldStream) =>
			{
                OldStream.WriteStream(NewStream, (Current, Total) =>
                {
                    Progress(Current, OldStream.Length);
                });
                long Waypoint = OldStream.Position;
                OldStream.WriteByteRepeatedTo(0x00, OldStream.Length, (Current, Total) =>
                {
                    Progress(Waypoint + Current, OldStream.Length);
                });
			});
			
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Path"></param>
		/// <param name="Regex"></param>
		/// <returns></returns>
		public IEnumerable<FileSystemEntry> FindFiles(String Path, Regex Regex)
		{
			return FindFiles(Path)
				.Where(Entry => Regex.IsMatch(Entry.Name))
			;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Path"></param>
		/// <param name="Wildcard"></param>
		/// <returns></returns>
		public IEnumerable<FileSystemEntry> FindFiles(String Path, Wildcard Wildcard)
		{
			return FindFiles(Path, (Regex)Wildcard);
		}
	}
}
