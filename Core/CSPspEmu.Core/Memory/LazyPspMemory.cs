namespace CSPspEmu.Core.Memory
{
	public unsafe class LazyPspMemory : NormalPspMemory
	{
		public LazyPspMemory()
		{
		}

		private void LazyCreateMemory()
		{
			if (ScratchPadPtr == null)
			{
				AllocateMemory();
			}
		}

		public override uint PointerToPspAddressUnsafe(void* pointer)
		{
			LazyCreateMemory();
			return base.PointerToPspAddressUnsafe(pointer);
		}

		public override void* PspAddressToPointerUnsafe(uint address)
		{
			LazyCreateMemory();
			return base.PspAddressToPointerUnsafe(address);
		}
	}
}
