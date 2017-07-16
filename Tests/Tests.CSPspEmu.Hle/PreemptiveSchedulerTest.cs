using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using CSPspEmu.Hle;
using NUnit.Framework;

namespace CSPspEmu.Tests.Hle
{
    [TestFixture]
    public class PreemptiveSchedulerTest
    {
        public class PreemptiveItem : IPreemptiveItem
        {
            public string Name;
            public int Priority { get; set; }
            public bool Ready { get; set; }

            public override string ToString()
            {
                return String.Format("PreemptiveItem(Name='{0}',Priority={1}, Ready={2})", Name, Priority, Ready);
            }
        }

        PreemptiveScheduler<PreemptiveItem> Scheduler = new PreemptiveScheduler<PreemptiveItem>(NewItemsFirst: false);

        PreemptiveItem PreemptiveItem1 = new PreemptiveItem() {Name = "1", Priority = 10, Ready = true};
        PreemptiveItem PreemptiveItem2 = new PreemptiveItem() {Name = "2", Priority = 10, Ready = true};
        PreemptiveItem PreemptiveItem3 = new PreemptiveItem() {Name = "3", Priority = 10, Ready = false};

        PreemptiveItem PreemptiveItem4 = new PreemptiveItem() {Name = "4", Priority = 8, Ready = true};

        [Test]
        [ExpectedException(typeof(Exception))]
        public void TestNextWithoutItems()
        {
            Scheduler.Next();
        }

        [Test]
        public void TestNextSingleItem()
        {
            Scheduler.Update(PreemptiveItem1);

            for (int n = 0; n < 3; n++)
            {
                Scheduler.Next();
                Assert.AreEqual(PreemptiveItem1, Scheduler.Current);
            }
        }

        [Test]
        public void TestNextTwoItemsWithSamePriority()
        {
            Scheduler.Update(PreemptiveItem1);
            Scheduler.Update(PreemptiveItem2);
            Scheduler.Update(PreemptiveItem3);

            for (int n = 0; n < 3; n++)
            {
                Scheduler.Next();
                Assert.AreEqual(PreemptiveItem1, Scheduler.Current);

                Scheduler.Next();
                Assert.AreEqual(PreemptiveItem2, Scheduler.Current);
            }
        }

        [Test]
        public void TestNextTwoItemsWithSamePriorityAndOtherWithLowerPriority()
        {
            Scheduler.Update(PreemptiveItem1);
            Scheduler.Update(PreemptiveItem2);
            Scheduler.Update(PreemptiveItem3);
            Scheduler.Update(PreemptiveItem4);

            for (int n = 0; n < 3; n++)
            {
                Scheduler.Next();
                Assert.AreEqual(PreemptiveItem1, Scheduler.Current);

                Scheduler.Next();
                Assert.AreEqual(PreemptiveItem2, Scheduler.Current);
            }

            PreemptiveItem1.Ready = false;
            //Scheduler.Put(PreemptiveItem1);

            for (int n = 0; n < 3; n++)
            {
                Scheduler.Next();
                Assert.AreEqual(PreemptiveItem2, Scheduler.Current);
            }

            PreemptiveItem2.Ready = false;

            for (int n = 0; n < 3; n++)
            {
                Scheduler.Next();
                Assert.AreEqual(PreemptiveItem4, Scheduler.Current);
            }

            PreemptiveItem4.Ready = false;
            Scheduler.Update(PreemptiveItem4);

            PreemptiveItem1.Ready = true;
            Scheduler.Update(PreemptiveItem1);

            for (int n = 0; n < 3; n++)
            {
                Scheduler.Next();
                Assert.AreEqual(PreemptiveItem1, Scheduler.Current);
            }

            PreemptiveItem4.Priority = 11;
            PreemptiveItem4.Ready = true;
            Scheduler.Update(PreemptiveItem4);

            for (int n = 0; n < 3; n++)
            {
                Scheduler.Next();
                Assert.AreEqual(PreemptiveItem4, Scheduler.Current);
            }
        }
    }
}