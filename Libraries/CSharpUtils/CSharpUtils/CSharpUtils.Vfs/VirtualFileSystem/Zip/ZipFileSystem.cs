using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace CSharpUtils.VirtualFileSystem.Zip
{
	/// <summary>
	/// 
	/// </summary>
	public class ZipFileSystem : FileSystem
	{
		/// <summary>
		/// Specifies values for interacting with zip archive entries.
		/// </summary>
		public enum ZipArchiveMode
		{
			/// <summary>
			/// Only reading archive entries is permitted.
			/// </summary>
			Read = 0,
			
			/// <summary>
			/// Only creating new archive entries is permitted.
			/// </summary>
			Create = 1,
			
			/// <summary>
			/// Both read and write operations are permitted for archive entries.
			/// </summary>
			Update = 2,
		}

		/// <summary>
		/// 
		/// </summary>
		protected String ZipFilePath;

		/// <summary>
		/// 
		/// </summary>
		protected Stream ZipStream;

		/// <summary>
		/// 
		/// </summary>
		protected ZipArchive ZipArchive;

		/// <summary>
		/// 
		/// </summary>
		protected ZipArchiveMode Mode;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ZipFilePath"></param>
		/// <param name="Mode"></param>
		public ZipFileSystem(String ZipFilePath, ZipArchiveMode Mode)
		{
			this.ZipFilePath = ZipFilePath;
			this.Mode = Mode;
			switch (Mode)
			{
				case ZipArchiveMode.Create: this.ZipStream = File.Open(ZipFilePath, FileMode.Create, FileAccess.Write, FileShare.Read); break;
				case ZipArchiveMode.Read: this.ZipStream = File.OpenRead(ZipFilePath); break;
				case ZipArchiveMode.Update: this.ZipStream = File.Open(ZipFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read); break;
				default: throw(new NotImplementedException());
			}
			this.ZipArchive = new ZipArchive(this.ZipStream, (System.IO.Compression.ZipArchiveMode)Mode);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileName"></param>
		/// <param name="FileMode"></param>
		/// <returns></returns>
		protected override FileSystemFileStream ImplOpenFile(string FileName, FileMode FileMode)
		{
			return new FileSystemFileStreamStream(this, this.ZipArchive.GetEntry(FileName).Open());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Path"></param>
		/// <param name="FileTime"></param>
		protected override void ImplSetFileTime(string Path, FileSystemEntry.FileTime FileTime)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Path"></param>
		/// <returns></returns>
		protected override FileSystemEntry ImplGetFileInfo(string Path)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Path"></param>
		protected override void ImplDeleteFile(string Path)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Path"></param>
		protected override void ImplDeleteDirectory(string Path)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Path"></param>
		/// <param name="Mode"></param>
		protected override void ImplCreateDirectory(string Path, int Mode = 0777)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ExistingFileName"></param>
		/// <param name="NewFileName"></param>
		/// <param name="ReplaceExisiting"></param>
		protected override void ImplMoveFile(string ExistingFileName, string NewFileName, bool ReplaceExisiting)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ZipArchiveEntry"></param>
		/// <returns></returns>
		private FileSystemEntry _ConvertFileSystemEntry(ZipArchiveEntry ZipArchiveEntry)
		{
			return new FileSystemEntry(this, ZipArchiveEntry.FullName)
			{
				Size = ZipArchiveEntry.Length,
				Time = new FileSystemEntry.FileTime()
				{
					LastWriteTime = ZipArchiveEntry.LastWriteTime.Date,
				},
				Type = FileSystemEntry.EntryType.File,
			};
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Path"></param>
		/// <returns></returns>
		protected override IEnumerable<FileSystemEntry> ImplFindFiles(string Path)
		{
			var DirectoriesListed = new HashSet<string>();

			Path = (Path + "/").TrimStart('/');

			// List Files
			foreach (var ZipArchiveEntry in ZipArchive.Entries)
			{
				var FullName = ZipArchiveEntry.FullName;
				if (!FullName.StartsWith(Path)) continue;

				var Part = FullName.Substring(Path.Length);
				
				// Directory
				if (Part.IndexOf('/') != -1)
				{
					var Folder = Part.Split('/')[0];

					if (!DirectoriesListed.Contains(Folder))
					{
						//Console.WriteLine("Part: {0} : {1}", Part, Folder);

						var Entry = new FileSystemEntry(this, Path + Folder)
						{
							Size = 0,
							Time = new FileSystemEntry.FileTime()
							{
								LastWriteTime = ZipArchiveEntry.LastWriteTime.Date,
							},
							Type = FileSystemEntry.EntryType.Directory,
						};

						DirectoriesListed.Add(Folder);

						//Console.WriteLine(Entry);

						yield return Entry;
					}
					else
					{
						continue;
					}
				}
				// File
				else
				{
					yield return _ConvertFileSystemEntry(ZipArchiveEntry);
				}
			}
		}

		protected override void ImplCreateSymLink(string Pointer, string Pointee)
		{
			throw new NotImplementedException();
		}
	}
}
