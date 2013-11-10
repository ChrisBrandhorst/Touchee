using System;
using System.Collections.Generic;
using System.Linq;

using Touchee;
using Touchee.Media.Music;

namespace Spotify.Media {

    /// <remarks>
    /// 
    /// </remarks>
    public class MasterPlaylist : Container, IPlaylist {


        #region Privates

        #endregion



        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="medium"></param>
        public MasterPlaylist(Medium medium) : base(medium.Name, medium) { }

        #endregion




        #region IPlaylist implementation


        /// <summary>
        /// The tracks of this playlist
        /// </summary>
        public virtual IEnumerable<ITrack> Tracks { get {
            return this.Medium.Containers
                .Where(pl => pl != this)
                .SelectMany(pl => ((Playlist)pl).Tracks)
                .Distinct();
        } }


        public virtual void Add(ITrack track) {
            throw new NotImplementedException("This should not happen");
        }
        public virtual void Add(ITrack track, uint index) {
            throw new NotImplementedException("This should not happen");
        }
        public virtual bool Remove(ITrack track) {
            throw new NotImplementedException("This should not happen");
        }
        public virtual void Update(ITrack track) {
            throw new NotImplementedException("This should not happen");
        }

        #endregion




        #region Properties


        /// <summary>
        /// The type of the container, e.g. what the container 'looks like'
        /// </summary>
        public override string Type { get { return "master_playlist"; } }

        /// <summary>
        /// The content type of the container, e.g. what kind of items reside inside this container
        /// </summary>
        public override string ContentType { get { return "music"; } }

        /// <summary>
        /// String array containing names of views by which the contents can be viewed
        /// The first view should be the default one
        /// </summary>
        public override string[] Views {
            get {
                return new string[]{
                    "track",
                    "artist",
                    "album",
                    "genre"
                };
            }
        }


        #endregion

    }

}
