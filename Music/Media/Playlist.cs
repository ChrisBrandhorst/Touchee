//using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

using Touchee;
using Touchee.Media.Music;

namespace Music.Media {

    /// <remarks>
    /// Represents a music playlist
    /// </remarks>
    public class Playlist : Container, IPlaylist {


        #region Privates

        // List of tracks
        IEnumerable<ITrack> _tracks = new List<ITrack>();

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


        #endregion


        #region Properties


        /// <summary>
        /// The type of the container, e.g. what the container 'looks like'
        /// </summary>
        public override string Type { get { return ContainerType.Playlist; } }

        /// <summary>
        /// The content type of the container, e.g. what kind of items reside inside this container
        /// </summary>
        public override string ContentType { get { return ContainerContentType.Music; } }

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
