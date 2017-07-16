using System;
using System.Runtime.CompilerServices;
using Tamir.Streams;
using System.Text;
using CSharpUtils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tamir.SharpSsh.jsch;
using System.Threading;

namespace Tamir.SharpSsh.jsch
{
    /* -*-mode:java; c-basic-offset:2; indent-tabs-mode:nil -*- */
    /*
    Copyright (c) 2002,2003,2004,2005,2006 ymnk, JCraft,Inc. All rights reserved.

    Redistribution and use in source and binary forms, with or without
    modification, are permitted provided that the following conditions are met:

    1. Redistributions of source code must retain the above copyright notice,
    this list of conditions and the following disclaimer.

    2. Redistributions in binary form must reproduce the above copyright
    notice, this list of conditions and the following disclaimer in
    the documentation and/or other materials provided with the distribution.

    3. The names of the authors may not be used to endorse or promote products
    derived from this software without specific prior written permission.

    THIS SOFTWARE IS PROVIDED ``AS IS'' AND ANY EXPRESSED OR IMPLIED WARRANTIES,
    INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
    FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL JCRAFT,
    INC. OR ANY CONTRIBUTORS TO THIS SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT,
    INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
    LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,
    OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
    LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
    NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
    EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
    */

    /// <summary>
    /// Based on JSch-0.1.30
    /// </summary>
    public partial class ChannelSftp : ChannelSession
    {
        private static byte SSH_FXP_INIT = 1;
        //private static byte SSH_FXP_VERSION = 2;
        private static byte SSH_FXP_OPEN = 3;
        private static byte SSH_FXP_CLOSE = 4;
        private static byte SSH_FXP_READ = 5;
        private static byte SSH_FXP_WRITE = 6;
        private static byte SSH_FXP_LSTAT = 7;
        private static byte SSH_FXP_FSTAT = 8;
        private static byte SSH_FXP_SETSTAT = 9;
        //private static byte SSH_FXP_FSETSTAT = 10;
        private static byte SSH_FXP_OPENDIR = 11;
        private static byte SSH_FXP_READDIR = 12;
        private static byte SSH_FXP_REMOVE = 13;
        private static byte SSH_FXP_MKDIR = 14;
        private static byte SSH_FXP_RMDIR = 15;
        private static byte SSH_FXP_REALPATH = 16;
        private static byte SSH_FXP_STAT = 17;
        private static byte SSH_FXP_RENAME = 18;
        private static byte SSH_FXP_READLINK = 19;
        private static byte SSH_FXP_SYMLINK = 20;
        private static byte SSH_FXP_STATUS = 101;
        private static byte SSH_FXP_HANDLE = 102;
        private static byte SSH_FXP_DATA = 103;
        private static byte SSH_FXP_NAME = 104;
        private static byte SSH_FXP_ATTRS = 105;
        //private static byte SSH_FXP_EXTENDED = (byte)200;
        //private static byte SSH_FXP_EXTENDED_REPLY = (byte)201;

        // pflags
        private static int SSH_FXF_READ = 0x00000001;
        private static int SSH_FXF_WRITE = 0x00000002;
        //private static  int SSH_FXF_APPEND=         0x00000004;
        private static int SSH_FXF_CREAT = 0x00000008;
        private static int SSH_FXF_TRUNC = 0x00000010;
        //private static int SSH_FXF_EXCL = 0x00000020;

        //private static  int SSH_FILEXFER_ATTR_SIZE=         0x00000001;
        //private static  int SSH_FILEXFER_ATTR_UIDGID=       0x00000002;
        //private static  int SSH_FILEXFER_ATTR_PERMISSIONS=  0x00000004;
        //private static  int SSH_FILEXFER_ATTR_ACMODTIME=    0x00000008;
        //private static  uint SSH_FILEXFER_ATTR_EXTENDED=     0x80000000;

