using System;
using System.IO;

namespace CSharpUtils.VirtualFileSystem
{
	public class RemoteFileSystemFileStream : FileSystemFileStreamStream
	{
		protected bool Modified = false;
		public String TempFileName;
		public String FileName;
		public FileMode FileMode;
		protected RemoteFileSystem RemoteFileSystem;

		public RemoteFileSystemFileStream(RemoteFileSystem RemoteFileSystem, String FileName, FileMode FileMode)
			: base(RemoteFileSystem, (Lazy<Stream>)null)
		{
			this.RemoteFileSystem = RemoteFileSystem;
			this.FileName = FileName;
			this.FileMode = FileMode;
		}

		protected Stream _Stream;

		public override Stream Stream
		{
			get
			{
				if (_Stream == null)
				{
					TempFileName = RemoteFileSystem.GetTempFile();
					switch (FileMode)
					{
						case FileMode.CreateNew:
							try
							{
								RemoteFileSystem.GetFileTime(FileName);
								throw (new Exception("File '" + FileName + "' already exists can't open using FileMode.CreateNew"));
							}
							catch
							{
							}
							Modified = true;
							break;
						case FileMode.Create:
							Modified = true;
							break;
						case FileMode.Truncate:
							Modified = true;
							break;
						case FileMode.OpenOrCreate:
						case FileMode.Append:
						case FileMode.Open:
							try {
								RemoteFileSystem.DownloadFile(FileName, TempFileName);
							}
							catch (Exception Exception)
							{
								throw(new FileNotFoundException("Can't find ftp file '" + FileName + "'", Exception));
							}

							break;
					}

					_Stream = new FileStream(TempFileName, FileMode);
				}
				return _Stream;
			}
		}

		public override void SetLength(long value)
		{
			Modified = true;
			base.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			Modified = true;
			base.Write(buffer, offset, count);
		}

		public override void Close()
		{
			base.Close();
			// Has to reupload.
			if (_Stream != null)
			{
				_Stream = null;
				if (Modified)
				{
					RemoteFileSystem.UploadFile(FileName, TempFileName);
				}
				File.Delete(TempFileName);
			}
		}
	}

}
