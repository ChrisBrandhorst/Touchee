using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Touchee.Playback {

    /// <summary>
    /// Class for items in the queue.
    /// Ensures we can trace back from which containers the items were added to the queue.
    /// </summary>
    public class QueueItem {


        /// <summary>
        /// The Container the Item was added from
        /// </summary>
        public Container Container { get; protected set; }


        /// <summary>
        /// The Item in the queue
        /// </summary>
        public IItem Item { get; protected set; }


        /// <summary>
        /// Constructs a new QueueItem
        /// </summary>
        /// <param name="container">The container of the item</param>
        /// <param name="item">The item in the queue</param>
        public QueueItem(Container container, IItem item) {
            this.Container = container;
            this.Item = item;
        }

    }
}
