using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Touchee {

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
