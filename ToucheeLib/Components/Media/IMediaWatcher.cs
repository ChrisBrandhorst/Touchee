using System.Collections.Generic;

namespace Touchee.Components.Media {

    /// <remarks>
    /// Interface for classes which watch the system for Media
    /// </remarks>
    public interface IMediaWatcher : IComponent {

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
