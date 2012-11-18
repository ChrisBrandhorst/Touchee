using System;
using System.IO;

using Touchee;
using Touchee.Media;
using Touchee.Media.Music;

using System.Runtime.Serialization;
using System.Xml;

namespace Music.Media {

    [DataContract(Namespace = "")]
    public class Track : Collectable<Track>, ITrack {


        #region Statics


        /// <summary>
        /// Gets the first track which corresponds with the given file path
        /// </summary>
        /// <param name="path">The path to search for</param>
        /// <returns>The track with the given path, or null if none found</returns>
        public static Track GetByPath(string path) {
            var uri = new Uri(path);
            return Media.Track.FirstOrDefault(t => t.Uri.Equals(uri));
        }


        #endregion



        #region Constructor

        /// <summary>
        /// Constructs a new Track object
        /// </summary>
        /// <param name="medium">The medium this track is from</param>
        /// <param name="file">The FileInfo object of the music file</param>
        public Track(Medium medium, FileInfo file) {
            this.Medium = medium;
            this.Update(file);
        }

        #endregion



        #region Updating


        /// <summary>
        /// Updates the track with the properties of the given file
        /// </summary>
        /// <param name="file">The file to update to</param>
        public void Update(FileInfo file) {

            // Set the Uri
            this.Uri = new Uri(file.FullName);

            // Get the tag for the file
            TagLib.Tag tag = null;
            TagLib.File tagFile = null;
            try {
                tagFile = TagLib.File.Create(file.FullName);
                tag = tagFile.Tag;
            }
            catch (Exception) {
                Log("Could not parse tags for file " + file.FullName);
            }

            // Bail out if we have no tag
            if (tagFile == null) return;

            // Set properties
            if (!String.IsNullOrEmpty(tag.Album))
                this.Album = tag.Album;
            if (!String.IsNullOrEmpty(tag.JoinedAlbumArtists))
                this.AlbumArtist = tag.JoinedAlbumArtists; // Also sets AlbumArtistSort
            if (!String.IsNullOrEmpty(tag.JoinedPerformers))
                this.Artist = tag.JoinedPerformers;
            if (!String.IsNullOrEmpty(tag.JoinedPerformersSort))
                this.ArtistSort = tag.JoinedPerformersSort;
            this.DiscNumber = tag.Disc;
            this.Duration = tagFile.Properties.Duration;
            if (!String.IsNullOrEmpty(tag.JoinedGenres))
                this.Genre = tag.JoinedGenres;
            if (!String.IsNullOrEmpty(tag.Title))
                this.Title = tag.Title;
            if (!String.IsNullOrEmpty(tag.TitleSort))
                this.TitleSort = tag.TitleSort;
            this.TrackNumber = tag.Track;
            this.Year = tag.Year;
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
        [DataMember(EmitDefaultValue=false)]
        public string Title {
            get { return _title; }
            set {
                _title = value;
                if (TitleSort == null && _title != null)
                    TitleSort = _title.ToSortName();
            }
        }
        /// <summary>
        /// Gets or sets the artist of this track.
        /// If ArtistSort is null when this value is set, the ArtistSort value is automatically set
        /// to a sort variant of the given value (see String#ToSortName).
        /// </summary>
        [DataMember(EmitDefaultValue=false)]
        public string Artist {
            get { return _artist; }
            set {
                _artist = value;
                if (ArtistSort == null && _artist != null)
                    ArtistSort = _artist.ToSortName();
            }
        }
        /// <summary>
        /// Gets or sets the album of this track.
        /// If AlbumSort is null when this value is set, the AlbumSort value is automatically set
        /// to a sort variant of the given value (see String#ToSortName).
        /// </summary>
        [DataMember(EmitDefaultValue=false)]
        public string Album {
            get { return _album; }
            set {
                _album = value;
                if (AlbumSort == null && _album != null)
                    AlbumSort = _album.ToSortName();
            }
        }
        /// <summary>
        /// Gets or sets the album artist of this track.
        /// If AlbumArtistSort is null when this value is set, the AlbumArtistSort value is automatically set
        /// to a sort variant of the given value (see String#ToSortName).
        /// </summary>
        [DataMember(EmitDefaultValue=false)]
        public string AlbumArtist {
            get { return _albumArtist; }
            set {
                _albumArtist = value;
                if (AlbumArtistSort == null && _albumArtist != null)
                    AlbumArtistSort = _albumArtist.ToSortName();
            }
        }


        /// <summary>
        /// The sorted title of this track
        /// </summary>
        [DataMember(EmitDefaultValue=false)]
        public string TitleSort { get; protected set; }
        /// <summary>
        /// The sort value of the performing artist of this track
        /// </summary>
        [DataMember(EmitDefaultValue=false)]
        public string ArtistSort { get; protected set; }
        /// <summary>
        /// The sorted album this track appears on
        /// </summary>
        [DataMember(EmitDefaultValue=false)]
        public string AlbumSort { get; protected set; }
        /// <summary>
        /// The sort value of the album artist of this track
        /// </summary>
        [DataMember(EmitDefaultValue=false)]
        public string AlbumArtistSort { get; protected set; }


        /// <summary>
        /// The genre of this track
        /// </summary>
        [DataMember(EmitDefaultValue=false)]
        public string Genre { get; protected set; }
        /// <summary>
        /// The publish year of this track
        /// </summary>
        [DataMember(EmitDefaultValue=false)]
        public uint Year { get; protected set; }


        /// <summary>
        /// The duration of this track
        /// </summary>
        [DataMember(EmitDefaultValue=false)]
        public TimeSpan Duration { get; protected set; }
        /// <summary>
        /// The track number of this track
        /// </summary>
        [DataMember(EmitDefaultValue=false)]
        public uint TrackNumber { get; protected set; }
        /// <summary>
        /// The disc number of this track
        /// </summary>
        [DataMember(EmitDefaultValue=false)]
        public uint DiscNumber { get; protected set; }


        /// <summary>
        /// The uri for this track
        /// </summary>
        [DataMember(EmitDefaultValue=false)]
        public Uri Uri { get; protected set; }


        /// <summary>
        /// The application-wide, unique key string for this item.
        /// Is equal to Uri.ToString().
        /// </summary>
        public string UniqueKey { get { return this.Uri.ToString(); } }

        #endregion


    }




}
