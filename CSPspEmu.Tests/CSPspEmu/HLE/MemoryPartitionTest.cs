using CSharpUtils;
using CSPspEmu.Hle;
using Xunit;

namespace Tests.CSPspEmu.Hle
{
    
    public class MemoryPartitionTest
    {
        protected InjectContext InjectContext;
        protected MemoryPartition PartitionRoot;

        public MemoryPartitionTest()
        {
            PartitionRoot = new MemoryPartition(InjectContext, 0x000, 0x100);
        }

        [Fact(Skip = "check")]
        public void Allocate1Test()
        {
            PartitionRoot.Allocate(0x0FF);
            PartitionRoot.Allocate(0x001);
            Assert.Equal(
                "MemoryPartition(Low=0, High=100, Allocated=True, Name='<Unknown>', ChildPartitions=[" +
                "MemoryPartition(Low=0, High=FF, Allocated=True, Name='<Unknown>')," +
                "MemoryPartition(Low=FF, High=100, Allocated=True, Name='<Unknown>')" +
                "])",
                PartitionRoot.ToString()
            );
        }

        [Fact(Skip = "check")]
        public void AllocateTooBigTest()
        {
            Assert.Throws<MemoryPartitionNoMemoryException>(() =>
            {
                PartitionRoot.Allocate(0x100);
                PartitionRoot.Allocate(0x001);
            });
        }

        [Fact(Skip = "check")]
        // ReSharper disable RedundantArgumentDefaultValue
        // ReSharper disable UnusedVariable
        public void AllocateFreeNormalizeTest()
        {
            var partition1 = PartitionRoot.Allocate(0x040, MemoryPartition.Anchor.Low);
            var partition2 = PartitionRoot.Allocate(0x040, MemoryPartition.Anchor.Low);
            var partition3 = PartitionRoot.Allocate(0x040, MemoryPartition.Anchor.Low);
            var partition4 = PartitionRoot.Allocate(0x040, MemoryPartition.Anchor.Low);
            PartitionRoot.DeallocateLow(partition2.Low);
            PartitionRoot.DeallocateHigh(partition3.High);
            Assert.Equal(
                "MemoryPartition(Low=0, High=100, Allocated=True, Name='<Unknown>', ChildPartitions=[" +
                "MemoryPartition(Low=0, High=40, Allocated=True, Name='<Unknown>')," +
                "MemoryPartition(Low=40, High=C0, Allocated=False, Name='<Unknown>')," +
                "MemoryPartition(Low=C0, High=100, Allocated=True, Name='<Unknown>')" +
                "])",
                PartitionRoot.ToString()
            );
        }

        [Fact(Skip = "check")]
        public void AllocateFixedPositionTest()
        {
            var partition1 = PartitionRoot.AllocateLowSize(0x60, 0x40);
            Assert.Equal(
                "MemoryPartition(Low=0, High=100, Allocated=True, Name='<Unknown>', ChildPartitions=[" +
                "MemoryPartition(Low=0, High=60, Allocated=False, Name='<Free>')," +
                "MemoryPartition(Low=60, High=A0, Allocated=True, Name='<Unknown>')," +
                "MemoryPartition(Low=A0, High=100, Allocated=False, Name='<Free>')" +
                "])",
                PartitionRoot.ToString()
            );
        }

        [Fact(Skip = "check")]
        public void MathUtilsPrevAlignedTest()
        {
            Assert.Equal(0x200, (int) MathUtils.PrevAligned(0x260, 0x100));
        }

        [Fact(Skip = "check")]
        public void AllocateAlignedStackOnNonAlignedSegmentTest()
        {
            PartitionRoot = new MemoryPartition(InjectContext, 0x000, 0x260);
            PartitionRoot.Allocate(0x100, MemoryPartition.Anchor.High, Alignment: 0x100);
        }
    }
}