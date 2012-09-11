using System;
using System.Collections.Generic;
using System.Linq;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Cpu.Dynarec;

namespace CSPspEmu.Core.Cpu
{
	public sealed class MethodCacheFast : PspEmulatorComponent
	{
		private DynarecFunction[] MethodsScratchPad = new DynarecFunction[PspMemory.ScratchPadSize / 4];
		private DynarecFunction[] MethodsMain = new DynarecFunction[PspMemory.MainSize / 4];
		private DynarecFunction[] MethodsFrameBuffer = new DynarecFunction[PspMemory.FrameBufferSize / 4];
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
			if (PspMemory.MainSegment.Contains(Address)) MethodsMain[AddressToIndex_Main(Address)] = Action;
			else if (PspMemory.ScratchPadSegment.Contains(Address)) MethodsScratchPad[AdressToIndex_ScratchPad(Address)] = Action;
			else if (PspMemory.FrameBufferSegment.Contains(Address)) MethodsFrameBuffer[AddressToIndex_FrameBuffer(Address)] = Action;
			else if (PspMemory.VectorsSegment.Contains(Address)) MethodsVectors[AddressToIndex_Vectors(Address)] = Action;
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
			if (PspMemory.MainSegment.Contains(PC)) return MethodsMain[AddressToIndex_Main(PC)];
			if (PspMemory.ScratchPadSegment.Contains(PC)) return MethodsScratchPad[AdressToIndex_ScratchPad(PC)];
			if (PspMemory.FrameBufferSegment.Contains(PC)) return MethodsFrameBuffer[AddressToIndex_FrameBuffer(PC)];
			if (PspMemory.VectorsSegment.Contains(PC)) return MethodsVectors[AddressToIndex_Vectors(PC)];

			throw (new IndexOutOfRangeException(
				String.Format("Can't jump to '{0}'. Invalid address.", "0x%08X".Sprintf(PC))
			));
		}

		public static uint AddressToIndex_Main(uint PC) { return (PC - PspMemory.MainOffset) / 4; }
		public static uint AdressToIndex_ScratchPad(uint PC) { return (PC - PspMemory.ScratchPadOffset) / 4; }
		public static uint AddressToIndex_FrameBuffer(uint PC) { return (PC - PspMemory.FrameBufferOffset) / 4; }
		public static uint AddressToIndex_Vectors(uint PC) { return (PC - PspMemory.VectorsOffset) / 4; }
	}
}