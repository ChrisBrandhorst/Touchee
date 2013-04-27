using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Touchee;
using Touchee.Playback;

namespace ToucheeLibTest {

    public class DummyQueueItem : QueueItem {
        public DummyQueueItem(int id, string name) : base(null, new DummyItem(id, name)) { }
    }

    public class DummyItem : IItem {

        public DummyItem(int id, string name) {
            Id = id;
            Name = name;
        }

        public string Name {
            get;
            protected set;
        }

        public int Id {
            get;
            protected set;
        }

        public string UniqueKey {
            get { return null; }
        }

    }
}
