using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Linq;
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
            var item = new DummyItem(11, "Eleventh item");
            queue.Push(item);
            Assert.AreEqual(item, queue.Items.ToList().Last());
            Assert.AreEqual(11, queue.Upcoming.Count());
        }

        [TestMethod]
        public void QueuePushPriority() {
            var queue = BuildBasicQueue();
            var item1 = new DummyItem(11, "Eleventh item");
            var item2 = new DummyItem(12, "Eleventh item");

            queue.GoNext();
            queue.GoNext();
            queue.GoNext();
            queue.GoNext();

            queue.PushToPriority(item1);
            queue.PushToPriority(item2);
            Assert.AreEqual(3, queue.Index, "Index");
            Assert.AreEqual(11, queue.Items.Count(), "Count");
            Assert.AreEqual(1, queue.UpcomingPriorityCount, "Upcoming priority count");
            Assert.AreEqual(7, queue.Upcoming.Count(), "Upcoming count");
        }














        Queue BuildBasicQueue() {
            var queue = new Queue();

            queue.Push(new DummyItem(1, "First item"));
            queue.Push(new DummyItem(2, "Second item"));
            queue.Push(new DummyItem(3, "Third item"));
            queue.Push(new DummyItem(4, "Fourth item"));
            queue.Push(new DummyItem(5, "Fifth item"));
            queue.Push(new DummyItem(6, "Sixth item"));
            queue.Push(new DummyItem(7, "Seventh item"));
            queue.Push(new DummyItem(8, "Eight item"));
            queue.Push(new DummyItem(9, "Ninth item"));
            queue.Push(new DummyItem(10, "Tenth item"));

            return queue;
        }



    }

}
