using System;
using System.Runtime.CompilerServices;
using Tamir.Streams;
using System.Text;
using CSharpUtils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tamir.SharpSsh.jsch;

namespace Tamir.SharpSsh.jsch
{
	public partial class ChannelSftp : ChannelSession
	{
		public void quit() { disconnect(); }
		public void exit() { disconnect(); }
		public void lcd(String path)
		{ //throws SftpException{
			path = localAbsolutePath(path);
			if (System.IO.Directory.Exists(path))
			{
				try
				{
					path = System.IO.Path.GetFullPath(path);
				}
				catch (Exception) { }
				lcwd = path;
				return;
			}
			throw new SftpException(SSH_FX_NO_SUCH_FILE, "No such directory");
		}

		/*
		cd /tmp
		c->s REALPATH
		s->c NAME
		c->s STAT
		s->c ATTR
		*/
		public void cd(String path)
		{
			//throws SftpException{
			try
			{
				path = RemoteAbsolutePath(path);

				ArrayList v = glob_remote(path);
				if (v.Count != 1)
				{
					throw new SftpException(SSH_FX_FAILURE, v.ToString());
				}
				path = (String)(v[0]);
				sendREALPATH(path);

				Header _header = new Header();
				_header = ReadHeader(buf, _header);
				int length = _header.length;
				int type = _header.type;
				buf.Rewind();
				fill(buf.buffer, 0, length);

				if (type != 101 && type != 104)
				{
					throw new SftpException(SSH_FX_FAILURE, "");
				}
				int i;
				if (type == 101)
				{
					i = buf.ReadInt();
					throwStatusError(buf, i);
				}
				i = buf.ReadInt();
				byte[] str = buf.ReadString();
				if (str != null && str[0] != '/')
				{
					str = Encoding.UTF8.GetBytes(cwd + "/" + str);
				}
				str = buf.ReadString();         // logname
				i = buf.ReadInt();              // attrs

				String newpwd = Encoding.UTF8.GetString(str);
				SftpATTRS attr = _stat(newpwd);
				if ((attr.Flags & SftpATTRS.SSH_FILEXFER_ATTR_PERMISSIONS) == 0)
				{
					throw new SftpException(SSH_FX_FAILURE, "Can't change directory: " + path);
				}

				if (!attr.IsDirectory)
				{
					throw new SftpException(SSH_FX_FAILURE, "Can't change directory: " + path);
				}
				cwd = newpwd;
			}
			catch (Exception e)
			{
				if (e is SftpException) throw (SftpException)e;
				throw new SftpException(SSH_FX_FAILURE, "");
			}
		}

		/*
		put foo
		c->s OPEN
		s->c HANDLE
		c->s WRITE
		s->c STATUS
		c->s CLOSE
		s->c STATUS
		*/
		public void put(String src, String dst)
		{ //throws SftpException{
			put(src, dst, null, OVERWRITE);
		}
		public void put(String src, String dst, int mode)
		{ //throws SftpException{
			put(src, dst, null, mode);
		}
		public void put(String src, String dst,
			SftpProgressMonitor monitor)
		{ //throws SftpException{
			put(src, dst, monitor, OVERWRITE);
		}
		public void put(String src, String dst,
			SftpProgressMonitor monitor, int mode)
		{
			//throws SftpException{
			src = localAbsolutePath(src);
			dst = RemoteAbsolutePath(dst);

			//System.err.println("src: "+src+", "+dst);
			try
			{
				ArrayList v = glob_remote(dst);
				int vsize = v.Count;
				if (vsize != 1)
				{
					if (vsize == 0)
					{
						if (IsPattern(dst))
							throw new SftpException(SSH_FX_FAILURE, dst);
						else
							dst = Util.unquote(dst);
					}
					throw new SftpException(SSH_FX_FAILURE, v.ToString());
				}
				else
				{
					dst = (String)(v[0]);
				}

				//System.err.println("dst: "+dst);

				bool _isRemoteDir = isRemoteDir(dst);

				v = glob_local(src);
				//System.err.println("glob_local: "+v+" dst="+dst);
				vsize = v.Count;

				string dstsb = "";
				if (_isRemoteDir)
				{
					if (!dst.EndsWith("/"))
					{
						dst += "/";
					}
					dstsb = (dst);
				}
				else if (vsize > 1)
				{
					throw new SftpException(SSH_FX_FAILURE, "Copying multiple files, but destination is missing or a file.");
				}

				for (int j = 0; j < vsize; j++)
				{
					String _src = (String)(v[j]);
					String _dst = null;
					if (_isRemoteDir)
					{
						int i = _src.LastIndexOf(file_separatorc);
						if (i == -1) dstsb += (_src);
						else dstsb += (_src.Substring(i + 1));
						_dst = dstsb;
						dstsb = dstsb.Remove(dst.Length, _dst.Length);
					}
					else
					{
						_dst = dst;
					}
					//System.err.println("_dst "+_dst);

					long size_of_dst = 0;
					if (mode == RESUME)
					{
						try
						{
							SftpATTRS attr = _stat(_dst);
							size_of_dst = attr.Size;
						}
						catch (Exception)
						{
							//System.err.println(eee);
						}

						long size_of_src = new System.IO.FileInfo(_src).Length;
						if (size_of_src < size_of_dst)
						{
							throw new SftpException(SSH_FX_FAILURE, "failed to resume for " + _dst);
						}
						if (size_of_src == size_of_dst)
						{
							return;
						}
					}

					if (monitor != null)
					{
						monitor.init(
							SftpProgressMonitor.PUT,
							_src,
							_dst,
							new System.IO.FileInfo(_src).Length
						);
						if (mode == RESUME)
						{
							monitor.count(size_of_dst);
						}
					}
					FileInputStream fis = null;
					try
					{
						fis = new FileInputStream(_src);
						_put(fis, _dst, monitor, mode);
					}
					finally
					{
						if (fis != null)
						{
							//	    try{
							fis.close();
							//	    }catch(Exception ee){};
						}
					}
				}
			}
			catch (Exception e)
			{
				if (e is SftpException) throw (SftpException)e;
				throw new SftpException(SSH_FX_FAILURE, e.ToString());
			}
		}
		public void put(InputStream src, String dst)
		{ //throws SftpException{
			put(src, dst, null, OVERWRITE);
		}
		public void put(InputStream src, String dst, int mode)
		{ //throws SftpException{
			put(src, dst, null, mode);
		}
		public void put(InputStream src, String dst,
			SftpProgressMonitor monitor)
		{ //throws SftpException{
			put(src, dst, monitor, OVERWRITE);
		}
		public void put(InputStream src, String dst,
			SftpProgressMonitor monitor, int mode)
		{ //throws SftpException{
			try
			{
				dst = RemoteAbsolutePath(dst);
				ArrayList v = glob_remote(dst);
				int vsize = v.Count;
				if (vsize != 1)
				{
					if (vsize == 0)
					{
						if (IsPattern(dst))
							throw new SftpException(SSH_FX_FAILURE, dst);
						else
							dst = Util.unquote(dst);
					}
					throw new SftpException(SSH_FX_FAILURE, v.ToString());
				}
				else
				{
					dst = (String)(v[0]);
				}
				if (isRemoteDir(dst))
				{
					throw new SftpException(SSH_FX_FAILURE, dst + " is a directory");
				}
				_put(src, dst, monitor, mode);
			}
			catch (Exception e)
			{
				if (e is SftpException) throw (SftpException)e;
				throw new SftpException(SSH_FX_FAILURE, e.ToString());
			}
		}

