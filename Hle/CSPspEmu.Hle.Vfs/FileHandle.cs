using System;
using System.IO;

namespace CSPspEmu.Hle.Vfs
{
    public unsafe class FileHandle : Stream
    {
        HleIoDrvFileArg HleIoDrvFileArg;
        //public HleIoManager HleIoManager;

        public IHleIoDriver HleIoDriver => HleIoDrvFileArg.HleIoDriver;

        public FileHandle(HleIoDrvFileArg HleIoDrvFileArg)
        {
            this.HleIoDrvFileArg = HleIoDrvFileArg;
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override void Flush()
        {
        }

        public override long Length
        {
            get
            {
                var Previous = HleIoDriver.IoLseek(HleIoDrvFileArg, 0, SeekAnchor.Cursor);
                var Length = HleIoDriver.IoLseek(HleIoDrvFileArg, 0, SeekAnchor.End);
                HleIoDriver.IoLseek(HleIoDrvFileArg, Previous, SeekAnchor.Set);
                return Length;
            }
        }

        public override long Position
        {
            get => HleIoDriver.IoLseek(HleIoDrvFileArg, 0, SeekAnchor.Cursor);
            set => HleIoDriver.IoLseek(HleIoDrvFileArg, value, SeekAnchor.Set);
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            fixed (byte* FixedBuffer = &buffer[offset])
            {
                return HleIoDriver.IoRead(HleIoDrvFileArg, FixedBuffer, count);
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return HleIoDriver.IoLseek(HleIoDrvFileArg, offset, (SeekAnchor) origin);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            fixed (byte* FixedBuffer = &buffer[offset])
            {
                HleIoDriver.IoWrite(HleIoDrvFileArg, FixedBuffer, count);
            }
        }

        public override void Close()
        {
            HleIoDriver.IoClose(HleIoDrvFileArg);
            base.Close();
        }
    }
}