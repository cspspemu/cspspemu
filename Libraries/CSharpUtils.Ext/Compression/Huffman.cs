using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Extensions;
using System.IO;

namespace CSharpUtils.Compression
{
	public class Huffman
	{
		public class Node
		{
			public int Index;
			public bool Unused;
			public uint UsageCount;
			public bool HasChilds;
			public Node Parent;
			public Node LeftChild;
			public Node RightChild;

			// Used for fast encoding.
			public int EncodeBitsValue;
			public int EncodeBitsCount;

			public override string ToString()
			{
				return String.Format(
					"(Index={0}, Unused={1}, UsageCount={2}, HasChilds={3}, Parent={4}, Left={5}, Right={6})",
					Index,
					Unused, UsageCount, HasChilds, (Parent != null) ? Parent.Index : -1, (LeftChild != null) ? LeftChild.Index : -1, (RightChild != null) ? RightChild.Index : -1
				);
			}
		}

		static public Node[] BuildTable(uint[] UsageTable)
		{
			Node[] Nodes = new Node[UsageTable.Length * 2 - 1];
			int NodesCount = BuildTable(UsageTable, Nodes);
			Node[] NodesSliced = new Node[NodesCount];
			Array.Copy(Nodes, NodesSliced, NodesCount);
			return NodesSliced;
		}

		static public int BuildTable(uint[] UsageTable, Node[] EncodingTable)
		{
			uint SumOfInputTable = 0;

			//Console.WriteLine(table1.ToStringArray(", "));

			{ // Verified.
				for (int n = 0; n < UsageTable.Length; n++)
				{
					EncodingTable[n] = new Node()
					{
						Index = n,
						Unused = (UsageTable[n] > 0),
						UsageCount = UsageTable[n],
						HasChilds = false,
						Parent = null,
						LeftChild = null,
						RightChild = null,
					};

					//Nodes[n].LeftChild = Nodes[n];
					//Nodes[n].RightChild = Nodes[n];

					SumOfInputTable += UsageTable[n];
				}

				if (SumOfInputTable == 0)
				{
					throw (new Exception("All elements on EncodingTable are 0"));
				}
			}

			{ // Verified.
				for (int n = UsageTable.Length; n < EncodingTable.Length; n++)
				{
					EncodingTable[n] = new Node()
					{
						Index = n,
						Unused = false,
						UsageCount = 0,
						HasChilds = true,
						Parent = null,
						LeftChild = null,
						RightChild = null,
					};
				}

				//std.file.write("table_out", cast(ubyte[])cast(void[])*(&table2[0..table2.length]));
			}

			int CurrentNodeIndex = 0x100;
			Node Left, Right;

			while (true)
			{
				uint ParentUsageCount = 0;

				Left = EncodingTable.Where(Node => Node.Unused).LocateMin(Node => Node.UsageCount);

				if (Left != null)
				{
					Left.Unused = false;
					Left.Parent = EncodingTable[CurrentNodeIndex];
					ParentUsageCount += Left.UsageCount;
				}

				Right = EncodingTable.Where(Node => Node.Unused).LocateMin(Node => Node.UsageCount);

				if (Right != null)
				{
					Right.Unused = false;
					Right.Parent = EncodingTable[CurrentNodeIndex];
					ParentUsageCount += Right.UsageCount;
				}

				Node node;

				EncodingTable[CurrentNodeIndex] = node = new Node()
				{
					Index = CurrentNodeIndex,
					Unused = true,
					UsageCount = ParentUsageCount,
					HasChilds = true,
					Parent = null,
					LeftChild = Left,
					RightChild = Right,
				};

				CurrentNodeIndex++;

				if (node.UsageCount == SumOfInputTable) break;
			}

			SetEncodingRecursive(EncodingTable[CurrentNodeIndex - 1]);

			return CurrentNodeIndex;
		}

		private static void SetEncodingRecursive(Node Node, int EncodeBitsCount = 0, int EncodeBitsValue = 0)
		{
			if (Node == null) return;
			Node.EncodeBitsCount = EncodeBitsCount;
			Node.EncodeBitsValue = EncodeBitsValue;
			SetEncodingRecursive(Node.LeftChild, EncodeBitsCount + 1, (EncodeBitsValue << 1) | 0);
			SetEncodingRecursive(Node.RightChild, EncodeBitsCount + 1, (EncodeBitsValue << 1) | 1);
		}

		static public uint[] CalculateUsageTable(byte[] Data)
		{
			var UsageTable = new uint[256];
			foreach (var Value in Data) {
				UsageTable[Value]++;
			}
			return UsageTable;
		}

		static public Stream Compress(Stream Input, Node[] EncodingTable)
		{
			Stream Output = new MemoryStream();
			Compress(Input, Output, EncodingTable);
			Output.Position = 0;
			return Output;
		}

		static public void Compress(Stream Input, Stream Output, Node[] EncodingTable)
		{
			byte CurrentByte = 0;
			byte Mask = 0x80;

			while (!Input.Eof())
			{
				var Symbol = Input.ReadByte();
				var Node = EncodingTable[Symbol];
				var NodeEncodingBitsCount = Node.EncodeBitsCount;
				var NodeEncodeBitsValue = Node.EncodeBitsValue;

				if (Node.EncodeBitsCount == 0) throw (new Exception(String.Format("Symbol '{0}' not found on EncodingTable", Symbol)));

				//Console.WriteLine("Encode: {0}, {1}, {2}", Symbol, NodeEncodingBitsCount, NodeEncodeBitsValue);

				for (; NodeEncodingBitsCount > 0; NodeEncodingBitsCount--)
				{
					if ((NodeEncodeBitsValue & (1 << (NodeEncodingBitsCount - 1))) != 0)
					{
						CurrentByte |= Mask;
					}

					Mask >>= 1;

					if (Mask == 0)
					{
						Output.WriteByte(CurrentByte);
						Mask = 0x80;
						CurrentByte = 0;
					}

				}
			}

			// Write Remaining Bits.
			if (Mask != 0x80)
			{
				Output.WriteByte(CurrentByte);
			}
		}


		static public Stream Uncompress(Stream Input, uint OutputLength, Node[] EncodingTable)
		{
			Stream Output = new MemoryStream();
			Uncompress(Input, Output, OutputLength, EncodingTable);
			return Output;
		}

		static public void Uncompress(Stream Input, Stream Output, uint OutputLength, Node[] EncodingTable)
		{
			byte Mask = 0;
			byte CurrentByte = 0;

			for (int n = 0; n < OutputLength; n++)
			{
				var CurrentNode = EncodingTable[EncodingTable.Length - 1];

				while (CurrentNode.HasChilds)
				{
					// Need another byte.
					if (Mask == 0)
					{
						CurrentByte = (byte)Input.ReadByte();
						Mask = 0x80;
					}

					CurrentNode = ((CurrentByte & Mask) == 0)
						? CurrentNode.LeftChild
						: CurrentNode.RightChild
					;

					Mask >>= 1;
				}

				Output.WriteByte((byte)CurrentNode.Index);
			}

			Output.Position = 0;
		}
	}
}
