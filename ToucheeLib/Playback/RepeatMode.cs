using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Touchee.Playback {

    /// <remarks>
    /// Repeat modes for playback of a queue
    /// </remarks>
    public enum RepeatMode : int {
        /// <summary>
        /// No repeat
        /// </summary>
        Off = 0,
        /// <summary>
        /// Repeat all items in the queue
        /// </summary>
        All = 1,
        /// <summary>
        /// Repeat the current item in queue
        /// </summary>
        One = 2
    }

}