		private void _put(InputStream src, String dst,
			SftpProgressMonitor monitor, int mode)
		{
			try
			{
				long skip = 0;
				if (mode == RESUME || mode == APPEND)
				{
					try
					{
						SftpATTRS attr = _stat(dst);
						skip = attr.Size;
					}
					catch (Exception)
					{
						//System.err.println(eee);
					}
				}
				if (mode == RESUME && skip > 0)
				{
					long skipped = src.skip(skip);
					if (skipped < skip)
					{
						throw new SftpException(SSH_FX_FAILURE, "failed to resume for " + dst);
					}
				}
				if (mode == OVERWRITE) { sendOPENW(dst); }
				else { sendOPENA(dst); }

				Header _header = new Header();
				_header = ReadHeader(buf, _header);
				int length = _header.length;
				int type = _header.type;
				buf.Rewind();
				fill(buf.buffer, 0, length);

				if (type != SSH_FXP_STATUS && type != SSH_FXP_HANDLE)
				{
					throw new SftpException(SSH_FX_FAILURE, "invalid type=" + type);
				}
				if (type == SSH_FXP_STATUS)
				{
					int i = buf.ReadInt();
					throwStatusError(buf, i);
				}
				byte[] handle = buf.ReadString();         // filename
				byte[] data = null;

				bool dontcopy = true;

				if (!dontcopy)
				{
					data = new byte[buf.buffer.Length
								  - (5 + 13 + 21 + handle.Length
									+ 32 + 20 // padding and mac
								   )
						];
				}

				long offset = 0;
				if (mode == RESUME || mode == APPEND)
				{
					offset += skip;
				}

				int startid = seq;
				int _ackid = seq;
				int ackcount = 0;
				while (true)
				{
					int nread = 0;
					int s = 0;
					int datalen = 0;
					int count = 0;

					if (!dontcopy)
					{
						datalen = data.Length - s;
					}
					else
					{
						data = buf.buffer;
						s = 5 + 13 + 21 + handle.Length;
						datalen = buf.buffer.Length - s
								- 32 - 20; // padding and mac
					}

					do
					{
						nread = src.read(data, s, datalen);
						if (nread > 0)
						{
							s += nread;
							datalen -= nread;
							count += nread;
						}
					}
					while (datalen > 0 && nread > 0);
					if (count <= 0) break;

					int _i = count;
					while (_i > 0)
					{
						_i -= sendWRITE(handle, offset, data, 0, _i);
						if ((seq - 1) == startid ||
						   io.ins.available() >= 1024)
						{
							while (io.ins.available() > 0)
							{
								if (checkStatus(ackid, _header))
								{
									_ackid = ackid[0];
									if (startid > _ackid || _ackid > seq - 1)
									{
										if (_ackid == seq)
										{
											System.Console.Error.WriteLine("ack error: startid=" + startid + " seq=" + seq + " _ackid=" + _ackid);
										}
										else
										{
											//throw new SftpException(SSH_FX_FAILURE, "ack error:");
											throw new SftpException(SSH_FX_FAILURE, "ack error: startid=" + startid + " seq=" + seq + " _ackid=" + _ackid);
										}
									}
									ackcount++;
								}
								else
								{
									break;
								}
							}
						}
					}
					offset += count;
					if (monitor != null && !monitor.count(count))
					{
						break;
					}
				}
				int _ackcount = seq - startid;
				while (_ackcount > ackcount)
				{
					if (!checkStatus(null, _header))
					{
						break;
					}
					ackcount++;
				}
				if (monitor != null) monitor.end();
				_sendCLOSE(handle, _header);
				//System.err.println("start end "+startid+" "+endid);
			}
			catch (Exception e)
			{
				if (e is SftpException) throw (SftpException)e;
				throw new SftpException(SSH_FX_FAILURE, e.ToString());
			}
		}

