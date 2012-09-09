namespace CSPspEmu.Core.Memory
{
	public unsafe class LazyPspMemory : NormalPspMemory
	{
		protected override void Initialize()
		{
		}

		private void LazyCreateMemory()
		{
			if (ScratchPadPtr == null)
			{
				AllocateMemory();
			}
		}

		override public uint PointerToPspAddressUnsafe(void* Pointer)
		{
			LazyCreateMemory();
			return base.PointerToPspAddressUnsafe(Pointer);
		}

		override public void* PspAddressToPointerUnsafe(uint _Address)
		{
			LazyCreateMemory();
			return base.PspAddressToPointerUnsafe(_Address);
		}
	}
}
