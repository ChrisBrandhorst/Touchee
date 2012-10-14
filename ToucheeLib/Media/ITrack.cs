using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Touchee {

    /// <remarks>
    /// Interface for a track object.
    /// </remarks>
    public interface ITrack : IAudioItem {

        /// <summary>
        /// The title of this track
        /// </summary>
        string Name { get; }
        /// <summary>
        /// The artist of this track
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
        /// The album ID of the track (combination of artist and album name)
        /// </summary>
        string AlbumID { get; }

        /// <summary>
        /// The sorted title of this track
        /// </summary>
        string SortName { get; }
        /// <summary>
        /// The sorted artist of this track
        /// </summary>
        string SortArtist { get; }
        /// <summary>
        /// The sorted album this track appears on
        /// </summary>
        string SortAlbum { get; }
        /// <summary>
        /// The sorted artist of the album this track is on
        /// </summary>
        string SortAlbumArtist { get; }
        /// <summary>
        /// The sorted genre of the track
        /// </summary>
        string SortGenre { get; }

        /// <summary>
        /// The duration of this track
        /// </summary>
        TimeSpan Duration { get; }
        /// <summary>
        /// The track number of this track
        /// </summary>
        int TrackNumber { get; }
        /// <summary>
        /// The disc number of this track
        /// </summary>
        int DiscNumber { get; }

        /// <summary>
        /// The genre of this track
        /// </summary>
        string Genre { get; }
        /// <summary>
        /// The publish year of this track
        /// </summary>
        int Year { get; }
        /// <summary>
        /// The rating for this track
        /// Range is 0 through 100
        /// </summary>
        int Rating { get; }
        /// <summary>
        /// The uri for this track
        /// </summary>
        Uri Uri { get; }

    }


    /// <summary>
    /// Comparer for comparing tracks by their artists
    /// </summary>
    public class ArtistComparer : IComparer<ITrack> {
        int IComparer<ITrack>.Compare(ITrack x, ITrack y) {
            int c;

            // First sort on artist
            if (x.SortArtist != null && y.SortArtist != null)
                c = String.CompareOrdinal(x.SortArtist.ToLower(), y.SortArtist.ToLower());
            else if (x.SortArtist == null && y.SortArtist == null)
                c = 0;
            else
                c = x.SortArtist == null ? 1 : -1;

            // Equal? Then sort on album
            if (c == 0) {
                if (x.SortAlbum != null && y.SortAlbum != null)
                    c = String.CompareOrdinal(x.SortAlbum.ToLower(), y.SortAlbum.ToLower());
                else if (x.SortAlbum == null && y.SortAlbum == null)
                    c = 0;
                else
                    c = x.SortAlbum == null ? 1 : -1;
            }

            return c;
        }
    }


    /// <summary>
    /// Comparer for comparing tracks by their albums
    /// </summary>
    public class AlbumComparer : IComparer<ITrack> {
        int IComparer<ITrack>.Compare(ITrack x, ITrack y) {
            int c;

            // First sort on album
            if (x.SortAlbum != null && y.SortAlbum != null)
                c = String.CompareOrdinal(x.SortAlbum.ToLower(), y.SortAlbum.ToLower());
            else if (x.SortAlbum == null && y.SortAlbum == null)
                c = 0;
            else
                c = x.SortAlbum == null ? 1 : -1;

            // Equal? Then sort on album artist
            if (c == 0) {
                if (x.SortAlbumArtist != null && y.SortAlbumArtist != null)
                    c = String.CompareOrdinal(x.SortAlbumArtist.ToLower(), y.SortAlbumArtist.ToLower());
                else if (x.SortAlbumArtist == null && y.SortAlbumArtist == null)
                    c = 0;
                else
                    c = x.SortAlbumArtist == null ? 1 : -1;
            }

            return c;
        }
    }


}
