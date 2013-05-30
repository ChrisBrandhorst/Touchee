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


        /// <summary>
        /// Constructs a playlist, giving it the name "Playlist"
        /// </summary>
        /// <param name="medium">The Medium the playlist resides in</param>
        public Playlist(Medium medium) : this("Playlist", medium) { }



        /// <summary>
        /// Constructs a playlist
        /// </summary>
        /// <param name="name">The name of the playlist</param>
        /// <param name="medium">The Medium the playlist resides in</param>
        public Playlist(string name, Medium medium) : base(name, medium) { }


        #endregion



        #region IPlaylist implementation


        /// <summary>
        /// The tracks of this playlist
        /// </summary>
        public virtual IEnumerable<ITrack> Tracks { get { return _tracks; } }


        /// <summary>
        /// Add a track to this playlist
        /// </summary>
        /// <param name="track">The track to add</param>
        public virtual void Add(ITrack track) {
            lock (_tracks) {
                _tracks.Add(track);
            }
            this.NotifyContentsChanged();
        }


        /// <summary>
        /// Add a track to this playlist at a specific position
        /// </summary>
        /// <param name="track">The track to add</param>
        /// <param name="index">The index to add the track</param>
        /// <exception cref="ArgumentOutOfRangeException">If the position exceeds the size of the playlist</exception>
        public virtual void Add(ITrack track, uint index) {
            if (index > _tracks.Count)
                throw new ArgumentOutOfRangeException("The index exceeds the size of the playlist");
            lock (_tracks) {
                _tracks.Insert((int)index, track);
            }
            this.NotifyContentsChanged();
        }


        /// <summary>
        /// Remove a track from this playlist
        /// </summary>
        /// <param name="track">The track to remove</param>
        public virtual bool Remove(ITrack track) {
            bool removed;
            lock (_tracks) {
                removed = _tracks.Remove(track);
            }
            this.NotifyContentsChanged();
            return removed;
        }


        /// <summary>
        /// Can be called when a track is updated
        /// </summary>
        /// <param name="track">The track that is updated</param>
        public virtual void Update(ITrack track) {
            this.NotifyContentsChanged();
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