        public static int SSH_FX_OK = 0;
        public static int SSH_FX_EOF = 1;
        public static int SSH_FX_NO_SUCH_FILE = 2;
        public static int SSH_FX_PERMISSION_DENIED = 3;
        public static int SSH_FX_FAILURE = 4;
        public static int SSH_FX_BAD_MESSAGE = 5;
        public static int SSH_FX_NO_CONNECTION = 6;
        public static int SSH_FX_CONNECTION_LOST = 7;
        public static int SSH_FX_OP_UNSUPPORTED = 8;
        /*
        SSH_FX_OK
        Indicates successful completion of the operation.
        SSH_FX_EOF
        indicates end-of-file condition; for SSH_FX_READ it means that no
        more data is available in the file, and for SSH_FX_READDIR it
        indicates that no more files are contained in the directory.
        SSH_FX_NO_SUCH_FILE
        is returned when a reference is made to a file which should exist
        but doesn't.
        SSH_FX_PERMISSION_DENIED
        is returned when the authenticated user does not have sufficient
        permissions to perform the operation.
        SSH_FX_FAILURE
        is a generic catch-all error message; it should be returned if an
        error occurs for which there is no more specific error code
        defined.
        SSH_FX_BAD_MESSAGE
        may be returned if a badly formatted packet or protocol
        incompatibility is detected.
        SSH_FX_NO_CONNECTION
        is a pseudo-error which indicates that the client has no
        connection to the server (it can only be generated locally by the
        client, and MUST NOT be returned by servers).
        SSH_FX_CONNECTION_LOST
        is a pseudo-error which indicates that the connection to the
        server has been lost (it can only be generated locally by the
        client, and MUST NOT be returned by servers).
        SSH_FX_OP_UNSUPPORTED
        indicates that an attempt was made to perform an operation which
        is not supported for the server (it may be generated locally by
        the client if e.g.  the version number exchange indicates that a
        required feature is not supported by the server, or it may be
        returned by the server if the server does not implement an
        operation).
        */
        private static int MAX_MSG_LENGTH = 256 * 1024;

        public const int OVERWRITE = 0;
        public const int RESUME = 1;
        public const int APPEND = 2;

        //  private bool interactive=true;
        //private bool interactive=false;
        internal int seq = 1;
        private int[] ackid = new int[1];
        private Buffer buf;
        private Packet packet;//=new Packet(buf);

        private String _version = "3";
        private int ServerVersion = 3;
        /*
        10. Changes from previous protocol versions
        The SSH File Transfer Protocol has changed over time, before it's
        standardization.  The following is a description of the incompatible
        changes between different versions.
        10.1 Changes between versions 3 and 2
        o  The SSH_FXP_READLINK and SSH_FXP_SYMLINK messages were added.
        o  The SSH_FXP_EXTENDED and SSH_FXP_EXTENDED_REPLY messages were added.
        o  The SSH_FXP_STATUS message was changed to include fields `error
        message' and `language tag'.
        10.2 Changes between versions 2 and 1
        o  The SSH_FXP_RENAME message was added.
        10.3 Changes between versions 1 and 0
        o  Implementation changes, no actual protocol changes.
        */

        private static String file_separator = System.IO.Path.DirectorySeparatorChar.ToString();
        private static char file_separatorc = System.IO.Path.DirectorySeparatorChar;

        private String cwd;
        private String home;
        private String lcwd;

        internal ChannelSftp() { packet = new Packet(buf); }

        public override void init()
        {
            /*
            io.setInputStream(session.in);
            io.setOutputStream(session.out);
            */
        }

