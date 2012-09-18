using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSPspEmu.Core;
using CSPspEmu.Core.Cpu;

namespace CSPspEmuLLETest
{
	public class Dma
	{
		public enum Direction
		{
			Write,
			Read,
		}

		public CpuThreadState CpuThreadState;
		public LLEState LLEState;
		//0xBC100004

		public Dma(CpuThreadState CpuThreadState)
		{
			this.CpuThreadState = CpuThreadState;
		}

		bool LogDMAReads = true;

		private string GetRegisterName(uint Address)
		{
			string Reg = "";
			var DmaAddress = (DmaEnum)Address;

			if ((DmaAddress >= DmaEnum.NAND__DATA_PAGE_START) && (DmaAddress < DmaEnum.NAND__DATA_PAGE_END))
			{
				return String.Format("NAND__DATA_PAGE[{0}]", DmaAddress - DmaEnum.NAND__DATA_PAGE_START);
			}

			if (Enum.IsDefined(typeof(DmaEnum), Address))
			{
				Reg = String.Format("{0}({1:X8})", (DmaEnum)Address, (uint)Address);
			}
			else
			{
				Reg = String.Format("Unknown({0:X8})", (uint)Address);
			}

			return Reg;
		}

		public void LogDMA(Dma.Direction Direction, int Size, uint Address, ref uint Value)
		{
			var DmaAddress = (DmaEnum)Address;
			if ((DmaAddress >= DmaEnum.NAND__DATA_PAGE_START) && (DmaAddress < DmaEnum.NAND__DATA_PAGE_END)) return;
			Console.WriteLine("PC({0:X8}) {1}: {2} : 0x{3:X8}", CpuThreadState.PC, Direction, GetRegisterName(Address), Value);
		}

		byte[] Test = new byte[0x1000];

		public void TransferDMA(Dma.Direction Direction, int Size, DmaEnum Address, ref uint Value)
		{
			if (false) { }
			/*
			else if ((uint)Address >= 0x1FD00000 && (uint)Address <= 0x1FD00000 + 0x1000)
			{
				TransferUtils.TransferToArray(Direction, Test, (int)(Address - 0x1FD00000), Size, ref Value);
			}
			*/
			else if (Address >= DmaEnum.GPIO && Address <= DmaEnum.GPIO__PORT_CLEAR) { LLEState.GPIO.Transfer(Direction, Size, (DmaEnum)Address, ref Value); return; }
			else if (Address >= DmaEnum.NAND__CONTROL && Address <= DmaEnum.NAND__READDATA) { LLEState.NAND.Transfer(Direction, Size, (DmaEnum)Address, ref Value); return; }
			else if (Address >= DmaEnum.NAND__DATA_PAGE_START && Address <= DmaEnum.NAND__DATA_EXTRA_END) { LLEState.NAND.Transfer(Direction, Size, (DmaEnum)Address, ref Value); return; }
			else if (Address >= DmaEnum.KIRK_SIGNATURE && Address <= DmaEnum.KIRK_UNK_50) { LLEState.LleKirk.Transfer(Direction, Size, (DmaEnum)Address, ref Value); return; }
			else
			{
				Console.WriteLine("Unprocessed LLEState.Memory:{0}", LLEState.Memory.MountedPreIpl);
				//Thread.Sleep(100);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Size"></param>
		/// <param name="Address"></param>
		/// <returns></returns>
		public uint ReadDMA(int Size, uint Address)
		{
			uint Value = 0;

			TransferDMA(Direction.Read, Size, (DmaEnum)Address, ref Value);
			LogDMA(Direction.Read, Size, Address, ref Value);

			return Value;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Size"></param>
		/// <param name="Address"></param>
		/// <param name="Value"></param>
		public void WriteDMA(int Size, uint Address, uint Value)
		{
			LogDMA(Direction.Write, Size, Address, ref Value);
			TransferDMA(Direction.Write, Size, (DmaEnum)Address, ref Value);

			switch ((DmaEnum)Address)
			{
				case DmaEnum.SystemConfig__RESET_ENABLE:
					LLEState.Memory.MountedPreIpl = false;
					/*
					if ((Value & 2) != 0) // ME
					{
						LLEState.Me.Reset();
					}
					*/
					break;
			}
		}
	}
}
