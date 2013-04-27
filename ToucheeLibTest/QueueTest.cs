using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Linq;
using System.Collections.Generic;
using Touchee;
using Touchee.Playback;

namespace ToucheeLibTest {
    
    [TestClass]
    public class QueueTest {

        [TestMethod]
        public void QueueStart() {
            var queue = BuildBasicQueue();
            Assert.AreEqual(-1, queue.Index);
            Assert.AreEqual(null, queue.Current);
            Assert.AreEqual(true, queue.IsBeforeFirstItem);
            Assert.AreEqual(false, queue.IsAtFirstItem);
            Assert.AreEqual(false, queue.IsAtLastItem);
            Assert.AreEqual(10, queue.Upcoming.Count());
        }

        [TestMethod]
        public void QueueFirst() {
            var queue = BuildBasicQueue();
            queue.GoNext();
            Assert.AreEqual(0, queue.Index);
            Assert.AreEqual(queue[0], queue.Current);
            Assert.AreEqual(false, queue.IsBeforeFirstItem);
            Assert.AreEqual(true, queue.IsAtFirstItem);
            Assert.AreEqual(false, queue.IsAtLastItem);
            Assert.AreEqual(9, queue.Upcoming.Count());
        }

        [TestMethod]
        public void QueueSecond() {
            var queue = BuildBasicQueue();
            queue.GoNext();
            queue.GoNext();
            Assert.AreEqual(1, queue.Index);
            Assert.AreEqual(queue[1], queue.Current);
            Assert.AreEqual(false, queue.IsBeforeFirstItem);
            Assert.AreEqual(false, queue.IsAtFirstItem);
            Assert.AreEqual(false, queue.IsAtLastItem);
            Assert.AreEqual(8, queue.Upcoming.Count());
        }

        [TestMethod]
        public void QueueLast() {
            var queue = BuildBasicQueue();
            for (var i = 0; i < 10; i++)
                queue.GoNext();
            Assert.AreEqual(9, queue.Index);
            Assert.AreEqual(queue[9], queue.Current);
            Assert.AreEqual(false, queue.IsBeforeFirstItem);
            Assert.AreEqual(false, queue.IsAtFirstItem);
            Assert.AreEqual(true, queue.IsAtLastItem);
            Assert.AreEqual(0, queue.Upcoming.Count());
        }
        
        [TestMethod]
        public void QueueBeyondLast() {
            var queue = BuildBasicQueue();
            for (var i = 0; i < 11; i++)
                queue.GoNext();
            Assert.AreEqual(-1, queue.Index);
            Assert.AreEqual(null, queue.Current);
            Assert.AreEqual(true, queue.IsBeforeFirstItem);
            Assert.AreEqual(false, queue.IsAtFirstItem);
            Assert.AreEqual(false, queue.IsAtLastItem);
            Assert.AreEqual(10, queue.Upcoming.Count());
        }

        [TestMethod]
        public void QueuePush() {
            var queue = BuildBasicQueue();
            var item = new DummyQueueItem(11, "Eleventh item");
            queue.Push(item);
            Assert.AreEqual(item, queue.Items.ToList().Last());
            Assert.AreEqual(11, queue.Upcoming.Count());
        }

        [TestMethod]
        public void QueuePrioritize() {
            var queue = BuildBasicQueue();
            var item1 = new DummyQueueItem(11, "Eleventh item");
            var item2 = new DummyQueueItem(12, "Eleventh item");

            queue.GoNext();
            queue.GoNext();
            queue.GoNext();
            queue.GoNext();

            queue.Prioritize(item1);
            queue.Prioritize(item2);
            Assert.AreEqual(3, queue.Index, "Index");
            Assert.AreEqual(12, queue.Items.Count(), "Count");
            Assert.AreEqual(2, queue.UpcomingPriorityCount, "Upcoming priority count");
            Assert.AreEqual(8, queue.Upcoming.Count(), "Upcoming count");
            Assert.AreEqual(item2, queue.Next, "Next item");

            queue.GoPrev();
            queue.GoPrev();
            Assert.AreEqual(1, queue.Index, "Index II");
            Assert.AreEqual(4, queue.UpcomingPriorityCount, "Upcoming priority count II");
            Assert.AreEqual(10, queue.Upcoming.Count(), "Upcoming count II");
            Assert.AreEqual(3, queue.Next.Item.Id, "Next item ID II");

            queue.GoNext();
            queue.GoNext();
            queue.GoNext();
            queue.GoNext();
            Assert.AreEqual(5, queue.Index, "Index III");
            Assert.AreEqual(0, queue.UpcomingPriorityCount, "Upcoming priority count III");
            Assert.AreEqual(6, queue.Upcoming.Count(), "Upcoming count III");
            Assert.AreEqual(5, queue.Next.Item.Id, "Next item ID III");

            queue.GoNext();
            Assert.AreEqual(10, queue.Items.Count(), "Count IV");
            Assert.AreEqual(0, queue.UpcomingPriorityCount, "Upcoming priority count IV");
            Assert.AreEqual(5, queue.Upcoming.Count(), "Upcoming count IV");
            Assert.AreEqual(6, queue.Next.Item.Id, "Next item ID IV");

            queue.GoPrev();
            Assert.AreEqual(4, queue.Current.Item.Id, "Current item ID V");

        }

        [TestMethod]
        public void QueuePriorityClear() {
            var queue = BuildBasicQueue();
            var item1 = new DummyQueueItem(11, "Prio 1");
            var item2 = new DummyQueueItem(12, "Prio 2");
            var item3 = new DummyQueueItem(13, "Prio 3");
            var item4 = new DummyQueueItem(14, "Prio 4");

            queue.GoNext();
            queue.GoNext();
            queue.GoNext();
            queue.GoNext();

            queue.PushToPriority(new List<QueueItem>() { item1, item2, item3, item4 });
            Assert.AreEqual(14, queue.Items.Count(), "Count");
            Assert.AreEqual(4, queue.UpcomingPriorityCount, "Upcoming priority count");

            queue.ClearPriority();
            Assert.AreEqual(10, queue.Items.Count(), "Count II");
            Assert.AreEqual(0, queue.UpcomingPriorityCount, "Upcoming priority count II");

            queue.PushToPriority(new List<QueueItem>() { item1, item2, item3, item4 });
            queue.GoNext();
            queue.GoNext();
            queue.ClearPriority();
            Assert.AreEqual(12, queue.Items.Count(), "Count III");
            Assert.AreEqual(0, queue.UpcomingPriorityCount, "Upcoming priority count III");
        }












        Queue BuildBasicQueue() {
            var queue = new Queue();

            queue.Push(new DummyQueueItem(1, "First item"));
            queue.Push(new DummyQueueItem(2, "Second item"));
            queue.Push(new DummyQueueItem(3, "Third item"));
            queue.Push(new DummyQueueItem(4, "Fourth item"));
            queue.Push(new DummyQueueItem(5, "Fifth item"));
            queue.Push(new DummyQueueItem(6, "Sixth item"));
            queue.Push(new DummyQueueItem(7, "Seventh item"));
            queue.Push(new DummyQueueItem(8, "Eight item"));
            queue.Push(new DummyQueueItem(9, "Ninth item"));
            queue.Push(new DummyQueueItem(10, "Tenth item"));

            return queue;
        }



    }

}