		private SftpATTRS _stat(String path)
		{
			try
			{
				sendSTAT(path);

				Header _header = new Header();
				_header = ReadHeader(buf, _header);
				int length = _header.length;
				int type = _header.type;
				buf.Rewind();
				fill(buf.buffer, 0, length);

				if (type != SSH_FXP_ATTRS)
				{
					if (type == SSH_FXP_STATUS)
					{
						int i = buf.ReadInt();
						throwStatusError(buf, i);
					}
					throw new SftpException(SSH_FX_FAILURE, "");
				}
				SftpATTRS attr = SftpATTRS.getATTR(buf);
				return attr;
			}
			catch (Exception e)
			{
				if (e is SftpException) throw (SftpException)e;
				throw new SftpException(SSH_FX_FAILURE, "");
			}
			//return null;
		}

		/**/

		public OutputStream put(String dst)
		{ //throws SftpException{
			return put(dst, (SftpProgressMonitor)null, OVERWRITE);
		}
		public OutputStream put(String dst, int mode)
		{ //throws SftpException{
			return put(dst, (SftpProgressMonitor)null, mode);
		}
		public OutputStream put(String dst, SftpProgressMonitor monitor, int mode)
		{ //throws SftpException{
			return put(dst, monitor, mode, 0);
		}
		public OutputStream put(String dst, SftpProgressMonitor monitor, int mode, long offset)
		{
			dst = RemoteAbsolutePath(dst);
			try
			{
				ArrayList v = glob_remote(dst);
				if (v.Count != 1)
				{
					throw new SftpException(SSH_FX_FAILURE, v.ToString());
				}
				dst = (String)(v[0]);
				if (isRemoteDir(dst))
				{
					throw new SftpException(SSH_FX_FAILURE, dst + " is a directory");
				}

				long skip = 0;
				if (mode == RESUME || mode == APPEND)
				{
					try
					{
						SftpATTRS attr = stat(dst);
						skip = attr.Size;
					}
					catch (Exception)
					{
						//System.out.println(eee);
					}
				}

				if (mode == OVERWRITE) { sendOPENW(dst); }
				else { sendOPENA(dst); }

				Header _header = new Header();
				_header = ReadHeader(buf, _header);
				int length = _header.length;
				int type = _header.type;

				buf.Rewind();
				fill(buf.buffer, 0, length);

				if (type != SSH_FXP_STATUS && type != SSH_FXP_HANDLE)
				{
					throw new SftpException(SSH_FX_FAILURE, "");
				}
				if (type == SSH_FXP_STATUS)
				{
					int i = buf.ReadInt();
					throwStatusError(buf, i);
				}

				byte[] handle = buf.ReadString();         // filename

				//long offset=0;
				if (mode == RESUME || mode == APPEND)
				{
					offset += skip;
				}

				long[] _offset = new long[1];
				_offset[0] = offset;
				OutputStream outs = new OutputStreamPut(this, handle, _offset, monitor);
				//  private bool init=true;
				//  private int[] ackid=new int[1];
				//  private int startid=0;
				//  private int _ackid=0;
				//  private int ackcount=0;

				//  public void write(byte[] d, int s, int len) { //throws java.io.IOException{

				//    if(init){
				//      startid=count;
				//      _ackid=count;
				//      init=false;
				//    }

				//    try{
				//      int _len=len;
				//      while(_len>0){
				//        _len-=sendWRITE(handle, _offset[0], d, s, _len);

				//        if((count-1)==startid ||
				//           io.ins.available()>=1024){
				//          while(io.ins.available()>0){
				//            if(checkStatus(ackid)){
				//              _ackid=ackid[0];
				//              if(startid>_ackid || _ackid>count-1){
				//                throw new SftpException(SSH_FX_FAILURE, "");
				//              }
				//              ackcount++;
				//            }
				//            else{
				//              break;
				//            }
				//          }
				//        }

				//      }
				//      _offset[0]+=len;
				//      if(monitor!=null && !monitor.count(len)){
				//        throw new IOException("canceled");
				//  }
				//    }
				//    catch(IOException e){ throw e; }
				//    catch(Exception e){ throw new IOException(e.toString());  }
				//  }
				//  byte[] _data=new byte[1];
				//  public void write(int foo) { //throws java.io.IOException{
				//    _data[0]=(byte)foo;
				//    write(_data, 0, 1);
				//  }
				//  public void close() { //throws java.io.IOException{

				//    try{
				//      int _ackcount=count-startid;
				//      while(_ackcount>ackcount){
				//        if(!checkStatus(null)){
				//          break;
				//        }
				//        ackcount++;
				//      }
				//    }
				//    catch(SftpException e){
				//      throw new IOException(e.toString());
				//    }

				//    if(monitor!=null)monitor.end();
				//    try{ _sendCLOSE(handle); }
				//    catch(IOException e){ throw e; }
				//    catch(Exception e){
				//      throw new IOException(e.toString());
				//    }
				//  }
				//};
				return outs;
			}
			catch (Exception e)
			{
				if (e is SftpException) throw (SftpException)e;
				throw new SftpException(SSH_FX_FAILURE, "");
			}
		}