        public override void start()
        { //throws JSchException{
            try
            {

                PipedOutputStream pos = new PipedOutputStream();
                io.setOutputStream(pos);
                PipedInputStream pis = new MyPipedInputStream(pos, 32 * 1024);
                io.setInputStream(pis);

                Request request = new RequestSftp();
                request.request(session, this);

                /*
                      System.err.println("lmpsize: "+lmpsize);
                      System.err.println("lwsize: "+lwsize);
                      System.err.println("rmpsize: "+rmpsize);
                      System.err.println("rwsize: "+rwsize);
                */

                buf = new Buffer(rmpsize);
                packet = new Packet(buf);
                int i = 0;
                int length;
                int type;
                byte[] str;

                // send SSH_FXP_INIT
                sendINIT();

                // receive SSH_FXP_VERSION
                Header _header = new Header();
                _header = ReadHeader(buf, _header);
                length = _header.length;
                if (length > MAX_MSG_LENGTH)
                {
                    throw new SftpException(SSH_FX_FAILURE, "Received message is too long: " + length);
                }
                type = _header.type;             // 2 -> SSH_FXP_VERSION
                ServerVersion = _header.rid;
                skip(length);
                //System.err.println("SFTP protocol server-version="+server_version);
                //System.Console.WriteLine("server_version="+server_version+", type="+type+", length="+length+", i="+i);

                // send SSH_FXP_REALPATH
                sendREALPATH(".");

                // receive SSH_FXP_NAME
                _header = ReadHeader(buf, _header);
                length = _header.length;
                type = _header.type;            // 104 -> SSH_FXP_NAME
                buf.Rewind();
                fill(buf.buffer, 0, length);
                i = buf.ReadInt();              // count
                //System.Console.WriteLine("type="+type+", length="+length+", i="+i);
                str = buf.ReadString();         // filename
                //System.Console.WriteLine("str.length="+str.Length);
                home = cwd = Encoding.UTF8.GetString(str);
                str = buf.ReadString();         // logname
                //    SftpATTRS.getATTR(buf);      // attrs

                lcwd = System.IO.Path.GetFullPath(".");
            }
            catch (Exception e)
            {
                //System.out.println(e);
                //System.Console.WriteLine(e);
                if (e is JSchException) throw (JSchException)e;
                throw new JSchException(e.ToString());
            }
        }

        private void read(byte[] buf, int s, int l)
        { //throws IOException, SftpException{
            int i = 0;
            while (l > 0)
            {
                i = io.ins.read(buf, s, l);
                if (i <= 0)
                {
                    throw new SftpException(SSH_FX_FAILURE, "");
                }
                s += i;
                l -= i;
            }
        }
        internal bool checkStatus(int[] ackid, Header _header)
        { //throws IOException, SftpException{
            _header = ReadHeader(buf, _header);
            int length = _header.length;
            int type = _header.type;
            if (ackid != null)
                ackid[0] = _header.rid;
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
            return true;
        }

        internal bool _sendCLOSE(byte[] handle, Header header)
        {
            sendCLOSE(handle);
            return checkStatus(null, header);
        }

        private void sendINIT()
        {
            packet.reset();
            putHEAD(SSH_FXP_INIT, 5);
            // http://tools.ietf.org/html/draft-ietf-secsh-filexfer-02
            buf.WriteInt(3);                // version 3
            session.write(packet, this, 5 + 4);
        }

