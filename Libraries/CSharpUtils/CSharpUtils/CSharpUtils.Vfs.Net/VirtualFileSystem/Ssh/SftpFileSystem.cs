using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Tamir.SharpSsh.jsch;

namespace CSharpUtils.VirtualFileSystem.Ssh
{
	public class SftpFileSystem : RemoteFileSystem
	{
		JSch _jsch = null;
		Session _session = null;
		private ChannelSftp _csftp = null;
		String RootPath = "";

		public SftpFileSystem()
			:base()
		{
		}

		public SftpFileSystem(string Host, int Port, string Username, string Password, int timeout = 10000)
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
			_jsch = null;
		}

		protected void _Connect()
		{
			_jsch = new JSch();
			//session.setConfig();
			_session = _jsch.getSession(this.Username, this.Host, this.Port);
			UserInfo ui = new DirectPasswordUserInfo(this.Password);
			_session.setUserInfo(ui);
			_session.connect();

			_csftp = (ChannelSftp)_session.openChannel("sftp");
			_csftp.connect();

			//RootPath = csftp.getHome();
			RootPath = "";
		}

		ChannelSftp csftp
		{
			get
			{
				// Try reconnect.
				if ((_csftp != null) && !_csftp.isConnected())
				{
					_csftp = null;
				}

				if (_csftp == null)
				{
					_Connect();
				}
				return _csftp;
			}
		}

		public override RemoteFileSystem EnsureConnect()
		{
			var csftp = this.csftp;
			return this;
		}

		protected override String RealPath(String Path)
		{
			return RootPath + "/" + Path;
		}

		public override void Shutdown()
		{
			if (_csftp != null)
			{
				_csftp.disconnect();
				_csftp = null;
			}

			if (_session != null)
			{
				_session.disconnect();
				_session = null;
			}

			if (_jsch != null)
			{
				_jsch = null;
			}
		}

		protected override FileSystemEntry ImplGetFileInfo(String Path)
		{
			var FileSystemEntry = new FileSystemEntry(this, Path);
			var stat = csftp.lstat(RealPath(Path));
			FileSystemEntry.Size = stat.Size;
			FileSystemEntry.Time.LastAccessTime = stat.getATime();
			FileSystemEntry.Time.CreationTime = stat.getMTime();
			FileSystemEntry.Time.LastWriteTime = stat.getMTime();
			return FileSystemEntry;
		}

		protected override IEnumerable<FileSystemEntry> ImplFindFiles(String Path)
		{
			foreach (var i in csftp.ListEntries(RealPath(Path)))
			{
				var LsEntry = (Tamir.SharpSsh.jsch.ChannelSftp.LsEntry)i;
				var FileSystemEntry = new FileSystemEntry(this, Path + "/" + LsEntry.FileName);
				FileSystemEntry.Size = LsEntry.Attributes.Size;
				FileSystemEntry.GroupId = LsEntry.Attributes.getGId();
				FileSystemEntry.UserId = LsEntry.Attributes.getUId();
				if (LsEntry.Attributes.IsDirectory) {
					FileSystemEntry.Type = VirtualFileSystem.FileSystemEntry.EntryType.Directory;
				} else if (LsEntry.Attributes.IsLink) {
					FileSystemEntry.Type = VirtualFileSystem.FileSystemEntry.EntryType.Link;
				} else {
					FileSystemEntry.Type = VirtualFileSystem.FileSystemEntry.EntryType.File;
				}
				FileSystemEntry.Time.CreationTime = LsEntry.Attributes.getMTime();
				FileSystemEntry.Time.LastWriteTime = LsEntry.Attributes.getMTime();
				FileSystemEntry.Time.LastAccessTime = LsEntry.Attributes.getATime();
				//Console.WriteLine("FILE(" + LsEntry.getFilename() + ") : (" + LsEntry.getAttrs().getPermissions() + ") (" + String.Join(",", LsEntry.getAttrs().getExtended()) + ")");
				//Console.WriteLine(String.Format("FILE({}) : ({})", LsEntry.getFilename(), Convert.ToString(LsEntry.getAttrs().getPermissions(), 2)));
				//Console.WriteLine("FILE(" + LsEntry.getFilename() + ") : (" + Convert.ToString(LsEntry.getAttrs().getPermissions(), 2) + ")");

				// Version 3 supported.
				// http://tools.ietf.org/wg/secsh/draft-ietf-secsh-filexfer/
				if (FileSystemEntry.Name.Substring(0, 1) == ".")
				{
					FileSystemEntry.ExtendedFlags |= VirtualFileSystem.FileSystemEntry.ExtendedFlagsTypes.Hidden;
				}
				yield return FileSystemEntry;
			}
		}

		public override String DownloadFile(String RemoteFile, String LocalFile = null)
		{
			try
			{
				if (LocalFile == null) LocalFile = GetTempFile();
				csftp.get(RemoteFile, LocalFile);
				return LocalFile;
			}
			catch (Exception e)
			{
				throw (new Exception("Can't download sftp file '" + RemoteFile + "' : " + e.Message, e));
			}
		}

		public override void UploadFile(String RemoteFile, String LocalFile)
		{
			try
			{
				csftp.put(LocalFile, RemoteFile);
			}
			catch (Exception e)
			{
				throw (new Exception("Can't upload sftp file '" + RemoteFile + "' : " + e.Message, e));
			}
		}

		public override String Title
		{
			get
			{
				return String.Format("sftp://{0}@{1}/{2}", Username, Host, RootPath);
			}
		}
	}

	class DirectPasswordUserInfo : UserInfo
	{
		String Password;

		public DirectPasswordUserInfo(String Password) { this.Password = Password; }
		public String getPassword() { return Password; }
		public bool promptYesNo(String str) { return true; }
		public String getPassphrase() { return null; }
		public bool promptPassphrase(String message) { return true; }
		public bool promptPassword(String message) { return true; }
		public void showMessage(String message) { MessageBox.Show(message, "SharpSSH", MessageBoxButtons.OK, MessageBoxIcon.Asterisk); }
	}

}