		/**/
		public void get(String src, String dst)
		{ //throws SftpException{
			get(src, dst, null, OVERWRITE);
		}
		public void get(String src, String dst,
			SftpProgressMonitor monitor)
		{ //throws SftpException{
			get(src, dst, monitor, OVERWRITE);
		}
		public void get(String src, String dst,
			SftpProgressMonitor monitor, int mode)
		{
			//throws SftpException{
			src = RemoteAbsolutePath(src);
			dst = localAbsolutePath(dst);
			try
			{
				ArrayList v = glob_remote(src);
				int vsize = v.Count;
				if (vsize == 0)
				{
					throw new SftpException(SSH_FX_NO_SUCH_FILE, "No such file");
				}

				bool isDstDir = System.IO.Directory.Exists(dst);
				string dstsb = "";
				if (isDstDir)
				{
					if (!dst.EndsWith(file_separator))
					{
						dst += file_separator;
					}
					dstsb = (dst);
				}
				else if (vsize > 1)
				{
					throw new SftpException(SSH_FX_FAILURE, "Copying multiple files, but destination is missing or a file.");
				}

				for (int j = 0; j < vsize; j++)
				{
					String _src = (String)(v[j]);

					SftpATTRS attr = _stat(_src);
					if (attr.IsDirectory)
					{
						throw new SftpException(SSH_FX_FAILURE, "not supported to get directory " + _src);
					}

					String _dst = null;
					if (isDstDir)
					{
						int i = _src.LastIndexOf('/');
						if (i == -1) dstsb += (_src);
						else dstsb += (_src.Substring(i + 1));
						_dst = dstsb;
						dstsb = dstsb.Remove(dst.Length, _dst.Length);
					}
					else
					{
						_dst = dst;
					}

					if (mode == RESUME)
					{
						long size_of_src = attr.Size;
						long size_of_dst = new System.IO.FileInfo(_dst).Length;
						if (size_of_dst > size_of_src)
						{
							throw new SftpException(SSH_FX_FAILURE, "failed to resume for " + _dst);
						}
						if (size_of_dst == size_of_src)
						{
							return;
						}
					}

					if (monitor != null)
					{
						monitor.init(SftpProgressMonitor.GET, _src, _dst, attr.Size);
						if (mode == RESUME)
						{
							monitor.count(new System.IO.FileInfo(_dst).Length);
						}
					}
					FileOutputStream fos = null;
					if (mode == OVERWRITE)
					{
						fos = new FileOutputStream(_dst);
					}
					else
					{
						fos = new FileOutputStream(_dst, true); // append
					}

					//System.err.println("_get: "+_src+", "+_dst);
					_get(_src, fos, monitor, mode, new System.IO.FileInfo(_dst).Length);
					fos.close();
				}
			}
			catch (Exception e)
			{
				if (e is SftpException) throw (SftpException)e;
				throw new SftpException(SSH_FX_FAILURE, "");
			}
		}
		public void get(String src, OutputStream dst)
		{ //throws SftpException{
			get(src, dst, null, OVERWRITE, 0);
		}
		public void get(String src, OutputStream dst,
			SftpProgressMonitor monitor)
		{ //throws SftpException{
			get(src, dst, monitor, OVERWRITE, 0);
		}
		public void get(String src, OutputStream dst,
			SftpProgressMonitor monitor, int mode, long skip)
		{
			//throws SftpException{
			try
			{
				src = RemoteAbsolutePath(src);
				ArrayList v = glob_remote(src);
				if (v.Count != 1)
				{
					throw new SftpException(SSH_FX_FAILURE, v.ToString());
				}
				src = (String)(v[0]);

				if (monitor != null)
				{
					SftpATTRS attr = _stat(src);
					monitor.init(SftpProgressMonitor.GET, src, "??", attr.Size);
					if (mode == RESUME)
					{
						monitor.count(skip);
					}
				}
				_get(src, dst, monitor, mode, skip);
			}
			catch (Exception e)
			{
				if (e is SftpException) throw (SftpException)e;
				throw new SftpException(SSH_FX_FAILURE, "");
			}
		}

		///tamir: updated to jcsh-0.1.30
		private void _get(String src, OutputStream dst,
			SftpProgressMonitor monitor, int mode, long skip)
		{ //throws SftpException{
			//System.out.println("_get: "+src+", "+dst);
			try
			{
				sendOPENR(src);


				Header _header = new Header();
				_header = ReadHeader(buf, _header);
				int length = _header.length;
				int type = _header.type;

				buf.Rewind();

				fill(buf.buffer, 0, length);

				if (type != SSH_FXP_STATUS && type != SSH_FXP_HANDLE)
				{
					//System.Console.WriteLine("Type is "+type);
					throw new SftpException(SSH_FX_FAILURE, "Type is " + type);
				}

				if (type == SSH_FXP_STATUS)
				{
					int i = buf.ReadInt();
					throwStatusError(buf, i);
				}

				byte[] handle = buf.ReadString();         // filename

				long offset = 0;
				if (mode == RESUME)
				{
					offset += skip;
				}

				int request_len = 0;
			//loop:
				while (true)
				{

					request_len = buf.buffer.Length - 13;
					if (ServerVersion == 0) { request_len = 1024; }
					sendREAD(handle, offset, request_len);

					_header = ReadHeader(buf, _header);
					length = _header.length;
					type = _header.type;

					int i;
					if (type == SSH_FXP_STATUS)
					{
						buf.Rewind();
						fill(buf.buffer, 0, length);
						i = buf.ReadInt();
						if (i == SSH_FX_EOF)
						{
							goto BREAK;
						}
						throwStatusError(buf, i);
					}

					if (type != SSH_FXP_DATA)
					{
						goto BREAK;
					}

					buf.Rewind();
					fill(buf.buffer, 0, 4); length -= 4;
					i = buf.ReadInt();   // length of data 
					int foo = i;
					while (foo > 0)
					{
						int bar = foo;
						if (bar > buf.buffer.Length)
						{
							bar = buf.buffer.Length;
						}
						i = io.ins.read(buf.buffer, 0, bar);
						if (i < 0)
						{
							goto BREAK;
						}
						int data_len = i;
						dst.write(buf.buffer, 0, data_len);

						offset += data_len;
						foo -= data_len;

						if (monitor != null)
						{
							if (!monitor.count(data_len))
							{
								while (foo > 0)
								{
									i = io.ins.read(buf.buffer,
										0,
										(buf.buffer.Length < foo ? buf.buffer.Length : foo));
									if (i <= 0) break;
									foo -= i;
								}
								goto BREAK;
							}
						}
					}
					//System.out.println("length: "+length);  // length should be 0
				}
			BREAK:
				dst.flush();

				if (monitor != null) monitor.end();
				_sendCLOSE(handle, _header);
			}
			catch (Exception e)
			{
				//System.Console.WriteLine(e);
				if (e is SftpException) throw (SftpException)e;
				throw new SftpException(SSH_FX_FAILURE, "");
			}
		}

