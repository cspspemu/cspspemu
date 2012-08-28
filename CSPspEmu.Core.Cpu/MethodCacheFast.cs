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
		private DynarecFunction[] MethodsScratchPad = new DynarecFunction[PspMemory.ScratchPadSize / 4];
		private DynarecFunction[] MethodsMain = new DynarecFunction[PspMemory.MainSize / 4];
		private DynarecFunction[] MethodsVectors = new DynarecFunction[PspMemory.VectorsSize / 4];
		private SortedSet<uint> MethodsInList = new SortedSet<uint>();

		public event Action<uint, uint> OnClearRange;

		public void ClearRange(uint Low, uint High)
		{
			if (OnClearRange != null)
			{
				OnClearRange(Low, High);
			}

			lock (this)
			{
				if (Low == uint.MinValue && High == uint.MaxValue)
				{
					foreach (var Address in MethodsInList) RemoveMethodAt(Address);
					MethodsInList = new SortedSet<uint>();
				}
				else
				{
					foreach (var Address in MethodsInList.Where(Address => ((Address >= Low) && (Address < High))).ToArray())
					{
						RemoveMethodAt(Address);
						MethodsInList.Remove(Address);
					}
				}
			}
		}

		public void SetMethodAt(uint Address, DynarecFunction Action)
		{
			if (PspMemory.MainSegment.Contains(Address)) MethodsMain[MainAddress_To_Index(Address)] = Action;
			else if (PspMemory.ScratchPadSegment.Contains(Address)) MethodsScratchPad[ScratchPadAddress_To_Index(Address)] = Action;
			else if (PspMemory.VectorsSegment.Contains(Address)) MethodsVectors[VectorsAddress_To_Index(Address)] = Action;
			if (Action != null) MethodsInList.Add(Address);
		}

		public void RemoveMethodAt(uint Address)
		{
			SetMethodAt(Address, null);
		}

		public void Clear()
		{
			ClearRange(uint.MinValue, uint.MaxValue);
		}

		public DynarecFunction TryGetMethodAt(uint PC)
		{
			if (PspMemory.MainSegment.Contains(PC)) return MethodsMain[MainAddress_To_Index(PC)];
			if (PspMemory.ScratchPadSegment.Contains(PC)) return MethodsScratchPad[ScratchPadAddress_To_Index(PC)];
			if (PspMemory.VectorsSegment.Contains(PC)) return MethodsVectors[VectorsAddress_To_Index(PC)];

			throw (new IndexOutOfRangeException(
				String.Format("Can't jump to '{0}'. Invalid address.", "0x%08X".Sprintf(PC))
			));
		}

		public uint VectorsAddress_To_Index(uint PC)
		{
			return (PC - PspMemory.VectorsOffset) / 4;
		}

		public uint ScratchPadAddress_To_Index(uint PC)
		{
			return (PC - PspMemory.ScratchPadOffset) / 4;
		}

		public uint MainAddress_To_Index(uint PC)
		{
			return (PC - PspMemory.MainOffset) / 4;
		}
	}
}