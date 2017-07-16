using System;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CSharpUtils._45.MysqlAsync
{
	public class MysqlClientAsync
	{
		TcpClient TcpClient;
		NetworkStream TcpClientStream;
		byte[] stage1_hash;

		public MysqlClientAsync()
		{
			this.TcpClient = new TcpClient();
		}

		protected static byte[] Sha1(byte[] Data)
		{
			return SHA1.Create().ComputeHash(Data);
		}

		public async Task ConnectAsync(int Port = 3306, string Host = "127.0.0.1", string User = "root", string Password = "")
		{
			await this.TcpClient.ConnectAsync(Host, Port);
			this.TcpClientStream = this.TcpClient.GetStream();
			this.stage1_hash = Sha1(Password.GetBytes(Encoding.ASCII));
		}

		struct HandshakeInitializationPacket
		{
			public byte ProtocolVersion;
			public string ServerVersion;
			public uint ThreadNumber;
			public byte[] ScrambleBuffer;
			public byte Filler1;
			public ServerCapabilitiesSet ServerCapablities;
			public byte ServerLanguage;
			public short ServerStatus;
		}

		enum ServerCapabilitiesSet : ushort
		{
		  CLIENT_LONG_PASSWORD	= 1,	/* new more secure passwords */
		  CLIENT_FOUND_ROWS= 	2,	/* Found instead of affected rows */
		  CLIENT_LONG_FLAG	= 4,	/* Get all column flags */
		  CLIENT_CONNECT_WITH_DB	= 8,	/* One can specify db on connect */
		  CLIENT_NO_SCHEMA	= 16,	/* Don't allow database.table.column */
		  CLIENT_COMPRESS		= 32,	/* Can use compression protocol */
		  CLIENT_ODBC		= 64,	/* Odbc client */
		  CLIENT_LOCAL_FILES	= 128,	/* Can use LOAD DATA LOCAL */
		  CLIENT_IGNORE_SPACE	= 256,	/* Ignore spaces before '(' */
		  CLIENT_PROTOCOL_41	= 512,	/* New 4.1 protocol */
		  CLIENT_INTERACTIVE	= 1024,	/* This is an interactive client */
		  CLIENT_SSL              = 2048,	/* Switch to SSL after handshake */
		  CLIENT_IGNORE_SIGPIPE   = 4096,    /* IGNORE sigpipes */
		  CLIENT_TRANSACTIONS	= 8192,	/* Client knows about transactions */
		  CLIENT_RESERVED         = 16384,   /* Old flag for 4.1 protocol  */
		  CLIENT_SECURE_CONNECTION = 32768,  /* New 4.1 authentication */
		  //CLIENT_MULTI_STATEMENTS = 65536,   /* Enable/disable multi-stmt support */
		  //CLIENT_MULTI_RESULTS    = 131072,  /* Enable/disable multi-results */
		}

		public async Task HandlePacketAsync()
		{
			var PacketHeader = new byte[4];
			await this.TcpClientStream.ReadAsync(PacketHeader, 0, 4);
			var Value = BitConverter.ToUInt32(PacketHeader, 0);
			uint PacketSize = Value & 0x7FFFFF;
			bool PacketFlag = ((Value >> 23) & 1) != 0;
			byte PacketSerial = (byte)(Value >> 24);
			var PacketData = new byte[PacketSize];
			await this.TcpClientStream.ReadAsync(PacketData, 0, PacketData.Length);
			var PacketDataStream = new MemoryStream(PacketData);
			var PacketDataBinaryReader = new BinaryReader(PacketDataStream);

			var Packet = default(HandshakeInitializationPacket);
			Packet.ProtocolVersion = PacketDataBinaryReader.ReadByte();
			Packet.ServerVersion = PacketDataStream.ReadStringz();
			Packet.ThreadNumber = PacketDataBinaryReader.ReadUInt32();
			Packet.ScrambleBuffer = PacketDataBinaryReader.ReadBytes(8);
			Packet.Filler1 = PacketDataBinaryReader.ReadByte();
			Packet.ServerCapablities = (ServerCapabilitiesSet)PacketDataBinaryReader.ReadUInt16();
			Packet.ServerLanguage = PacketDataBinaryReader.ReadByte();

			var Token = BitUtils.XorBytes(
				Sha1(Packet.ScrambleBuffer.Concat(stage1_hash)),
				stage1_hash
			);
			Console.WriteLine(Packet.ServerCapablities.ToStringDefault());
			//Console.WriteLine(Packet.ToStringDefault());
		}
	}
}
