using CSPspEmu.Hle;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using CSharpUtils;

namespace CSPspEmu.Core.Tests
{
	[TestClass]
	public class MemoryPartitionTest
	{
		protected MemoryPartition PartitionRoot;

		[TestInitialize]
		public void SetUp()
		{
			PartitionRoot = new MemoryPartition(0x000, 0x100);
		}

		[TestMethod]
		public void Allocate1Test()
		{
			var Partition1 = PartitionRoot.Allocate(0x0FF);
			var Partition2 = PartitionRoot.Allocate(0x001);
			Assert.AreEqual(
				"MemoryPartition(Low=0, High=100, Allocated=True, Name='<Unknown>', ChildPartitions=[" +
					"MemoryPartition(Low=0, High=FF, Allocated=True, Name='<Unknown>')," +
					"MemoryPartition(Low=FF, High=100, Allocated=True, Name='<Unknown>')" +
				"])",
				PartitionRoot.ToString()
			);
		}

		[TestMethod]
		[ExpectedException(typeof(MemoryPartitionNoMemoryException))]
		public void AllocateTooBigTest()
		{
			PartitionRoot.Allocate(0x100);
			PartitionRoot.Allocate(0x001);
		}

		[TestMethod]
		public void AllocateFreeNormalizeTest()
		{
			var Partition1 = PartitionRoot.Allocate(0x040, MemoryPartition.Anchor.Low);
			var Partition2 = PartitionRoot.Allocate(0x040, MemoryPartition.Anchor.Low);
			var Partition3 = PartitionRoot.Allocate(0x040, MemoryPartition.Anchor.Low);
			var Partition4 = PartitionRoot.Allocate(0x040, MemoryPartition.Anchor.Low);
			PartitionRoot.DeallocateLow(Partition2.Low);
			PartitionRoot.DeallocateHigh(Partition3.High);
			Assert.AreEqual(
				"MemoryPartition(Low=0, High=100, Allocated=True, Name='<Unknown>', ChildPartitions=[" +
					"MemoryPartition(Low=0, High=40, Allocated=True, Name='<Unknown>')," +
					"MemoryPartition(Low=40, High=C0, Allocated=False, Name='<Unknown>')," +
					"MemoryPartition(Low=C0, High=100, Allocated=True, Name='<Unknown>')" +
				"])",
				PartitionRoot.ToString()
			);
		}

		[TestMethod]
		public void AllocateFixedPositionTest()
		{
			var Partition1 = PartitionRoot.AllocateLowSize(0x60, 0x40);
			Assert.AreEqual(
				"MemoryPartition(Low=0, High=100, Allocated=True, Name='<Unknown>', ChildPartitions=[" +
					"MemoryPartition(Low=0, High=60, Allocated=False, Name='<Free>')," +
					"MemoryPartition(Low=60, High=A0, Allocated=True, Name='<Unknown>')," +
					"MemoryPartition(Low=A0, High=100, Allocated=False, Name='<Free>')" +
				"])",
				PartitionRoot.ToString()
			);
		}

		[TestMethod]
		public void MathUtilsPrevAlignedTest()
		{
			Assert.AreEqual(0x200, (int)MathUtils.PrevAligned(0x260, 0x100));
		}

		[TestMethod]
		public void AllocateAlignedStackOnNonAlignedSegmentTest()
		{
			PartitionRoot = new MemoryPartition(0x000, 0x260);
			PartitionRoot.Allocate(0x100, MemoryPartition.Anchor.High, Alignment: 0x100);
		}
	}
}
