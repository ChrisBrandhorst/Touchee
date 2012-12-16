using System;
using System.Collections.Generic;
using System.Linq;

using Touchee;
using Touchee.Media.Music;

namespace Music.Media {

    /// <remarks>
    /// Represents a music playlist
    /// </remarks>
    public class Playlist : Container, IPlaylist {


        #region Privates

        // List of tracks
        List<ITrack> _tracks = new List<ITrack>();

        #endregion



        #region Constructor

        public Playlist(Medium medium) : this(medium, "Playlist") { }

        public Playlist(Medium medium, string name) : base(name, medium) { }



        #endregion



        #region IPlaylist implementation


        /// <summary>
        /// The tracks of this playlist
        /// </summary>
        public IEnumerable<ITrack> Tracks { get { return _tracks; } }


        /// <summary>
        /// Add a track to this playlist
        /// </summary>
        /// <param name="track">The track to add</param>
        public void Add(ITrack track) {
            _tracks.Add(track);
        }


        /// <summary>
        /// Add a track to this playlist at a specific position
        /// </summary>
        /// <param name="track">The track to add</param>
        /// <param name="index">The index to add the track</param>
        /// <exception cref="ArgumentOutOfRangeException">If the position exceeds the size of the playlist</exception>
        public void Add(ITrack track, uint index) {
            if (index > _tracks.Count)
                throw new ArgumentOutOfRangeException("The index exceeds the size of the playlist");
            _tracks.Insert((int)index, track);
        }


        /// <summary>
        /// Remove a track from this playlist
        /// </summary>
        /// <param name="track">The track to remove</param>
        public void Remove(ITrack track) {
            _tracks.Remove(track);
        }


        #endregion


        #region Properties


        /// <summary>
        /// The type of the container, e.g. what the container 'looks like'
        /// </summary>
        public override string Type { get { return "playlist"; } }

        /// <summary>
        /// The content type of the container, e.g. what kind of items reside inside this container
        /// </summary>
        public override string ContentType { get { return "music"; } }

        /// <summary>
        /// String array containing names of views by which the contents can be viewed
        /// The first view should be the default one
        /// </summary>
        public override string[] ViewTypes {
            get {
                return new string[]{
                    Music.ViewTypes.Track
                };
            }
        }


        #endregion

    }

}
