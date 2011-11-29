using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Extensions;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Core.Cpu
{
	sealed public class MethodCacheFast
	{
		private Action<CpuThreadState>[] Methods2 = new Action<CpuThreadState>[PspMemory.MainSize / 4];
		private SortedSet<uint> MethodsInList = new SortedSet<uint>();

		public void ClearRange(uint Low, uint High)
		{
			/*
			for (uint n = Low; n < High; n += 4)
			{
				Methods2[n] = null;
			}
			*/

			foreach (var Address in MethodsInList.Where(Address => ((Address >= Low) && (Address < High))).ToArray())
			{
				Methods2[Address_To_Index(Address)] = null;
				MethodsInList.Remove(Address);
			}
		}

		public void Clear()
		{
			//Methods2 = new Action<CpuThreadState>[PspMemory.MainSize / 4];
			foreach (var Address in MethodsInList) Methods2[Address_To_Index(Address)] = null;
			MethodsInList = new SortedSet<uint>();
		}

		public Action<CpuThreadState> TryGetMethodAt(uint PC)
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
			Action<CpuThreadState> Action = Methods2[Index];
			return Action;
		}

		public uint Address_To_Index(uint PC)
		{
			return (PC - PspMemory.MainOffset) / 4;
		}

		public void SetMethodAt(uint PC, Action<CpuThreadState> Action)
		{
			//PC &= PspMemory.MemoryMask;
			if (PC < PspMemory.MainOffset) throw (new PspMemory.InvalidAddressException(PC));
			uint Index = Address_To_Index(PC);
			Methods2[Index] = Action;
			MethodsInList.Add(PC);
		}
	}
}