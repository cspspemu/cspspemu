using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Cpu.Emiter;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Cpu.Dynarec;

namespace CSPspEmu.Core.Cpu
{
	sealed public class MethodCacheFast : PspEmulatorComponent
	{
		private DynarecFunction[] Methods2 = new DynarecFunction[PspMemory.MainSize / 4];
		private SortedSet<uint> MethodsInList = new SortedSet<uint>();

		public event Action<uint, uint> OnClearRange;

		public void ClearRange(uint Low, uint High)
		{
			if (OnClearRange != null)
			{
				OnClearRange(Low, High);
			}

			if (Low == uint.MinValue && High == uint.MaxValue)
			{
				foreach (var Address in MethodsInList) Methods2[Address_To_Index(Address)] = null;
				MethodsInList = new SortedSet<uint>();
			}
			else
			{
				foreach (var Address in MethodsInList.Where(Address => ((Address >= Low) && (Address < High))).ToArray())
				{
					Methods2[Address_To_Index(Address)] = null;
					MethodsInList.Remove(Address);
				}
			}
		}

		public void Clear()
		{
			ClearRange(uint.MinValue, uint.MaxValue);
		}

		public DynarecFunction TryGetMethodAt(uint PC)
		{
			//PC &= PspMemory.MemoryMask;
			if (PC < PspMemory.MainOffset) throw (new PspMemory.InvalidAddressException(PC));
			uint Index = (PC - PspMemory.MainOffset) / 4;
			if (Index < 0 || Index >= PspMemory.MainSize / 4)
			{
				throw(new IndexOutOfRangeException(
					String.Format("Can't jump to '{0}'. Invalid address.", "0x%08X".Sprintf(PC))
				));
			}
			return Methods2[Index];
		}

		public uint Address_To_Index(uint PC)
		{
			return (PC - PspMemory.MainOffset) / 4;
		}

		public void SetMethodAt(uint PC, DynarecFunction Action)
		{
			//PC &= PspMemory.MemoryMask;
			if (PC < PspMemory.MainOffset) throw (new PspMemory.InvalidAddressException(PC));
			uint Index = Address_To_Index(PC);
			Methods2[Index] = Action;
			MethodsInList.Add(PC);
		}
	}
}