		public InputStream get(String src, SftpProgressMonitor monitor = null, int mode = OVERWRITE)
		{
			if (mode == RESUME)
			{
				throw new SftpException(SSH_FX_FAILURE, "faile to resume from " + src);
			}
			src = RemoteAbsolutePath(src);
			try
			{
				ArrayList v = glob_remote(src);
				if (v.Count != 1)
				{
					throw new SftpException(SSH_FX_FAILURE, v.ToString());
				}
				src = (String)(v[0]);

				SftpATTRS attr = _stat(src);
				if (monitor != null)
				{
					monitor.init(SftpProgressMonitor.GET, src, "??", attr.Size);
				}

				sendOPENR(src);

				Header _header = new Header();
				_header = ReadHeader(buf, _header);
				int length = _header.length;
				int type = _header.type;
				buf.Rewind();
				fill(buf.buffer, 0, length);

				if (type != SSH_FXP_STATUS && type != SSH_FXP_HANDLE)
				{
					throw new SftpException(SSH_FX_FAILURE, "");
				}
				if (type == SSH_FXP_STATUS)
				{
					int i = buf.ReadInt();
					throwStatusError(buf, i);
				}

				byte[] handle = buf.ReadString();         // filename

				InputStream ins = new InputStreamGet(this, handle, monitor);
				//  long offset=0;
				//  bool closed=false;
				//  int rest_length=0;
				//  byte[] _data=new byte[1];
				//  public int read() { //throws java.io.IOException{
				//    int i=read(_data, 0, 1);
				//    if (i==-1) { return -1; }
				//    else {
				//      return _data[0]&0xff;
				//    }
				//  }
				//  public int read(byte[] d) { //throws java.io.IOException{
				//    return read(d, 0, d.Length);
				//  }
				//  public int read(byte[] d, int s, int len) { //throws java.io.IOException{
				//    if(d==null){throw new NullPointerException();}
				//    if(s<0 || len <0 || s+len>d.Length){
				//      throw new IndexOutOfBoundsException();
				//    }
				//    if(len==0){ return 0; }

				//    if(rest_length>0){
				//      int foo=rest_length;
				//      if(foo>len) foo=len;
				//      int i=io.ins.read(d, s, foo);
				//      if(i<0){
				//        throw new IOException("error");
				//      }
				//      rest_length-=i;
				//      return i;
				//    }

				//    if(buf.buffer.Length-13<len){
				//      len=buf.buffer.Length-13;
				//    }
				//    if(server_version==0 && len>1024){
				//      len=1024;
				//    }

				//    try{sendREAD(handle, offset, len);}
				//    catch(Exception e){ throw new IOException("error"); }

				//    buf.rewind();
				//    int i=io.ins.read(buf.buffer, 0, 13);  // 4 + 1 + 4 + 4
				//    if(i!=13){
				//      throw new IOException("error");
				//    }

				//    rest_length=buf.getInt();
				//    int type=buf.getByte();
				//    rest_length--;
				//    buf.getInt();
				//    rest_length-=4;
				//    if(type!=SSH_FXP_STATUS && type!=SSH_FXP_DATA){
				//      throw new IOException("error");
				//    }
				//    if(type==SSH_FXP_STATUS){
				//      i=buf.getInt();
				//      rest_length-=4;
				//      io.ins.read(buf.buffer, 13, rest_length);
				//      rest_length=0;
				//      if(i==SSH_FX_EOF){
				//        close();
				//        return -1;
				//      }
				//      //throwStatusError(buf, i);
				//      throw new IOException("error");
				//    }

				//    i=buf.getInt();
				//    rest_length-=4;
				//    offset+=rest_length;
				//    int foo=i;
				//    if(foo>0){
				//      int bar=rest_length;
				//      if(bar>len){
				//        bar=len;
				//      }
				//      i=io.ins.read(d, s, bar);
				//      if(i<0){
				//        return -1;
				//      }
				//      rest_length-=i;

				//      if(monitor!=null){
				//        if(!monitor.count(i)){
				//          return -1;
				//        }
				//      }
				//      return i;
				//    }
				//    return 0; // ??
				//  }
				//  public void close() { //throws IOException{
				//    if(closed)return;
				//    closed=true;
				//    /*
				//    while(rest_length>0){
				//      int foo=rest_length;
				//      if(foo>buf.buffer.Length){
				//        foo=buf.buffer.Length;
				//      }
				//      io.ins.read(buf.buffer, 0, foo);
				//      rest_length-=foo;
				//    }
				//    */
				//    if(monitor!=null)monitor.end();
				//    try{_sendCLOSE(handle);}
				//    catch(Exception e){throw new IOException("error");}
				//  }
				//};
				return ins;
			}
			catch (Exception e)
			{
				if (e is SftpException) throw (SftpException)e;
				throw new SftpException(SSH_FX_FAILURE, "");
			}
		}