        private void sendREALPATH(String path)
        {
            sendPacketPath(SSH_FXP_REALPATH, path);
        }
        private void sendSTAT(String path)
        {
            sendPacketPath(SSH_FXP_STAT, path);
        }
        private void sendLSTAT(String path)
        {
            sendPacketPath(SSH_FXP_LSTAT, path);
        }
        private void sendFSTAT(byte[] handle)
        {
            sendPacketPath(SSH_FXP_FSTAT, handle);
        }
        private void sendSETSTAT(String path, SftpATTRS attr)
        {
            packet.reset();
            putHEAD(SSH_FXP_SETSTAT, 9 + path.Length + attr.Length());
            buf.WriteInt(seq++);
            buf.WriteString(path);             // path
            attr.dump(buf);
            session.write(packet, this, 9 + path.Length + attr.Length() + 4);
        }
        private void sendREMOVE(String path)
        {
            sendPacketPath(SSH_FXP_REMOVE, path);
        }
        private void sendMKDIR(String path, SftpATTRS attr)
        {
            packet.reset();
            putHEAD(SSH_FXP_MKDIR, 9 + path.Length + (attr != null ? attr.Length() : 4));
            buf.WriteInt(seq++);
            buf.WriteString(path);             // path
            if (attr != null) attr.dump(buf);
            else buf.WriteInt(0);
            session.write(packet, this, 9 + path.Length + (attr != null ? attr.Length() : 4) + 4);
        }
        private void sendRMDIR(String path) { sendPacketPath(SSH_FXP_RMDIR, path); }
        private void sendSYMLINK(String p1, String p2) { sendPacketPath(SSH_FXP_SYMLINK, p1, p2); }
        private void sendREADLINK(String path) { sendPacketPath(SSH_FXP_READLINK, path); }
        private void sendOPENDIR(String path) { sendPacketPath(SSH_FXP_OPENDIR, path); }
        private void sendOPENDIR(byte[] path) { sendPacketPath(SSH_FXP_OPENDIR, path); }
        private void sendREADDIR(byte[] path) { sendPacketPath(SSH_FXP_READDIR, path); }
        private void sendRENAME(String p1, String p2) { sendPacketPath(SSH_FXP_RENAME, p1, p2); }
        private void sendCLOSE(byte[] path) { sendPacketPath(SSH_FXP_CLOSE, path); }
        private void sendOPENR(String path) { sendOPEN(path, SSH_FXF_READ); }
        private void sendOPENW(String path) { sendOPEN(path, SSH_FXF_WRITE | SSH_FXF_CREAT | SSH_FXF_TRUNC); }
        private void sendOPENA(String path) { sendOPEN(path, SSH_FXF_WRITE |/*SSH_FXF_APPEND|*/SSH_FXF_CREAT); }
        private void sendOPEN(String path, int mode)
        {
            packet.reset();
            putHEAD(SSH_FXP_OPEN, 17 + path.Length);
            buf.WriteInt(seq++);
            buf.WriteString(path);
            buf.WriteInt(mode);
            buf.WriteInt(0);           // attrs
            session.write(packet, this, 17 + path.Length + 4);
        }

        private void sendPacketPath(byte fxp, String path) { sendPacketPath(fxp, Encoding.UTF8.GetBytes(path)); }
        private void sendPacketPath(byte fxp, String p1, String p2) { sendPacketPath(fxp, Encoding.UTF8.GetBytes(p1), Encoding.UTF8.GetBytes(p2)); }

        private void sendPacketPath(byte fxp, byte[] path)
        {
            packet.reset();
            putHEAD(fxp, 9 + path.Length);
            buf.WriteInt(seq++);
            buf.WriteString(path);             // path
            session.write(packet, this, 9 + path.Length + 4);
        }
        private void sendPacketPath(byte fxp, byte[] p1, byte[] p2)
        {
            packet.reset();
            putHEAD(fxp, 13 + p1.Length + p2.Length);
            buf.WriteInt(seq++);
            buf.WriteString(p1);
            buf.WriteString(p2);
            session.write(packet, this, 13 + p1.Length + p2.Length + 4);
        }

        internal int sendWRITE(byte[] handle, long offset, byte[] data, int start, int length)
        {
            int _length = length;
            packet.reset();
            if (buf.buffer.Length < buf.index + 13 + 21 + handle.Length + length
                + 32 + 20  // padding and mac
                )
            {
                _length = buf.buffer.Length - (buf.index + 13 + 21 + handle.Length
                    + 32 + 20  // padding and mac
                    );
                //System.err.println("_length="+_length+" length="+length);
            }
            putHEAD(SSH_FXP_WRITE, 21 + handle.Length + _length);       // 14
            buf.WriteInt(seq++);                                    //  4
            buf.WriteString(handle);                                  //  4+handle.length
            buf.WriteLong(offset);                                    //  8
            if (buf.buffer != data)
            {
                buf.WriteString(data, start, _length);                    //  4+_length
            }
            else
            {
                buf.WriteInt(_length);
                buf.Skip(_length);
            }
            session.write(packet, this, 21 + handle.Length + _length + 4);
            return _length;
        }

