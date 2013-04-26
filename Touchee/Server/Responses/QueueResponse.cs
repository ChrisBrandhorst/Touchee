using System.Collections.Generic;
using System.Linq;
using Touchee.Playback;

namespace Touchee.Server.Responses {

    /// <summary>
    /// Queue response object
    /// </summary>
    public class QueueResponse : ToucheeResponse {

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<IItem> Items { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public int PriorityCount { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public QueueResponse(Queue queue) {
            this.Items = queue.Upcoming.Take(20);
            this.PriorityCount = queue.UpcomingPriorityCount;
        }

    }

}
