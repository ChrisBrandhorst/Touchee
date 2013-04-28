﻿using System;
using System.Collections.Generic;
using System.Linq;
using Touchee.Playback;

namespace Touchee.Server.Responses {

    /// <summary>
    /// Queue response object
    /// </summary>
    public class QueueResponse : ToucheeResponse {

        /// <summary>
        /// The upcoming items
        /// </summary>
        public IEnumerable<object> Items { get; protected set; }

        /// <summary>
        /// The number of upcoming items which belong to the priority count
        /// </summary>
        public int PriorityCount { get; protected set; }

        /// <summary>
        /// Whether shuffling is enabled
        /// </summary>
        public bool Shuffle { get; protected set; }

        /// <summary>
        /// The repeat mode
        /// </summary>
        public string Repeat { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public QueueResponse(Queue queue) {
            if (queue == null) {
                this.Items = new List<object>();
            }
            else {
                this.Items = queue.CurrentAndUpcoming.Take(21).Select(i => QueueItemObject(i));
                this.PriorityCount = queue.UpcomingPriorityCount;
            }
            this.Shuffle = queue.Shuffle;
            this.Repeat = Enum.GetName(typeof(RepeatMode), queue.Repeat).ToCamelCase(false);
        }

        /// <summary>
        /// QueueItem to serialize
        /// </summary>
        object QueueItemObject(QueueItem queueItem) {
            return new {
                MediumID    = queueItem.Container.Medium.Id,
                ContainerID = queueItem.Container.Id,
                Item        = queueItem.Item
            };
        }

    }

}