		public List<LsEntry> ListEntries(String path)
		{ //throws SftpException{
			try
			{
				path = RemoteAbsolutePath(path);

				String dir = path;
				byte[] pattern = null;
				SftpATTRS attr = null;
				if (IsPattern(dir) ||
					((attr = stat(dir)) != null && !attr.IsDirectory))
				{
					int foo = path.LastIndexOf('/');
					dir = path.Substring(0, ((foo == 0) ? 1 : foo));
					pattern = Encoding.UTF8.GetBytes(path.Substring(foo + 1));
				}

				sendOPENDIR(Encoding.UTF8.GetBytes(dir));

				Header _header = new Header();
				_header = ReadHeader(buf, _header);
				int length = _header.length;
				int type = _header.type;
				buf.Rewind();
				fill(buf.buffer, 0, length);

				if (type != SSH_FXP_STATUS && type != SSH_FXP_HANDLE)
				{
					throw new SftpException(SSH_FX_FAILURE, "");
				}
				if (type == SSH_FXP_STATUS)
				{
					int i = buf.ReadInt();
					throwStatusError(buf, i);
				}

				byte[] handle = buf.ReadString();         // filename

				var List = new List<LsEntry>();
				while (true)
				{
					sendREADDIR(handle);

					_header = ReadHeader(buf, _header);
					length = _header.length;
					type = _header.type;
					if (type != SSH_FXP_STATUS && type != SSH_FXP_NAME)
					{
						throw new SftpException(SSH_FX_FAILURE, "");
					}
					if (type == SSH_FXP_STATUS)
					{
						buf.Rewind();
						fill(buf.buffer, 0, length);
						int i = buf.ReadInt();
						if (i == SSH_FX_EOF)
							break;
						throwStatusError(buf, i);
					}

					buf.Rewind();
					fill(buf.buffer, 0, 4); length -= 4;
					int count = buf.ReadInt();

					byte[] str;
					//int flags;

					buf.Reset();
					while (count > 0)
					{
						if (length > 0)
						{
							buf.Shift();
							int j = (buf.buffer.Length > (buf.index + length)) ? length : (buf.buffer.Length - buf.index);
							int i = fill(buf.buffer, buf.index, j);
							buf.index += i;
							length -= i;
						}
						byte[] filename = buf.ReadString();
						str = buf.ReadString();
						String longname = Encoding.UTF8.GetString(str);

						SftpATTRS attrs = SftpATTRS.getATTR(buf);
						if (pattern == null || Util.glob(pattern, filename))
						{
							List.Add(new LsEntry()
							{
								FileName = Encoding.UTF8.GetString(filename),
								LongName = longname,
								Attributes = attrs,
							});
						}

						count--;
					}
				}
				_sendCLOSE(handle, _header);
				return List;
			}
			catch (Exception e)
			{
				if (e is SftpException) throw (SftpException)e;
				throw new SftpException(SSH_FX_FAILURE, "");
			}
		}

		public String ReadLink(String path)
		{
			// throws SftpException{
			try
			{
				path = RemoteAbsolutePath(path);
				ArrayList v = glob_remote(path);
				if (v.Count != 1)
				{
					throw new SftpException(SSH_FX_FAILURE, v.ToString());
				}
				path = (String)(v[0]);

				sendREADLINK(path);

				Header _header = new Header();
				_header = ReadHeader(buf, _header);
				int length = _header.length;
				int type = _header.type;
				buf.Rewind();
				fill(buf.buffer, 0, length);

				if (type != SSH_FXP_STATUS && type != SSH_FXP_NAME)
				{
					throw new SftpException(SSH_FX_FAILURE, "");
				}
				int i;
				if (type == SSH_FXP_NAME)
				{
					int count = buf.ReadInt();       // count
					byte[] filename = null;
					byte[] longname = null;
					for (i = 0; i < count; i++)
					{
						filename = buf.ReadString();
						longname = buf.ReadString();
						SftpATTRS.getATTR(buf);
					}
					return Encoding.UTF8.GetString(filename);
				}

				i = buf.ReadInt();
				throwStatusError(buf, i);
			}
			catch (Exception e)
			{
				if (e is SftpException) throw (SftpException)e;
				throw new SftpException(SSH_FX_FAILURE, "");
			}
			return null;
		}


		public void symlink(String oldpath, String newpath)
		{
			//throws SftpException{
			if (ServerVersion < 3)
			{
				throw new SftpException(SSH_FX_FAILURE, "The remote sshd is too old to support symlink operation.");
			}

			try
			{
				oldpath = RemoteAbsolutePath(oldpath);
				newpath = RemoteAbsolutePath(newpath);

				ArrayList v = glob_remote(oldpath);
				int vsize = v.Count;
				if (vsize != 1)
				{
					throw new SftpException(SSH_FX_FAILURE, v.ToString());
				}
				oldpath = (String)(v[0]);

				if (IsPattern(newpath))
				{
					throw new SftpException(SSH_FX_FAILURE, v.ToString());
				}

				newpath = Util.unquote(newpath);

				sendSYMLINK(oldpath, newpath);

				Header _header = new Header();
				_header = ReadHeader(buf, _header);
				int length = _header.length;
				int type = _header.type;
				buf.Rewind();
				fill(buf.buffer, 0, length);

				if (type != SSH_FXP_STATUS)
				{
					throw new SftpException(SSH_FX_FAILURE, "");
				}

				int i = buf.ReadInt();
				if (i == SSH_FX_OK) return;
				throwStatusError(buf, i);
			}
			catch (Exception e)
			{
				if (e is SftpException) throw (SftpException)e;
				throw new SftpException(SSH_FX_FAILURE, "");
			}
		}

