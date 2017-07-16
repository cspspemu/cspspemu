using System;
using System.IO;
using System.Text;

namespace CSharpUtils.Fastcgi
{
	public class FastcgiPacketWriter
	{
        public bool Debug = false;
        public IFastcgiPipe FastcgiPipe;
        byte[] Header = new byte[8];
        byte[] TempContents = new byte[8];
        static byte[] Padding = new byte[8];

        public FastcgiPacketWriter(IFastcgiPipe FastcgiPipe, bool Debug = false)
		{
            this.FastcgiPipe = FastcgiPipe;
            this.Debug = Debug;
		}

		public void WritePacketParams()
		{
		}

		public static void WriteVariableInt(Stream Stream, int Value)
		{
			throw (new NotImplementedException());
		}

		public bool WritePacketEndRequest(ushort RequestId, int AppStatus, Fastcgi.ProtocolStatus ProtocolStatus)
		{
            TempContents[0] = (byte)((AppStatus >> 24) & 0xFF);
            TempContents[1] = (byte)((AppStatus >> 16) & 0xFF);
            TempContents[2] = (byte)((AppStatus >> 8) & 0xFF);
            TempContents[3] = (byte)((AppStatus >> 0) & 0xFF);
            TempContents[4] = (byte)(ProtocolStatus);
            TempContents[5] = 0;
            TempContents[6] = 0;
            TempContents[7] = 0;

            if (Debug)
            {
                Console.WriteLine("WritePacketEndRequest(" + TempContents.Implode(",") + ")");
            }

            return WritePacket(RequestId, Fastcgi.PacketType.FCGI_END_REQUEST, TempContents, 0, 8);
		}

		public bool WritePacket(ushort RequestId, Fastcgi.PacketType Type, byte[] Contents, int ContentsOffset, int ContentsLength)
		{
            int PaddingLength = (8 - ContentsLength & 7) & 7;

            if (Debug)
            {
                Console.WriteLine("WritePacket(RequestId=" + RequestId + ", Type=" + Type + ", Contents=" + Contents.Length + ", Padding=" + PaddingLength + ")");
                if (Type == Fastcgi.PacketType.FCGI_STDOUT)
                {
                    Console.Write(Encoding.UTF8.GetString(Contents));
                }
            }

            Header[0] = 1;
            Header[1] = (byte)Type;
            Header[2] = (byte)((RequestId      >> 8) & 0xFF);
            Header[3] = (byte)((RequestId      >> 0) & 0xFF);
            Header[4] = (byte)((ContentsLength >> 8) & 0xFF);
            Header[5] = (byte)((ContentsLength >> 0) & 0xFF);
            Header[6] = (byte)PaddingLength;
            Header[7] = 0;

            try
            {
                FastcgiPipe.Write(Header, 0, 8);
                if (ContentsLength > 0)
                {
                    FastcgiPipe.Write(Contents, ContentsOffset, ContentsLength);
                }
                if (PaddingLength > 0)
                {
                    FastcgiPipe.Write(Padding, 0, PaddingLength);
                }
                return true;
            }
            catch (Exception Exception)
            {
                if (Debug)
                {
                    Console.Error.WriteLine(Exception);
                }
                return false;
            }
		}
	}
}
