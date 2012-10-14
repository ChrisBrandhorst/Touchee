using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Touchee {

    /// <remarks>
    /// Interface for classes which watch the system for Media
    /// </remarks>
    public interface IMediaWatcher {

        /// <summary>
        /// Starts the media detecting
        /// </summary>
        /// <param name="interval">The polling interval to be used. Can be any value when the implementation does not use this</param>
        void Watch(int interval);

        /// <summary>
        /// List of the currently present media which have been detected by this watcher
        /// </summary>
        List<Medium> Media { get; }

    }

}
