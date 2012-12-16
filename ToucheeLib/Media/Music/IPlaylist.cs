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

        /// <summary>
        /// Add a track to this playlist
        /// </summary>
        /// <param name="track">The track to add</param>
        void Add(ITrack track);

        /// <summary>
        /// Add a track to this playlist at a specific position
        /// </summary>
        /// <param name="track">The track to add</param>
        /// <param name="position">The position to add the track</param>
        void Add(ITrack track, uint position);

        /// <summary>
        /// Remove a track from this playlist
        /// </summary>
        /// <param name="track">The track to remove</param>
        void Remove(ITrack track);


    }

}
