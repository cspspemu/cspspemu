using System;
using System.IO;
using System.Linq;
using CSharpUtils.Ext.Extensions;
using CSharpUtils.Extensions;

namespace CSharpUtils.Ext.Compression
{
    /// <summary>
    /// 
    /// </summary>
    public class Huffman
    {
        /// <summary>
        /// 
        /// </summary>
        public class Node
        {
            /// <summary>
            /// 
            /// </summary>
            public int Index;

            /// <summary>
            /// 
            /// </summary>
            public bool Unused;

            /// <summary>
            /// 
            /// </summary>
            public uint UsageCount;

            /// <summary>
            /// 
            /// </summary>
            public bool HasChilds;

            /// <summary>
            /// 
            /// </summary>
            public Node Parent;

            /// <summary>
            /// 
            /// </summary>
            public Node LeftChild;

            /// <summary>
            /// 
            /// </summary>
            public Node RightChild;

            // Used for fast encoding.
            /// <summary>
            /// 
            /// </summary>
            public int EncodeBitsValue;

            /// <summary>
            /// 
            /// </summary>
            public int EncodeBitsCount;

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return
                    $"(Index={Index}, Unused={Unused}, UsageCount={UsageCount}, HasChilds={HasChilds}, Parent={Parent?.Index ?? -1}, Left={LeftChild?.Index ?? -1}, Right={RightChild?.Index ?? -1})";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="usageTable"></param>
        /// <returns></returns>
        public static Node[] BuildTable(uint[] usageTable)
        {
            var nodes = new Node[usageTable.Length * 2 - 1];
            var nodesCount = BuildTable(usageTable, nodes);
            var nodesSliced = new Node[nodesCount];
            Array.Copy(nodes, nodesSliced, nodesCount);
            return nodesSliced;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="usageTable"></param>
        /// <param name="encodingTable"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static int BuildTable(uint[] usageTable, Node[] encodingTable)
        {
            uint sumOfInputTable = 0;

            //Console.WriteLine(table1.ToStringArray(", "));

            {
                // Verified.
                for (int n = 0; n < usageTable.Length; n++)
                {
                    encodingTable[n] = new Node()
                    {
                        Index = n,
                        Unused = usageTable[n] > 0,
                        UsageCount = usageTable[n],
                        HasChilds = false,
                        Parent = null,
                        LeftChild = null,
                        RightChild = null,
                    };

                    //Nodes[n].LeftChild = Nodes[n];
                    //Nodes[n].RightChild = Nodes[n];

                    sumOfInputTable += usageTable[n];
                }

                if (sumOfInputTable == 0)
                {
                    throw new Exception("All elements on EncodingTable are 0");
                }
            }

            {
                // Verified.
                for (var n = usageTable.Length; n < encodingTable.Length; n++)
                {
                    encodingTable[n] = new Node()
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

            var currentNodeIndex = 0x100;

            while (true)
            {
                uint parentUsageCount = 0;

                var left = encodingTable.Where(it => it.Unused).LocateMin(it => it.UsageCount);

                if (left != null)
                {
                    left.Unused = false;
                    left.Parent = encodingTable[currentNodeIndex];
                    parentUsageCount += left.UsageCount;
                }

                var right = encodingTable.Where(it => it.Unused).LocateMin(it => it.UsageCount);

                if (right != null)
                {
                    right.Unused = false;
                    right.Parent = encodingTable[currentNodeIndex];
                    parentUsageCount += right.UsageCount;
                }

                Node node;

                encodingTable[currentNodeIndex] = node = new Node()
                {
                    Index = currentNodeIndex,
                    Unused = true,
                    UsageCount = parentUsageCount,
                    HasChilds = true,
                    Parent = null,
                    LeftChild = left,
                    RightChild = right,
                };

                currentNodeIndex++;

                if (node.UsageCount == sumOfInputTable) break;
            }

            SetEncodingRecursive(encodingTable[currentNodeIndex - 1]);

            return currentNodeIndex;
        }

        private static void SetEncodingRecursive(Node node, int encodeBitsCount = 0, int encodeBitsValue = 0)
        {
            if (node == null) return;
            node.EncodeBitsCount = encodeBitsCount;
            node.EncodeBitsValue = encodeBitsValue;
            SetEncodingRecursive(node.LeftChild, encodeBitsCount + 1, (encodeBitsValue << 1) | 0);
            SetEncodingRecursive(node.RightChild, encodeBitsCount + 1, (encodeBitsValue << 1) | 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static uint[] CalculateUsageTable(byte[] data)
        {
            var usageTable = new uint[256];
            foreach (var value in data)
            {
                usageTable[value]++;
            }
            return usageTable;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="encodingTable"></param>
        /// <returns></returns>
        public static Stream Compress(Stream input, Node[] encodingTable)
        {
            var output = new MemoryStream();
            Compress(input, output, encodingTable);
            output.Position = 0;
            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="encodingTable"></param>
        /// <exception cref="Exception"></exception>
        public static void Compress(Stream input, Stream output, Node[] encodingTable)
        {
            byte currentByte = 0;
            byte mask = 0x80;

            while (!input.Eof())
            {
                var symbol = input.ReadByte();
                var node = encodingTable[symbol];
                var nodeEncodingBitsCount = node.EncodeBitsCount;
                var nodeEncodeBitsValue = node.EncodeBitsValue;

                if (node.EncodeBitsCount == 0)
                    throw new Exception($"Symbol '{symbol}' not found on EncodingTable");

                //Console.WriteLine("Encode: {0}, {1}, {2}", Symbol, NodeEncodingBitsCount, NodeEncodeBitsValue);

                for (; nodeEncodingBitsCount > 0; nodeEncodingBitsCount--)
                {
                    if ((nodeEncodeBitsValue & (1 << (nodeEncodingBitsCount - 1))) != 0)
                    {
                        currentByte |= mask;
                    }

                    mask >>= 1;

                    if (mask == 0)
                    {
                        output.WriteByte(currentByte);
                        mask = 0x80;
                        currentByte = 0;
                    }
                }
            }

            // Write Remaining Bits.
            if (mask != 0x80)
            {
                output.WriteByte(currentByte);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="outputLength"></param>
        /// <param name="encodingTable"></param>
        /// <returns></returns>
        public static Stream Uncompress(Stream input, uint outputLength, Node[] encodingTable)
        {
            var output = new MemoryStream();
            Uncompress(input, output, outputLength, encodingTable);
            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="outputLength"></param>
        /// <param name="encodingTable"></param>
        public static void Uncompress(Stream input, Stream output, uint outputLength, Node[] encodingTable)
        {
            byte mask = 0;
            byte currentByte = 0;

            for (var n = 0; n < outputLength; n++)
            {
                var currentNode = encodingTable[encodingTable.Length - 1];

                while (currentNode.HasChilds)
                {
                    // Need another byte.
                    if (mask == 0)
                    {
                        currentByte = (byte) input.ReadByte();
                        mask = 0x80;
                    }

                    currentNode = (currentByte & mask) == 0
                            ? currentNode.LeftChild
                            : currentNode.RightChild
                        ;

                    mask >>= 1;
                }

                output.WriteByte((byte) currentNode.Index);
            }

            output.Position = 0;
        }
    }
}