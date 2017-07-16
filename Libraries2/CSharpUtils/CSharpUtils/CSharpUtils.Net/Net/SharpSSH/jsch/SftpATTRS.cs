using System;
using System.Text;

namespace Tamir.SharpSsh.jsch
{
    /* -*-mode:java; c-basic-offset:2; -*- */
    /*
    Copyright (c) 2002,2003,2004 ymnk, JCraft,Inc. All rights reserved.

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

    /*
      uint32   flags
      uint64   size           present only if flag SSH_FILEXFER_ATTR_SIZE
      uint32   uid            present only if flag SSH_FILEXFER_ATTR_UIDGID
      uint32   gid            present only if flag SSH_FILEXFER_ATTR_UIDGID
      uint32   permissions    present only if flag SSH_FILEXFER_ATTR_PERMISSIONS
      uint32   atime          present only if flag SSH_FILEXFER_ACMODTIME
      uint32   mtime          present only if flag SSH_FILEXFER_ACMODTIME
      uint32   extended_count present only if flag SSH_FILEXFER_ATTR_EXTENDED
      string   extended_type
      string   extended_data
        ...      more extended data (extended_type - extended_data pairs),
                 so that number of pairs equals extended_count
    */
    public class SftpATTRS
    {
        public enum PermissionFlags : int
        {
            S_IFDIR  = 0x4000,
            S_IFLNK  = 0xa000,

            S_ISUID  = 04000, // set user ID on execution
            S_ISGID  = 02000, // set group ID on execution
            S_ISVTX  = 01000, // sticky bit   ****** NOT DOCUMENTED *****

            S_IRUSR  = 00400, // read by owner
            S_IWUSR  = 00200, // write by owner
            S_IXUSR  = 00100, // execute/search by owner
            S_IREAD  = 00400, // read by owner
            S_IWRITE = 00200, // write by owner
            S_IEXEC  = 00100, // execute/search by owner

            S_IRGRP  = 00040, // read by group
            S_IWGRP  = 00020, // write by group
            S_IXGRP  = 00010, // execute/search by group

            S_IROTH  = 00004, // read by others
            S_IWOTH  = 00002, // write by others
            S_IXOTH  = 00001, // execute/search by others

            MASK = 0xFFF,
        }

        //private static int pmask = 0xFFF;

        public String PermissionsString
        {
            get
            {
                StringBuilder buf = new StringBuilder(10);

                if (IsDirectory) buf.Append('d');
                else if (IsLink) buf.Append('l');
                else buf.Append('-');

                buf.Append(_Permissions.HasFlag(PermissionFlags.S_IRUSR) ? 'r' : '-');
                buf.Append(_Permissions.HasFlag(PermissionFlags.S_IWUSR) ? 'w' : '-');

                if ((_Permissions & PermissionFlags.S_ISUID) != 0) buf.Append('s');
                else if ((_Permissions & PermissionFlags.S_IXUSR) != 0) buf.Append('x');
                else buf.Append('-');

                buf.Append(((_Permissions & PermissionFlags.S_IRGRP) != 0) ? 'r' : '-');
                buf.Append(((_Permissions & PermissionFlags.S_IWGRP) != 0) ? 'w' : '-');

                if ((_Permissions & PermissionFlags.S_ISGID) != 0) buf.Append('s');
                else if ((_Permissions & PermissionFlags.S_IXGRP) != 0) buf.Append('x');
                else buf.Append('-');

                buf.Append(((_Permissions & PermissionFlags.S_IROTH) != 0) ? 'r' : '-');
                buf.Append(((_Permissions & PermissionFlags.S_IWOTH) != 0) ? 'w' : '-');
                buf.Append(((_Permissions & PermissionFlags.S_IXOTH) != 0) ? 'x' : '-');

                return buf.ToString();
            }
        }

        public String getAtimeString()
        {
            //SimpleDateFormat locale=new SimpleDateFormat();
            //return (locale.format(new Date(atime)));
            //[tamir] use Time_T2DateTime to convert t_time to DateTime
            DateTime d = Util.Time_T2DateTime((uint)AccessTime);
            return d.ToShortDateString();

        }

        public String getMtimeString()
        {
            //[tamir] use Time_T2DateTime to convert t_time to DateTime
            DateTime date = Util.Time_T2DateTime((uint)ModificationTime);
            return (date.ToString());
        }

        public static int SSH_FILEXFER_ATTR_SIZE = 0x00000001;
        public static int SSH_FILEXFER_ATTR_UIDGID = 0x00000002;
        public static int SSH_FILEXFER_ATTR_PERMISSIONS = 0x00000004;
        public static int SSH_FILEXFER_ATTR_ACMODTIME = 0x00000008;
        public static uint SSH_FILEXFER_ATTR_EXTENDED = 0x80000000;

        public int Flags = 0;
        protected long _Size;
        internal int uid;
        internal int gid;
        protected PermissionFlags _Permissions;
        int AccessTime;
        int ModificationTime;
        String[] extended = null;

        private SftpATTRS()
        {
        }

