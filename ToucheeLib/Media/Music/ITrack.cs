using System;
using System.Drawing;
using System.Collections.Generic;

namespace Touchee.Media.Music {


    /// <remarks>
    /// Interface for a music track
    /// </remarks>
    public interface ITrack : IAudioItem {


        /// <summary>
        /// The title of this track
        /// </summary>
        string Title { get; }
        /// <summary>
        /// The performing artist of this track
        /// </summary>
        string Artist { get; }
        /// <summary>
        /// The album this track appears on
        /// </summary>
        string Album { get; }
        /// <summary>
        /// The artist of the album this track is on
        /// </summary>
        string AlbumArtist { get; }


        /// <summary>
        /// The sorted title of this track
        /// </summary>
        string TitleSort { get; }
        /// <summary>
        /// The sort value of the performing artist of this track
        /// </summary>
        string ArtistSort { get; }
        /// <summary>
        /// The sorted album this track appears on
        /// </summary>
        string AlbumSort { get; }
        /// <summary>
        /// The sort value of the album artist of this track
        /// </summary>
        string AlbumArtistSort { get; }


        /// <summary>
        /// The genre of this track
        /// </summary>
        string Genre { get; }
        /// <summary>
        /// The publish year of this track
        /// </summary>
        uint Year { get; }


        /// <summary>
        /// The duration of this track
        /// </summary>
        TimeSpan Duration { get; }
        /// <summary>
        /// The track number of this track
        /// </summary>
        uint TrackNumber { get; }
        /// <summary>
        /// The disc number of this track
        /// </summary>
        uint DiscNumber { get; }

        
        /// <summary>
        /// The uri for this track
        /// </summary>
        Uri Uri { get; }

        /// <summary>
        /// The artwork for this track
        /// </summary>
        Image Artwork { get; }

    }

}
