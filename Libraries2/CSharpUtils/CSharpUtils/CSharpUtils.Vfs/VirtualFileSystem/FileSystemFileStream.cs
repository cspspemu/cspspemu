using System;
using System.IO;

namespace CSharpUtils.VirtualFileSystem
{
	/// <summary>
	/// 
	/// </summary>
	abstract public class FileSystemFileStream : Stream
	{
		/// <summary>
		/// 
		/// </summary>
		protected FileSystem FileSystem;

		/// <summary>
		/// 
		/// </summary>
		protected FileSystemEntry FileSystemEntry;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileSystem"></param>
		/// <param name="FileSystemEntry"></param>
		public FileSystemFileStream(FileSystem FileSystem, FileSystemEntry FileSystemEntry = null)
		{
			this.FileSystem = FileSystem;
			this.FileSystemEntry = FileSystemEntry;
		}
	}
}
