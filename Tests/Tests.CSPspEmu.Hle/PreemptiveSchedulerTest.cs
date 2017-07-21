using System;
using CSPspEmu.Hle;
using Xunit;


namespace CSPspEmu.Tests.Hle
{
    
    public class PreemptiveSchedulerTest
    {
        public class PreemptiveItem : IPreemptiveItem
        {
            public string Name;
            public int Priority { get; set; }
            public bool Ready { get; set; }

            public override string ToString()
            {
                return $"PreemptiveItem(Name='{Name}',Priority={Priority}, Ready={Ready})";
            }
        }

        PreemptiveScheduler<PreemptiveItem> Scheduler = new PreemptiveScheduler<PreemptiveItem>(NewItemsFirst: false);

        PreemptiveItem PreemptiveItem1 = new PreemptiveItem() {Name = "1", Priority = 10, Ready = true};
        PreemptiveItem PreemptiveItem2 = new PreemptiveItem() {Name = "2", Priority = 10, Ready = true};
        PreemptiveItem PreemptiveItem3 = new PreemptiveItem() {Name = "3", Priority = 10, Ready = false};

        PreemptiveItem PreemptiveItem4 = new PreemptiveItem() {Name = "4", Priority = 8, Ready = true};

        [Fact(Skip = "check")]
        public void TestNextWithoutItems()
        {
            Assert.Throws<Exception>(() => { Scheduler.Next(); });
        }

        [Fact]
        public void TestNextSingleItem()
        {
            Scheduler.Update(PreemptiveItem1);

            for (int n = 0; n < 3; n++)
            {
                Scheduler.Next();
                Assert.Equal(PreemptiveItem1, Scheduler.Current);
            }
        }

        [Fact]
        public void TestNextTwoItemsWithSamePriority()
        {
            Scheduler.Update(PreemptiveItem1);
            Scheduler.Update(PreemptiveItem2);
            Scheduler.Update(PreemptiveItem3);

            for (int n = 0; n < 3; n++)
            {
                Scheduler.Next();
                Assert.Equal(PreemptiveItem1, Scheduler.Current);

                Scheduler.Next();
                Assert.Equal(PreemptiveItem2, Scheduler.Current);
            }
        }

        [Fact]
        public void TestNextTwoItemsWithSamePriorityAndOtherWithLowerPriority()
        {
            Scheduler.Update(PreemptiveItem1);
            Scheduler.Update(PreemptiveItem2);
            Scheduler.Update(PreemptiveItem3);
            Scheduler.Update(PreemptiveItem4);

            for (int n = 0; n < 3; n++)
            {
                Scheduler.Next();
                Assert.Equal(PreemptiveItem1, Scheduler.Current);

                Scheduler.Next();
                Assert.Equal(PreemptiveItem2, Scheduler.Current);
            }

            PreemptiveItem1.Ready = false;
            //Scheduler.Put(PreemptiveItem1);

            for (int n = 0; n < 3; n++)
            {
                Scheduler.Next();
                Assert.Equal(PreemptiveItem2, Scheduler.Current);
            }

            PreemptiveItem2.Ready = false;

            for (int n = 0; n < 3; n++)
            {
                Scheduler.Next();
                Assert.Equal(PreemptiveItem4, Scheduler.Current);
            }

            PreemptiveItem4.Ready = false;
            Scheduler.Update(PreemptiveItem4);

            PreemptiveItem1.Ready = true;
            Scheduler.Update(PreemptiveItem1);

            for (int n = 0; n < 3; n++)
            {
                Scheduler.Next();
                Assert.Equal(PreemptiveItem1, Scheduler.Current);
            }

            PreemptiveItem4.Priority = 11;
            PreemptiveItem4.Ready = true;
            Scheduler.Update(PreemptiveItem4);

            for (int n = 0; n < 3; n++)
            {
                Scheduler.Next();
                Assert.Equal(PreemptiveItem4, Scheduler.Current);
            }
        }
    }
}