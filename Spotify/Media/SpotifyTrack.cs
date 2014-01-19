using System;
using System.IO;
using System.Drawing;
using System.Linq;

using Touchee;
using Touchee.Media;
//using Touchee.Media.Music;

namespace Spotify.Media {

    /// <summary>
    /// Representation of a Spotify track
    /// </summary>
    public class SpotifyTrack : Music.Media.Track {


        #region Statics


        #endregion

        


        #region Constructor

        /// <summary>
        /// Constructs a new Track object
        /// </summary>
        /// <param name="file">The FileInfo object of the music file</param>
        public SpotifyTrack(SpotiFire.Track spTrack) {
            this.Update(spTrack);
        }

        #endregion




        #region Updating


        /// <summary>
        /// Updates the track with the properties of the given Spotify track
        /// </summary>
        /// <param name="spTrack">The Spotify track to update to</param>
        public void Update(SpotiFire.Track spTrack) {
            this.Title = spTrack.Name;
            this.Artist = spTrack.FirstArtist();
            this.Album = spTrack.Album.Name;
            this.AlbumArtist = spTrack.Album.Artist.Name;
            this.DiscNumber = (uint)spTrack.Disc;
            this.TrackNumber = (uint)spTrack.Index;
            this.Duration = spTrack.Duration;
            this.AltId = spTrack.GetLink().ToString();
        }

        #endregion




        #region Properties

        /// <summary>
        /// The artwork for this track
        /// </summary>
        public override Image Artwork {
            get {
                // TODO
                return null;
            }
        }

        #endregion


    }


}