        private void sendREAD(byte[] handle, long offset, int length)
        {
            packet.reset();
            putHEAD(SSH_FXP_READ, 21 + handle.Length);
            buf.WriteInt(seq++);
            buf.WriteString(handle);
            buf.WriteLong(offset);
            buf.WriteInt(length);
            session.write(packet, this, 21 + handle.Length + 4);
        }

        private void putHEAD(byte type, int length)
        {
            buf.WriteByte((byte)Session.SSH_MSG_CHANNEL_DATA);
            buf.WriteInt(recipient);
            buf.WriteInt(length + 4);
            buf.WriteInt(length);
            buf.WriteByte(type);
        }
        private ArrayList glob_remote(String _path)
        {
            //System.err.println("glob_remote: "+_path);
            ArrayList v = new ArrayList();
            byte[] path = Encoding.UTF8.GetBytes(_path);
            if (!IsPattern(path))
            {
                v.Add(Util.unquote(_path)); return v;
            }
            int i = path.Length - 1;
            while (i >= 0) { if (path[i] == '/')break; i--; }
            if (i < 0) { v.Add(Util.unquote(_path)); return v; }
            byte[] dir;
            if (i == 0) { dir = new byte[] { (byte)'/' }; }
            else
            {
                dir = new byte[i];
                System.Array.Copy(path, 0, dir, 0, i);
            }
            //System.err.println("dir: "+Encoding.UTF8.GetString(dir));
            byte[] pattern = new byte[path.Length - i - 1];
            System.Array.Copy(path, i + 1, pattern, 0, pattern.Length);
            //System.err.println("file: "+Encoding.UTF8.GetString(pattern));

            sendOPENDIR(dir);

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
                i = buf.ReadInt();
                throwStatusError(buf, i);
            }

            byte[] handle = buf.ReadString();         // filename

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
                    break;
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
                        i = io.ins.read(buf.buffer, buf.index, j);
                        if (i <= 0) break;
                        buf.index += i;
                        length -= i;
                    }

                    byte[] filename = buf.ReadString();
                    //System.err.println("filename: "+Encoding.UTF8.GetString(filename));
                    str = buf.ReadString();
                    SftpATTRS attrs = SftpATTRS.getATTR(buf);

