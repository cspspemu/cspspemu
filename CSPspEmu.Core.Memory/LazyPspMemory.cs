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

		public override uint PointerToPspAddressUnsafe(void* Pointer)
		{
			LazyCreateMemory();
			return base.PointerToPspAddressUnsafe(Pointer);
		}

		public override void* PspAddressToPointerUnsafe(uint _Address)
		{
			LazyCreateMemory();
			return base.PspAddressToPointerUnsafe(_Address);
		}
	}
}
