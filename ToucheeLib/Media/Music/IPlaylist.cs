using System.Collections.Generic;

namespace Touchee.Media.Music {

    /// <remarks>
    /// Interface for a music playlist object.
    /// </remarks>
    public interface IPlaylist {

        /// <summary>
        /// The title of this playlist
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The tracks of this playlist
        /// </summary>
        IEnumerable<ITrack> Tracks { get; }

    }

}