                    if (Util.glob(pattern, filename))
                    {
                        v.Add(Encoding.UTF8.GetString(dir) + "/" + Encoding.UTF8.GetString(filename));
                    }
                    count--;
                }
            }
            if (_sendCLOSE(handle, _header))
                return v;
            return null;
        }

        private ArrayList glob_local(String _path)
        {
            //System.out.println("glob_local: "+_path);
            ArrayList v = new ArrayList();
            byte[] path = Encoding.UTF8.GetBytes(_path);
            int i = path.Length - 1;
            while (i >= 0) { if (path[i] == '*' || path[i] == '?')break; i--; }
            if (i < 0) { v.Add(_path); return v; }
            while (i >= 0) { if (path[i] == file_separatorc)break; i--; }
            if (i < 0) { v.Add(_path); return v; }
            byte[] dir;
            if (i == 0) { dir = new byte[] { (byte)file_separatorc }; }
            else
            {
                dir = new byte[i];
                System.Array.Copy(path, 0, dir, 0, i);
            }
            byte[] pattern = new byte[path.Length - i - 1];
            System.Array.Copy(path, i + 1, pattern, 0, pattern.Length);
            //System.out.println("dir: "+Encoding.UTF8.GetString(dir)+" pattern: "+Encoding.UTF8.GetString(pattern));
            try
            {
                var children = System.IO.Directory.GetFileSystemEntries(dir.GetString(Encoding.UTF8));
                foreach (var child in children)
                {
                    //System.out.println("children: "+children[j]);
                    if (Util.glob(pattern, Encoding.UTF8.GetBytes(child)))
                    {
                        v.Add(Encoding.UTF8.GetString(dir) + file_separator + child);
                    }
                }
            }
            catch (Exception)
            {
            }
            return v;
        }

        private void throwStatusError(Buffer buf, int i)
        {
            if (ServerVersion >= 3)
            {
                byte[] str = buf.ReadString();
                //byte[] tag=buf.getString();
                throw new SftpException(i, Encoding.UTF8.GetString(str));
            }
            else
            {
                throw new SftpException(i, "Failure");
            }
        }

        private static bool isLocalAbsolutePath(String path)
        {
            return System.IO.Path.IsPathRooted(path);
        }

        /*
        public void finalize() { //throws Throwable{
        base.finalize();
        }
        */

        public override void disconnect()
        {
            //waitForRunningThreadFinish(10000);
            clearRunningThreads();
            base.disconnect();
        }
        private List<Thread> ThreadList = null;

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected void addRunningThread(System.Threading.Thread thread)
        {
            if (ThreadList == null) ThreadList = new List<Thread>();
            ThreadList.Add(thread);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected void clearRunningThreads()
        {
            if (ThreadList == null) return;
            foreach (var Thread in ThreadList.Where((Thread) => Thread != null && Thread.IsAlive)) Thread.Interrupt();
            ThreadList.Clear();
        }
        private bool IsPattern(String path)
        {
            return path.IndexOf("*") != -1 || path.IndexOf("?") != -1;
        }
        private bool IsPattern(byte[] path)
        {
            int i = path.Length - 1;
            while (i >= 0)
            {
                if (path[i] == '*' || path[i] == '?')
                {
                    if (i > 0 && path[i - 1] == '\\')
                    {
                        i--;
                    }
                    else
                    {
                        break;
                    }
                }
                i--;
            }
            //System.err.println("isPattern: ["+(Encoding.UTF8.GetString(path))+"] "+(!(i<0)));
            return !(i < 0);
        }

        private int fill(byte[] buf, int s, int len)
        {
            int i = 0;
            int foo = s;
            while (len > 0)
            {
                i = io.ins.read(buf, s, len);
                if (i <= 0)
                {
                    throw new System.IO.IOException("inputstream is closed");
                    //return (s-foo)==0 ? i : s-foo;
                }
                s += i;
                len -= i;
            }
            return s - foo;
        }

        //tamir: some functions from jsch-0.1.30
        private void skip(long foo)
        {
            while (foo > 0)
            {
                long bar = io.ins.skip(foo);
                if (bar <= 0)
                    break;
                foo -= bar;
            }
        }

        internal class Header
        {
            public int length;
            public int type;
            public int rid;
        }
        internal Header ReadHeader(Buffer buf, Header header)
        {
            buf.Rewind();
            int i = fill(buf.buffer, 0, 9);
            header.length = buf.ReadInt() - 5;
            header.type = buf.ReadByte() & 0xff;
            header.rid = buf.ReadInt();
            return header;
        }

        private String RemoteAbsolutePath(String path)
        {
            if (path[0] == '/') return path;
            if (cwd.EndsWith("/")) return cwd + path;
            return cwd + "/" + path;
        }

        private String localAbsolutePath(String path)
        {
            if (isLocalAbsolutePath(path)) return path;
            if (lcwd.EndsWith(file_separator)) return lcwd + path;
            return lcwd + file_separator + path;
        }

        public class LsEntry
        {
            public String FileName;
            public String LongName;
            public SftpATTRS Attributes;

            /*
            internal LsEntry(String FileName, String LongName, SftpATTRS Attributes)
            {
                this.FileName = FileName;
                this.LongName = LongName;
                this.Attributes = Attributes;
            }
            */

            public override string ToString() { return LongName; }
        }

        public class InputStreamGet : InputStream
        {
            ChannelSftp sftp;
            SftpProgressMonitor monitor;
            long offset = 0;
            bool closed = false;
            int rest_length = 0;
            byte[] _data = new byte[1];
            byte[] rest_byte = new byte[1024];
            byte[] handle;
            Header header = new Header();

            public InputStreamGet(
                ChannelSftp sftp,
                byte[] handle,
                SftpProgressMonitor monitor)
            {
                this.sftp = sftp;
                this.handle = handle;
                this.monitor = monitor;
            }

            public override int ReadByte()
            {
                if (closed) return -1;
                int i = read(_data, 0, 1);
                if (i == -1) { return -1; }
                else
                {
                    return _data[0] & 0xff;
                }
            }
            public int Read(byte[] d)
            {
                if (closed) return -1;
                return Read(d, 0, d.Length);
            }
            public override int Read(byte[] d, int s, int len)
            {
                if (closed) return -1;
                int i;
                int foo;
                if (d == null) { throw new System.NullReferenceException(); }
                if (s < 0 || len < 0 || s + len > d.Length)
                {
                    throw new System.IndexOutOfRangeException();
                }
                if (len == 0) { return 0; }

                if (rest_length > 0)
                {
                    foo = rest_length;
                    if (foo > len) foo = len;
                    System.Array.Copy(rest_byte, 0, d, s, foo);
                    if (foo != rest_length)
                    {
                        System.Array.Copy(rest_byte, foo, rest_byte, 0, rest_length - foo);
                    }
                    if (monitor != null)
                    {
                        if (!monitor.count(foo))
                        {
                            close();
                            return -1;
                        }
                    }

                    rest_length -= foo;
                    return foo;
                }

                if (sftp.buf.buffer.Length - 13 < len)
                {
                    len = sftp.buf.buffer.Length - 13;
                }
                if (sftp.ServerVersion == 0 && len > 1024)
                {
                    len = 1024;
                }

                try { sftp.sendREAD(handle, offset, len); }
                catch (Exception) { throw new System.IO.IOException("error"); }

                header = sftp.ReadHeader(sftp.buf, header);
                rest_length = header.length;
                int type = header.type;
                int id = header.rid;

                if (type != SSH_FXP_STATUS && type != SSH_FXP_DATA)
                {
                    throw new System.IO.IOException("error");
                }
                if (type == SSH_FXP_STATUS)
                {
                    sftp.buf.Rewind();
                    sftp.fill(sftp.buf.buffer, 0, rest_length);
                    i = sftp.buf.ReadInt();
                    rest_length = 0;
                    if (i == SSH_FX_EOF)
                    {
                        close();
                        return -1;
                    }
                    //throwStatusError(buf, i);
                    throw new System.IO.IOException("error");
                }
                sftp.buf.Rewind();
                sftp.fill(sftp.buf.buffer, 0, 4);
                i = sftp.buf.ReadInt(); rest_length -= 4;

                offset += rest_length;
                foo = i;
                if (foo > 0)
                {
                    int bar = rest_length;
                    if (bar > len)
                    {
                        bar = len;
                    }
                    i = sftp.io.ins.read(d, s, bar);
                    if (i < 0)
                    {
                        return -1;
                    }
                    rest_length -= i;

                    if (rest_length > 0)
                    {
                        if (rest_byte.Length < rest_length)
                        {
                            rest_byte = new byte[rest_length];
                        }
                        int _s = 0;
                        int _len = rest_length;
                        int j;
                        while (_len > 0)
                        {
                            j = sftp.io.ins.read(rest_byte, _s, _len);
                            if (j <= 0) break;
                            _s += j;
                            _len -= j;
                        }
                    }

                    if (monitor != null)
                    {
                        if (!monitor.count(i))
                        {
                            close();
                            return -1;
                        }
                    }
                    return i;
                }
                return 0; // ??
            }
            public override void Close()
            {
                if (closed) return;
                closed = true;
                if (monitor != null) monitor.end();
                try { sftp._sendCLOSE(handle, header); }
                catch (Exception) { throw new System.IO.IOException("error"); }
            }
        }
    }
}
