using System;
using System.Collections.Generic;
using CSharpUtils.Net;

namespace CSharpUtils.VirtualFileSystem.Ftp
{
	/**
	 * http://www.networksorcery.com/enp/protocol/ftp.htm
	 */
	public class FtpFileSystem : RemoteFileSystem
	{
		public FTP _Ftp = null;
		public String RootPath;

		public FtpFileSystem() : base()
		{
		}

		public FTP Ftp
		{
			get
			{
				// Reconnect if disconnected.
				if ((_Ftp != null) && !(_Ftp.IsConnected))
				{
					_Ftp = null;
				}

				if (_Ftp == null)
				{
					_Ftp = new FTP();
					_Connect();
				}
				return _Ftp;
			}
		}

		public override RemoteFileSystem EnsureConnect()
		{
			var Ftp = this.Ftp;
			return this;
		}

		public FtpFileSystem(string Host, int Port, string Username, string Password, int timeout = 10000)
			:base (Host, Port, Username, Password, timeout)
		{
		}

		public override void Connect(string Host, int Port, string Username, string Password, int timeout = 10000)
		{
			this.Host = Host;
			this.Port = Port;
			this.Username = Username;
			this.Password = Password;
			this.timeout = timeout;
			_Ftp = null;
		}

		protected void _Connect()
		{
			_Ftp.Connect(this.Host, this.Port, this.Username, this.Password);
			_Ftp.timeout = this.timeout;
			RootPath = _Ftp.GetWorkingDirectory();
		}

		protected override String RealPath(String Path)
		{
			return CombinePath(RootPath, Path);
		}

		public override void Shutdown()
		{
			Ftp.Disconnect();
		}

		public override String DownloadFile(String RemoteFile, String LocalFile = null)
		{
			try
			{
				if (LocalFile == null) LocalFile = GetTempFile();
				Ftp.OpenDownload(RemoteFile, LocalFile, false);
				while (Ftp.DoDownload() > 0) ;
				return LocalFile;
			}
			catch (Exception e)
			{
				throw (new Exception("Can't download ftp file '" + RemoteFile + "' to '" + LocalFile + "' : " + e.Message, e));
			}
		}

		public override void UploadFile(String RemoteFile, String LocalFile)
		{
			try
			{
				Ftp.OpenUpload(LocalFile, RemoteFile, false);
				while (Ftp.DoUpload() > 0) ;
			}
			catch (Exception e)
			{
				throw (new Exception("Can't upload ftp file '" + RemoteFile + "' : " + e.Message, e));
			}
		}

		protected override FileSystemEntry ImplGetFileInfo(String Path)
		{
			String CachedRealPath = RealPath(Path);
			var Info = new FileSystemEntry(this, Path);
			Info.Size = Ftp.GetFileSize(CachedRealPath);
			Info.Time.LastWriteTime = Ftp.GetFileDate(CachedRealPath);
			return Info;
		}

		protected override void ImplDeleteFile(string Path)
		{
			String CachedRealPath = RealPath(Path);
			Ftp.RemoveFile(CachedRealPath);
		}

		protected override IEnumerable<FileSystemEntry> ImplFindFiles(string Path)
		{
			Ftp.ChangeDir(RealPath(Path));
			var Entries = Ftp.ListEntries();
			Ftp.ChangeDir(RootPath);
			foreach (var FTPEntry in Entries)
			{
				//Console.WriteLine(Path + " :: " + FTPEntry.Name);
				var FileSystemEntry = new FtpFileSystemEntry(this, Path + "/" + FTPEntry.Name, FTPEntry);

				yield return FileSystemEntry;
			}
		}

		protected override void ImplCreateDirectory(String Path, int Mode = 0777)
		{
			Ftp.MakeDir(Path);
			//Directory.CreateDirectory(Path);
		}

		public override String Title
		{
			get
			{
				return String.Format("ftp://{0}@{1}{2}", Username, Host, RootPath);
			}
		}

	}
}
