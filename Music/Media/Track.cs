using System;
using System.IO;
using System.Drawing;
using System.Runtime.Serialization;
using System.Linq;
using System.Collections.Generic;

using Touchee;
using Touchee.Media;
using Touchee.Media.Music;

namespace Music.Media {

    /// <summary>
    /// Abstract class for all tracks which should be collected into the library
    /// </summary>
    [DataContract(Namespace = "")]
    public abstract class Track : Collectable<Track>, ITrack {


        #region Statics


        /// <summary>
        /// Gets the first track which corresponds with the given uri
        /// </summary>
        /// <param name="uri">The uri to search for</param>
        /// <returns>The track with the given uri, or null if none found</returns>
        public static Track GetByUri(Uri uri) {
            return Media.Track.FirstOrDefault(t => t.Uri.Equals(uri));
        }


        #endregion



        #region Privates

        // Internal artist variable
        string _artist;
        // Internal album artist variable
        string _albumArtist;

        // Internal title sort variable
        string _titleSort;
        // Internal artist sort variable
        string _artistSort;
        // Internal album sort variable
        string _albumSort;
        // Internal album artist sort variable
        string _albumArtistSort;

        #endregion



        #region Properties


        /// <summary>
        /// The medium this track is on
        /// </summary>
        public Medium Medium { get; protected set; }

        /// <summary>
        /// Gets or sets the title of this track.
        /// If TitleSort is null when this value is set, the TitleSort value is automatically set
        /// to a sort variant of the given value (see String#ToSortName).
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Title { get; protected set; }
        /// <summary>
        /// Gets or sets the artist of this track.
        /// If ArtistSort is null when this value is set, the ArtistSort value is automatically set
        /// to a sort variant of the given value (see String#ToSortName).
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Artist {
            get { return _artist ?? _albumArtist; }
            protected set { _artist = value; }
        }
        /// <summary>
        /// Gets or sets the album of this track.
        /// If AlbumSort is null when this value is set, the AlbumSort value is automatically set
        /// to a sort variant of the given value (see String#ToSortName).
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Album { get; protected set; }
        /// <summary>
        /// Gets or sets the album artist of this track.
        /// If AlbumArtistSort is null when this value is set, the AlbumArtistSort value is automatically set
        /// to a sort variant of the given value (see String#ToSortName).
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string AlbumArtist {
            get { return _albumArtist; }
            protected set { _albumArtist = value; }
        }


        /// <summary>
        /// The sorted title of this track
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string TitleSort {
            get { return Util.ToSortName(_titleSort ?? Title); }
            protected set { _titleSort = value; }
        }
        /// <summary>
        /// The sort value of the performing artist of this track
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ArtistSort {
            get { return Util.ToSortName(_artistSort ?? Artist); }
            protected set { _artistSort = value; }
        }
        /// <summary>
        /// The sorted album this track appears on
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string AlbumSort {
            get { return Util.ToSortName(_albumSort ?? Album); }
            protected set { _albumSort = value; }
        }
        /// <summary>
        /// The sort value of the album artist of this track
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string AlbumArtistSort {
            get { return Util.ToSortName(_albumArtistSort ?? AlbumArtist); }
            protected set { _albumArtistSort = value; }
        }


        /// <summary>
        /// The genre of this track
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Genre { get; protected set; }
        /// <summary>
        /// The sort value of the genre of this track
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string GenreSort { get { return Util.ToSortName(Genre); } }



        /// <summary>
        /// The publish year of this track
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public uint Year { get; protected set; }


        /// <summary>
        /// The duration of this track
        /// </summary>
        public TimeSpan Duration { get; protected set; }

        /// <summary>
        /// The duration of this track in milliseconds
        /// </summary>
        [DataMember(Name = "Duration")]
        int Milliseconds {
            get { return this.Duration == null ? 0 : (int)this.Duration.TotalMilliseconds; }
            set { this.Duration = new TimeSpan(0, 0, 0, 0, value); }
        }

        /// <summary>
        /// The track number of this track
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public uint TrackNumber { get; protected set; }
        /// <summary>
        /// The disc number of this track
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public uint DiscNumber { get; protected set; }


        /// <summary>
        /// The uri for this track
        /// </summary>
        public Uri Uri { get; protected set; }


        /// <summary>
        /// The application-wide, unique key string for this item.
        /// Is equal to Uri.ToString().
        /// </summary>
        public string UniqueKey { get { return this.Uri.ToString(); } }


        /// <summary>
        /// The artwork for this track
        /// </summary>
        public abstract Image Artwork { get; }


        #endregion

        

        /// <summary>
        /// Gets all tracks for the album of the given tracl
        /// </summary>
        public static IEnumerable<Track> GetAlbum(Track track) {
            return Track
                .Where(t => t.Album == track.Album && (t.AlbumArtist ?? t.Artist) == (track.AlbumArtist ?? track.Artist))
                .OrderBy(t => t.DiscNumber)
                .ThenBy(t => t.TrackNumber)
                .ThenBy(t => t.TitleSort);
        }


    }

}
