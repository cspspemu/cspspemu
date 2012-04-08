using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Vfs
{
	unsafe public class FileHandle : Stream
	{
		HleIoDrvFileArg HleIoDrvFileArg;
		//public HleIoManager HleIoManager;

		public IHleIoDriver HleIoDriver
		{
			get
			{
				return HleIoDrvFileArg.HleIoDriver;
			}
		}

		public FileHandle(HleIoDrvFileArg HleIoDrvFileArg)
		{
			this.HleIoDrvFileArg = HleIoDrvFileArg;
		}

		public override bool CanRead
		{
			get { return true; }
		}

		public override bool CanSeek
		{
			get { return true; }
		}

		public override bool CanWrite
		{
			get { return true; }
		}

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
			get
			{
				return HleIoDriver.IoLseek(HleIoDrvFileArg, 0, SeekAnchor.Cursor);
			}
			set
			{
				HleIoDriver.IoLseek(HleIoDrvFileArg, value, SeekAnchor.Set);
			}
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
			return HleIoDriver.IoLseek(HleIoDrvFileArg, offset, (SeekAnchor)origin);
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
