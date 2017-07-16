using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace CSharpUtils._45.MongoAsync
{
	public class MongoClientAsync
	{
		public enum OpCodes : uint 
		{
			/// <summary>
			/// Reply to a client request. responseTo is set
			/// </summary>
			OP_REPLY          = 1,

			/// <summary>
			/// generic msg command followed by a string
			/// </summary>
			OP_MSG            = 1000,
			
			/// <summary>
			/// update document
			/// </summary>
			OP_UPDATE         = 2001,
			
			/// <summary>
			/// insert new document
			/// </summary>
			OP_INSERT         = 2002,
			
			/// <summary>
			/// formerly used for OP_GET_BY_OID
			/// </summary>
			RESERVED          = 2003,
			
			/// <summary>
			/// query a collection
			/// </summary>
			OP_QUERY          = 2004,
			
			/// <summary>
			/// Get more data from a query. See Cursors
			/// </summary>
			OP_GET_MORE       = 2005,
			
			/// <summary>
			/// Delete documents
			/// </summary>
			OP_DELETE         = 2006,
			
			/// <summary>
			/// Tell database client is done with a cursor
			/// </summary>
			OP_KILL_CURSORS   = 2007,
		}

		public struct MsgHeader
		{
			/// <summary>
			/// total message size, including this
			/// </summary>
			uint MessageLength;

			/// <summary>
			/// identifier for this message
			/// </summary>
			uint RequestID;
			
			/// <summary>
			/// requestID from the original request
			/// (used in reponses from db)
			/// </summary>
			uint ResponseTo;

			/// <summary>
			/// request type - see table below
			/// </summary>
			OpCodes OpCode;
		}

		protected TcpClient TcpClient;

		public MongoClientAsync()
		{
		}

		public void Connect(string Host = "localhost", ushort Port = 27017)
		{
			this.TcpClient = new TcpClient();
			TcpClient.Connect(Host, Port);
		}

		protected async Task<byte[]> ReadPacketAsync(Stream Stream)
		{
			var PacketSizeData = new byte[4];
			var PacketSizeDataReaded = await Stream.ReadAsync(PacketSizeData, 0, 4);
			if (PacketSizeDataReaded != 4) throw(new Exception("Error!"));
			var PacketSize = BitConverter.ToInt32(PacketSizeData, 0);
			var PacketData = new byte[PacketSize];
			PacketData[0] = PacketSizeData[0];
			PacketData[1] = PacketSizeData[1];
			PacketData[2] = PacketSizeData[2];
			PacketData[3] = PacketSizeData[3];
			var PacketDataReaded = await Stream.ReadAsync(PacketData, 4, PacketSize - 4);
			if (PacketDataReaded != PacketSize - 4) throw(new Exception("Error!"));
			return PacketData;
		}

		protected async Task<IEnumerable<BsonDocument>> ReadReplyFromMemory(MemoryStream PacketStream)
		{
			var PacketStreamReader = new BinaryReader(PacketStream);

			var MsgHeader = PacketStream.ReadStruct<MsgHeader>();

			var ResponseFlags = PacketStreamReader.ReadUInt32();
			var CursorID = PacketStreamReader.ReadUInt64();
			var StartingFrom = PacketStreamReader.ReadUInt32();
			var NumberReturned = PacketStreamReader.ReadUInt32();

			var Results = new List<BsonDocument>();

			for (int n = 0; n < NumberReturned; n++)
			{
				Results.Add(BsonSerializer.Deserialize<BsonDocument>(PacketStream));
			}

			return Results;
		}


		public async Task<IEnumerable<BsonDocument>> SendCommand()
		{
			var NetworkStream = TcpClient.GetStream();

			var WritePacket = new MemoryStream();
			var BinaryWriter = new BinaryWriter(WritePacket);
			var PacketBsonWriter = BsonWriter.Create(WritePacket);

			var MsgHeader = default(MsgHeader);
			BsonSerializer.Serialize(PacketBsonWriter, "admin.$cmd");
			BinaryWriter.Write((uint)0);
			BinaryWriter.Write((uint)100);

			BsonSerializer.Serialize(PacketBsonWriter, new[] { "test" });

			var WritePacketHeader = new MemoryStream();
			WritePacketHeader.WriteStruct(MsgHeader);

			await NetworkStream.WriteAsync(WritePacketHeader.GetBuffer(), 0, WritePacketHeader.GetBuffer().Length);
			await NetworkStream.WriteAsync(WritePacket.GetBuffer(), 0, WritePacket.GetBuffer().Length);

			return await ReadReplyFromMemory(new MemoryStream(await ReadPacketAsync(NetworkStream)));
		}
	}
}
