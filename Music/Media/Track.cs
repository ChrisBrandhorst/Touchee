﻿using System;
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

        // Internal title variable
        string _title;
        // Internal artist variable
        string _artist;
        // Internal album variable
        string _album;
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

        // Internal genre variable
        string _genre;

        #endregion



        #region Properties


        /// <summary>
        /// Gets or sets the title of this track.
        /// If TitleSort is null when this value is set, the TitleSort value is automatically set
        /// to a sort variant of the given value (see String#ToSortName).
        /// </summary>
        //[DataMember(EmitDefaultValue = false)]
        [DataMember]
        public string Title {
            get { return _title; }
            protected set { if (!string.IsNullOrWhiteSpace(value)) _title = value; }
        }
        /// <summary>
        /// Gets or sets the artist of this track.
        /// If ArtistSort is null when this value is set, the ArtistSort value is automatically set
        /// to a sort variant of the given value (see String#ToSortName).
        /// </summary>
        //[DataMember(EmitDefaultValue = false)]
        [DataMember]
        public string Artist {
            get { return _artist ?? _albumArtist; }
            protected set { if (!string.IsNullOrWhiteSpace(value)) _artist = value; }
        }
        /// <summary>
        /// Gets or sets the album of this track.
        /// If AlbumSort is null when this value is set, the AlbumSort value is automatically set
        /// to a sort variant of the given value (see String#ToSortName).
        /// </summary>
        //[DataMember(EmitDefaultValue = false)]
        [DataMember]
        public string Album {
            get { return _album; }
            protected set { if (!string.IsNullOrWhiteSpace(value)) _album = value; }
        }
        /// <summary>
        /// Gets or sets the album artist of this track.
        /// If AlbumArtistSort is null when this value is set, the AlbumArtistSort value is automatically set
        /// to a sort variant of the given value (see String#ToSortName).
        /// </summary>
        //[DataMember(EmitDefaultValue = false)]
        [DataMember]
        public string AlbumArtist {
            get { return _albumArtist; }
            protected set { if (!string.IsNullOrWhiteSpace(value)) _albumArtist = value; }
        }


        /// <summary>
        /// The sorted title of this track
        /// </summary>
        //[DataMember(EmitDefaultValue = false)]
        [DataMember]
        public string TitleSort {
            get { return Util.ToSortName(_titleSort ?? Title); }
            protected set { if (!string.IsNullOrWhiteSpace(value)) _titleSort = value; }
        }
        /// <summary>
        /// The sort value of the performing artist of this track
        /// </summary>
        //[DataMember(EmitDefaultValue = false)]
        [DataMember]
        public string ArtistSort {
            get { return Util.ToSortName(_artistSort ?? Artist); }
            protected set { if (!string.IsNullOrWhiteSpace(value)) _artistSort = value; }
        }
        /// <summary>
        /// The sorted album this track appears on
        /// </summary>
        //[DataMember(EmitDefaultValue = false)]
        [DataMember]
        public string AlbumSort {
            get { return Util.ToSortName(_albumSort ?? Album); }
            protected set { if (!string.IsNullOrWhiteSpace(value)) _albumSort = value; }
        }
        /// <summary>
        /// The sort value of the album artist of this track
        /// </summary>
        //[DataMember(EmitDefaultValue = false)]
        [DataMember]
        public string AlbumArtistSort {
            get { return Util.ToSortName(_albumArtistSort ?? AlbumArtist); }
            protected set { if (!string.IsNullOrWhiteSpace(value)) _albumArtistSort = value; }
        }


        /// <summary>
        /// The genre of this track
        /// </summary>
        //[DataMember(EmitDefaultValue = false)]
        [DataMember]
        public string Genre {
            get { return _genre; }
            protected set { if (!string.IsNullOrWhiteSpace(value)) _genre = value; }
        }
        /// <summary>
        /// The sort value of the genre of this track
        /// </summary>
        //[DataMember(EmitDefaultValue = false)]
        [DataMember]
        public string GenreSort { get { return Util.ToSortName(Genre); } }



        /// <summary>
        /// The publish year of this track
        /// </summary>
        //[DataMember(EmitDefaultValue = false)]
        [DataMember]
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
        //[DataMember(EmitDefaultValue = false)]
        [DataMember]
        public uint TrackNumber { get; protected set; }
        /// <summary>
        /// The disc number of this track
        /// </summary>
        //[DataMember(EmitDefaultValue = false)]
        [DataMember]
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


        /// <summary>
        /// The album ID for this track
        /// </summary>
        //[DataMember(EmitDefaultValue = false)]
        [DataMember]
        public virtual string AlbumID { get {
            return ((Album ?? Util.NullSortValue) + "|" + (AlbumArtist ?? Artist ?? Util.NullSortValue)).ToLower().GetInt64HashCode();
        } }


        #endregion

        

        /// <summary>
        /// Gets all tracks for the album of the given track
        /// </summary>
        public static IEnumerable<Track> GetTracksOfAlbum(Track track) {
            return Track.GetTracksOfAlbum(track.AlbumID);
        }


        /// <summary>
        /// Gets all tracks with the given albumID
        /// </summary>
        public static IEnumerable<Track> GetTracksOfAlbum(string albumID) {
            return Track.Where(t => t.AlbumID == albumID);
        }


    }

}