		public void Rename(String oldpath, String newpath)
		{
			//throws SftpException{
			if (ServerVersion < 2)
			{
				throw new SftpException(SSH_FX_FAILURE, "The remote sshd is too old to support rename operation.");
			}
			try
			{
				oldpath = RemoteAbsolutePath(oldpath);
				newpath = RemoteAbsolutePath(newpath);

				ArrayList v = glob_remote(oldpath);
				int vsize = v.Count;
				if (vsize != 1)
				{
					throw new SftpException(SSH_FX_FAILURE, v.ToString());
				}
				oldpath = (String)(v[0]);

				v = glob_remote(newpath);
				vsize = v.Count;
				if (vsize >= 2)
				{
					throw new SftpException(SSH_FX_FAILURE, v.ToString());
				}
				if (vsize == 1)
				{
					newpath = (String)(v[0]);
				}
				else
				{  // vsize==0
					if (IsPattern(newpath))
						throw new SftpException(SSH_FX_FAILURE, newpath);
					newpath = Util.unquote(newpath);
				}

				sendRENAME(oldpath, newpath);

				Header _header = new Header();
				_header = ReadHeader(buf, _header);
				int length = _header.length;
				int type = _header.type;
				buf.Rewind();
				fill(buf.buffer, 0, length);

				if (type != SSH_FXP_STATUS)
				{
					throw new SftpException(SSH_FX_FAILURE, "");
				}

				int i = buf.ReadInt();
				if (i == SSH_FX_OK) return;
				throwStatusError(buf, i);
			}
			catch (Exception e)
			{
				if (e is SftpException) throw (SftpException)e;
				throw new SftpException(SSH_FX_FAILURE, "");
			}
		}
		public void rm(String path)
		{
			//throws SftpException{
			try
			{
				path = RemoteAbsolutePath(path);
				ArrayList v = glob_remote(path);
				int vsize = v.Count;
				Header _header = new Header();

				for (int j = 0; j < vsize; j++)
				{
					path = (String)(v[j]);
					sendREMOVE(path);

					_header = ReadHeader(buf, _header);
					int length = _header.length;
					int type = _header.type;
					buf.Rewind();
					fill(buf.buffer, 0, length);

					if (type != SSH_FXP_STATUS)
					{
						throw new SftpException(SSH_FX_FAILURE, "");
					}
					int i = buf.ReadInt();
					if (i != SSH_FX_OK)
					{
						throwStatusError(buf, i);
					}
				}
			}
			catch (Exception e)
			{
				if (e is SftpException) throw (SftpException)e;
				throw new SftpException(SSH_FX_FAILURE, "");
			}
		}
		private bool isRemoteDir(String path)
		{
			try
			{
				sendSTAT(path);

				Header _header = new Header();
				_header = ReadHeader(buf, _header);
				int length = _header.length;
				int type = _header.type;
				buf.Rewind();
				fill(buf.buffer, 0, length);

				if (type != SSH_FXP_ATTRS)
				{
					return false;
				}
				return SftpATTRS.getATTR(buf).IsDirectory;
			}
			catch (Exception) { }
			return false;
		}
		/*
		bool isRemoteDir(String path) { //throws SftpException{
		SftpATTRS attr=stat(path);
		return attr.isDir();
		}
		*/
		public void chgrp(int gid, String path)
		{ //throws SftpException{
			try
			{
				path = RemoteAbsolutePath(path);

				ArrayList v = glob_remote(path);
				int vsize = v.Count;
				for (int j = 0; j < vsize; j++)
				{
					path = (String)(v[j]);

					SftpATTRS attr = _stat(path);

					attr.Flags = 0;
					attr.setUIDGID(attr.uid, gid);
					_setStat(path, attr);
				}
			}
			catch (Exception e)
			{
				if (e is SftpException) throw (SftpException)e;
				throw new SftpException(SSH_FX_FAILURE, "");
			}
		}
		public void chown(int uid, String path)
		{ //throws SftpException{
			try
			{
				path = RemoteAbsolutePath(path);

				ArrayList v = glob_remote(path);
				int vsize = v.Count;
				for (int j = 0; j < vsize; j++)
				{
					path = (String)(v[j]);

					SftpATTRS attr = _stat(path);

					attr.Flags = 0;
					attr.setUIDGID(uid, attr.gid);
					_setStat(path, attr);
				}
			}
			catch (Exception e)
			{
				if (e is SftpException) throw (SftpException)e;
				throw new SftpException(SSH_FX_FAILURE, "");
			}
		}
		public void chmod(SftpATTRS.PermissionFlags permissions, String path)
		{ //throws SftpException{
			try
			{
				path = RemoteAbsolutePath(path);

				ArrayList v = glob_remote(path);
				int vsize = v.Count;
				for (int j = 0; j < vsize; j++)
				{
					path = (String)(v[j]);

					SftpATTRS attr = _stat(path);

					attr.Flags = 0;
					attr.Permissions = permissions;
					_setStat(path, attr);
				}
			}
			catch (Exception e)
			{
				if (e is SftpException) throw (SftpException)e;
				throw new SftpException(SSH_FX_FAILURE, "");
			}
		}
		public void setMtime(String path, int mtime)
		{ //throws SftpException{
			try
			{
				path = RemoteAbsolutePath(path);

				ArrayList v = glob_remote(path);
				int vsize = v.Count;
				for (int j = 0; j < vsize; j++)
				{
					path = (String)(v[j]);

					SftpATTRS attr = _stat(path);

					attr.Flags = 0;
					attr.setACMODTIME(attr.getATime(), mtime);
					_setStat(path, attr);
				}
			}
			catch (Exception e)
			{
				if (e is SftpException) throw (SftpException)e;
				throw new SftpException(SSH_FX_FAILURE, "");
			}
		}
		public void rmdir(String path)
		{
			//throws SftpException{
			try
			{
				path = RemoteAbsolutePath(path);

				ArrayList v = glob_remote(path);
				int vsize = v.Count;
				Header _header = new Header();

				for (int j = 0; j < vsize; j++)
				{
					path = (String)(v[j]);
					sendRMDIR(path);

					_header = ReadHeader(buf, _header);
					int length = _header.length;
					int type = _header.type;
					buf.Rewind();
					fill(buf.buffer, 0, length);

					if (type != SSH_FXP_STATUS)
					{
						throw new SftpException(SSH_FX_FAILURE, "");
					}

					int i = buf.ReadInt();
					if (i != SSH_FX_OK)
					{
						throwStatusError(buf, i);
					}
				}
			}
			catch (Exception e)
			{
				if (e is SftpException) throw (SftpException)e;
				throw new SftpException(SSH_FX_FAILURE, "");
			}
		}