        internal static SftpATTRS getATTR(Buffer buf)
        {
            SftpATTRS attr = new SftpATTRS();
            attr.Flags = buf.ReadInt();
            if ((attr.Flags & SSH_FILEXFER_ATTR_SIZE) != 0) { attr._Size = buf.ReadLong(); }
            if ((attr.Flags & SSH_FILEXFER_ATTR_UIDGID) != 0)
            {
                attr.uid = buf.ReadInt(); attr.gid = buf.ReadInt();
            }
            if ((attr.Flags & SSH_FILEXFER_ATTR_PERMISSIONS) != 0)
            {
                attr._Permissions = (PermissionFlags)buf.ReadInt();
            }
            if ((attr.Flags & SSH_FILEXFER_ATTR_ACMODTIME) != 0)
            {
                attr.AccessTime = buf.ReadInt();
            }
            if ((attr.Flags & SSH_FILEXFER_ATTR_ACMODTIME) != 0)
            {
                attr.ModificationTime = buf.ReadInt();
            }
            if ((attr.Flags & SSH_FILEXFER_ATTR_EXTENDED) != 0)
            {
                int count = buf.ReadInt();
                if (count > 0)
                {
                    attr.extended = new String[count * 2];
                    for (int i = 0; i < count; i++)
                    {
                        attr.extended[i * 2] = Util.getString(buf.ReadString());
                        attr.extended[i * 2 + 1] = Util.getString(buf.ReadString());
                    }
                }
            }
            return attr;
        }

        internal int Length()
        {
            return length();
        }

        internal int length()
        {
            int len = 4;

            if ((Flags & SSH_FILEXFER_ATTR_SIZE) != 0) { len += 8; }
            if ((Flags & SSH_FILEXFER_ATTR_UIDGID) != 0) { len += 8; }
            if ((Flags & SSH_FILEXFER_ATTR_PERMISSIONS) != 0) { len += 4; }
            if ((Flags & SSH_FILEXFER_ATTR_ACMODTIME) != 0) { len += 8; }
            if ((Flags & SSH_FILEXFER_ATTR_EXTENDED) != 0)
            {
                len += 4;
                int count = extended.Length / 2;
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        len += 4; len += extended[i * 2].Length;
                        len += 4; len += extended[i * 2 + 1].Length;
                    }
                }
            }
            return len;
        }

        internal void dump(Buffer buf)
        {
            buf.WriteInt(Flags);
            if ((Flags & SSH_FILEXFER_ATTR_SIZE) != 0) { buf.WriteLong(_Size); }
            if ((Flags & SSH_FILEXFER_ATTR_UIDGID) != 0)
            {
                buf.WriteInt(uid); buf.WriteInt(gid);
            }
            if ((Flags & SSH_FILEXFER_ATTR_PERMISSIONS) != 0)
            {
                buf.WriteInt((int)_Permissions);
            }
            if ((Flags & SSH_FILEXFER_ATTR_ACMODTIME) != 0) { buf.WriteInt(AccessTime); }
            if ((Flags & SSH_FILEXFER_ATTR_ACMODTIME) != 0) { buf.WriteInt(ModificationTime); }
            if ((Flags & SSH_FILEXFER_ATTR_EXTENDED) != 0)
            {
                int count = extended.Length / 2;
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        buf.WriteString(Util.getBytes(extended[i * 2]));
                        buf.WriteString(Util.getBytes(extended[i * 2 + 1]));
                    }
                }
            }
        }

     
        public long Size {
            set
            {
                Flags |= SSH_FILEXFER_ATTR_SIZE;
                this._Size = value;
            }
            get
            {
                return this._Size;
            }
        }

        public void setUIDGID(int uid, int gid)
        {
            Flags |= SSH_FILEXFER_ATTR_UIDGID;
            this.uid = uid;
            this.gid = gid;
        }
        public void setACMODTIME(int atime, int mtime)
        {
            Flags |= SSH_FILEXFER_ATTR_ACMODTIME;
            this.AccessTime = atime;
            this.ModificationTime = mtime;
        }

        public PermissionFlags Permissions {
            set
            {
                Flags |= SSH_FILEXFER_ATTR_PERMISSIONS;
                this._Permissions = (PermissionFlags)(((int)this._Permissions & ~(int)PermissionFlags.MASK) | ((int)value & (int)PermissionFlags.MASK));
            }
            get
            {
                return _Permissions;
            }
        }

        public bool IsDirectory
        {
            get
            {
                return ((Flags & SSH_FILEXFER_ATTR_PERMISSIONS) != 0 && ((_Permissions & PermissionFlags.S_IFDIR) == PermissionFlags.S_IFDIR));
            }
        }
        public bool IsLink
        {
            get
            {
                return ((Flags & SSH_FILEXFER_ATTR_PERMISSIONS) != 0 && ((_Permissions & PermissionFlags.S_IFLNK) == PermissionFlags.S_IFLNK));
            }
        }
        public int getUId() { return uid; }
        public int getGId() { return gid; }
        public int getATime() { return AccessTime; }
        public int getMTime() { return ModificationTime; }
        public String[] getExtended() { return extended; }

        public String toString()
        {
            return (PermissionsString + " " + getUId() + " " + getGId() + " " + Size + " " + getMtimeString());
        }

        public override string ToString()
        {
            return toString();
        }
        /*
        public String toString(){
          return (((flags&SSH_FILEXFER_ATTR_SIZE)!=0) ? ("size:"+size+" ") : "")+
                 (((flags&SSH_FILEXFER_ATTR_UIDGID)!=0) ? ("uid:"+uid+",gid:"+gid+" ") : "")+
                 (((flags&SSH_FILEXFER_ATTR_PERMISSIONS)!=0) ? ("permissions:0x"+Integer.toHexString(permissions)+" ") : "")+
                 (((flags&SSH_FILEXFER_ATTR_ACMODTIME)!=0) ? ("atime:"+atime+",mtime:"+mtime+" ") : "")+
                 (((flags&SSH_FILEXFER_ATTR_EXTENDED)!=0) ? ("extended:?"+" ") : "");
        }
        */
    }

}
