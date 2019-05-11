using System;
using CSPspEmu.Hle;
using Xunit;

namespace Tests.CSPspEmu.Hle
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

        PreemptiveScheduler<PreemptiveItem> _scheduler = new PreemptiveScheduler<PreemptiveItem>(NewItemsFirst: false);

        PreemptiveItem _preemptiveItem1 = new PreemptiveItem() {Name = "1", Priority = 10, Ready = true};
        PreemptiveItem _preemptiveItem2 = new PreemptiveItem() {Name = "2", Priority = 10, Ready = true};
        PreemptiveItem _preemptiveItem3 = new PreemptiveItem() {Name = "3", Priority = 10, Ready = false};

        PreemptiveItem _preemptiveItem4 = new PreemptiveItem() {Name = "4", Priority = 8, Ready = true};

        [Fact(Skip = "check")]
        public void TestNextWithoutItems()
        {
            Assert.Throws<Exception>(() => { _scheduler.Next(); });
        }

        [Fact]
        public void TestNextSingleItem()
        {
            _scheduler.Update(_preemptiveItem1);

            for (int n = 0; n < 3; n++)
            {
                _scheduler.Next();
                Assert.Equal(_preemptiveItem1, _scheduler.Current);
            }
        }

        [Fact]
        public void TestNextTwoItemsWithSamePriority()
        {
            _scheduler.Update(_preemptiveItem1);
            _scheduler.Update(_preemptiveItem2);
            _scheduler.Update(_preemptiveItem3);

            for (int n = 0; n < 3; n++)
            {
                _scheduler.Next();
                Assert.Equal(_preemptiveItem1, _scheduler.Current);

                _scheduler.Next();
                Assert.Equal(_preemptiveItem2, _scheduler.Current);
            }
        }

        [Fact]
        public void TestNextTwoItemsWithSamePriorityAndOtherWithLowerPriority()
        {
            _scheduler.Update(_preemptiveItem1);
            _scheduler.Update(_preemptiveItem2);
            _scheduler.Update(_preemptiveItem3);
            _scheduler.Update(_preemptiveItem4);

            for (int n = 0; n < 3; n++)
            {
                _scheduler.Next();
                Assert.Equal(_preemptiveItem1, _scheduler.Current);

                _scheduler.Next();
                Assert.Equal(_preemptiveItem2, _scheduler.Current);
            }

            _preemptiveItem1.Ready = false;
            //Scheduler.Put(PreemptiveItem1);

            for (int n = 0; n < 3; n++)
            {
                _scheduler.Next();
                Assert.Equal(_preemptiveItem2, _scheduler.Current);
            }

            _preemptiveItem2.Ready = false;

            for (int n = 0; n < 3; n++)
            {
                _scheduler.Next();
                Assert.Equal(_preemptiveItem4, _scheduler.Current);
            }

            _preemptiveItem4.Ready = false;
            _scheduler.Update(_preemptiveItem4);

            _preemptiveItem1.Ready = true;
            _scheduler.Update(_preemptiveItem1);

            for (int n = 0; n < 3; n++)
            {
                _scheduler.Next();
                Assert.Equal(_preemptiveItem1, _scheduler.Current);
            }

            _preemptiveItem4.Priority = 11;
            _preemptiveItem4.Ready = true;
            _scheduler.Update(_preemptiveItem4);

            for (int n = 0; n < 3; n++)
            {
                _scheduler.Next();
                Assert.Equal(_preemptiveItem4, _scheduler.Current);
            }
        }
    }
}