		public void mkdir(String path)
		{
			//throws SftpException{
			try
			{
				path = RemoteAbsolutePath(path);

				sendMKDIR(path, null);

				Header _header = new Header();
				_header = ReadHeader(buf, _header);
				int length = _header.length;
				int type = _header.type;
				buf.Rewind();
				fill(buf.buffer, 0, length);

				if (type != SSH_FXP_STATUS)
				{
					throw new SftpException(SSH_FX_FAILURE, "");
				}

				int i = buf.ReadInt();
				if (i == SSH_FX_OK) return;
				throwStatusError(buf, i);
			}
			catch (Exception e)
			{
				if (e is SftpException) throw (SftpException)e;
				throw new SftpException(SSH_FX_FAILURE, "");
			}
		}

		public SftpATTRS stat(String path)
		{
			//throws SftpException{
			try
			{
				path = RemoteAbsolutePath(path);

				ArrayList v = glob_remote(path);
				if (v.Count != 1)
				{
					throw new SftpException(SSH_FX_FAILURE, v.ToString());
				}
				path = (String)(v[0]);
				return _stat(path);
			}
			catch (Exception e)
			{
				if (e is SftpException) throw (SftpException)e;
				throw new SftpException(SSH_FX_FAILURE, "");
			}
			//return null;
		}
		public SftpATTRS lstat(String path)
		{
			//throws SftpException{
			try
			{
				path = RemoteAbsolutePath(path);

				ArrayList v = glob_remote(path);
				if (v.Count != 1)
				{
					throw new SftpException(SSH_FX_FAILURE, v.ToString());
				}
				path = (String)(v[0]);

				return _lstat(path);
			}
			catch (Exception e)
			{
				if (e is SftpException) throw (SftpException)e;
				throw new SftpException(SSH_FX_FAILURE, "");
			}
		}

		private SftpATTRS _lstat(String path)
		{
			//throws SftpException{
			try
			{
				sendLSTAT(path);

				Header _header = new Header();
				_header = ReadHeader(buf, _header);
				int length = _header.length;
				int type = _header.type;
				buf.Rewind();
				fill(buf.buffer, 0, length);

				if (type != SSH_FXP_ATTRS)
				{
					if (type == SSH_FXP_STATUS)
					{
						int i = buf.ReadInt();
						throwStatusError(buf, i);
					}
					throw new SftpException(SSH_FX_FAILURE, "");
				}
				SftpATTRS attr = SftpATTRS.getATTR(buf);
				return attr;
			}
			catch (Exception e)
			{
				if (e is SftpException) throw (SftpException)e;
				throw new SftpException(SSH_FX_FAILURE, "");
			}
		}


		public void setStat(String path, SftpATTRS attr)
		{ //throws SftpException{
			try
			{
				path = RemoteAbsolutePath(path);

				ArrayList v = glob_remote(path);
				int vsize = v.Count;
				for (int j = 0; j < vsize; j++)
				{
					path = (String)(v[j]);
					_setStat(path, attr);
				}
			}
			catch (Exception e)
			{
				if (e is SftpException) throw (SftpException)e;
				throw new SftpException(SSH_FX_FAILURE, "");
			}
		}
		private void _setStat(String path, SftpATTRS attr)
		{
			//throws SftpException{
			try
			{
				sendSETSTAT(path, attr);

				Header _header = new Header();
				_header = ReadHeader(buf, _header);
				int length = _header.length;
				int type = _header.type;
				buf.Rewind();
				fill(buf.buffer, 0, length);

				if (type != SSH_FXP_STATUS)
				{
					throw new SftpException(SSH_FX_FAILURE, "");
				}
				int i = buf.ReadInt();
				if (i != SSH_FX_OK)
				{
					throwStatusError(buf, i);
				}
			}
			catch (Exception e)
			{
				if (e is SftpException) throw (SftpException)e;
				throw new SftpException(SSH_FX_FAILURE, "");
			}
		}

		public String pwd() { return cwd; }
		public String lpwd() { return lcwd; }
		public String version() { return _version; }
		public String getHome() { return home; }
	}
}
