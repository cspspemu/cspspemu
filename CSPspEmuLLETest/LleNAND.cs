using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpUtils;

namespace CSPspEmuLLETest
{
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="http://hitmen.c02.at/files/yapspd/psp_doc/chap8.html#sec8.6"/>
	public class LleNAND : ILleDma
	{
		[Flags]
		public enum EnumControlRegister : uint
		{
			CalculateECCWhenWritting = (1 << 17),
			CalculateECCWhenReading = (1 << 16),
		}

		public enum EnumCommands : uint
		{
			Reset = 0xFF,
		}

		[Flags]
		public enum EnumStatus : uint
		{
			WriteProtected = (1 << 7),
			Ready = (1 << 0), // 1 - READY | 0 - BUSY
		}

		EnumControlRegister ControlRegister;
		EnumStatus Status;
		uint Command;

		/// <summary>
		/// Physical page to access
		/// </summary>
		uint Address;

		/// <summary>
		/// 
		/// </summary>
		uint DmaAddress;

		/// <summary>
		/// 
		/// </summary>
		public Stream NandStream;

		public byte[] NandBlock;

		public LleNAND(Stream NandStream)
		{
			this.NandStream = NandStream;
		}
		

		private void Reset()
		{
			Console.WriteLine("Reset NAND controller to default state?");
		}

		public void Transfer(Dma.Direction Direction, int Size, DmaEnum Address, ref uint Value)
		{
			// Reading sector
			if ((Address >= DmaEnum.NAND__DATA_PAGE_START) && (Address < DmaEnum.NAND__DATA_PAGE_END))
			{
				var Offset = (int)(Address - DmaEnum.NAND__DATA_PAGE_START);
				//Console.WriteLine("{0:X8}", (uint)Address);
				//Console.WriteLine("Transfer {0} / {1} [{2}]", Offset, Size, NandBlock.Length);
				TransferUtils.TransferToArray(Direction, NandBlock, Offset, Size, ref Value);
				return;
			}

			if ((Address >= DmaEnum.NAND__DATA_SPARE_BUF0_REG) && (Address < DmaEnum.NAND__DATA_EXTRA_END))
			{
				var Offset = (int)(Address - DmaEnum.NAND__DATA_SPARE_BUF0_REG);
				TransferUtils.TransferToArray(Direction, NandBlock, 512 + Offset + 4, Size, ref Value);
				return;
			}

			switch (Address) 
			{
				case DmaEnum.NAND__CONTROL: TransferUtils.Transfer(Direction, ref ControlRegister, ref Value); break;
				case DmaEnum.NAND__STATUS:
					TransferUtils.Transfer(Direction, ref Status, ref Value);
					//Thread.Sleep(200);
					break;
				case DmaEnum.NAND__COMMAND:
					TransferUtils.Transfer(Direction, ref Command, ref Value);

					// Reset
					if (Direction == Dma.Direction.Write)
					{
						switch ((EnumCommands)Value)
						{
							case EnumCommands.Reset:
								Status = EnumStatus.Ready;
								break;
						}
					}
				break;
				case DmaEnum.NAND__ADDRESS: TransferUtils.Transfer(Direction, ref Address, ref Value); break;
				case DmaEnum.NAND__RESET:
					if (Direction == Dma.Direction.Write)
					{
						Reset();
					}
				break;
				case DmaEnum.NAND__DMA_ADDRESS: TransferUtils.Transfer(Direction, ref DmaAddress, ref Value); break;
				case DmaEnum.NAND__DMA_CONTROL:
					if (Direction == Dma.Direction.Write)
					{
						if (Value == 0x301)
						{
							//0x20000/2/512*(512+16)
							NandStream.Position = ((DmaAddress / 2 / 512) * (512 + 16));
							NandBlock = NandStream.ReadBytes(512 + 16);
							Console.WriteLine("Read from NAND: 0x{0:X8}", DmaAddress);
							ArrayUtils.HexDump(NandBlock);
							
							//Thread.Sleep(TimeSpan.FromSeconds(0.5));
							//Thread.Sleep(-1);
						}
					}
					else
					{
						Value = 0;
					}
					break;
				case DmaEnum.NAND__DMA_ERROR:
					Value = 0;
					break;
			}
		}
	}
}
