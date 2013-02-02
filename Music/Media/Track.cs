using System;
using System.IO;
using System.Drawing;
using System.Runtime.Serialization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

using Touchee;
using Touchee.Media;
using Touchee.Media.Music;

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
        /// <param name="file">The FileInfo object of the music file</param>
        public Track(FileInfo file) {
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
            catch (Exception e) {
                Log("Could not parse tags for file " + file.FullName);
            }

            // Set properties if we have a tag
            if (tagFile != null) {
                if (!String.IsNullOrEmpty(tag.TitleSort))
                    this.TitleSort = Util.ToSortName(tag.TitleSort);
                if (!String.IsNullOrEmpty(tag.Title))
                    this.Title = tag.Title;

                if (!String.IsNullOrEmpty(tag.JoinedPerformersSort))
                    this.ArtistSort = Util.ToSortName(tag.JoinedPerformersSort);
                if (!String.IsNullOrEmpty(tag.JoinedPerformers))
                    this.Artist = tag.JoinedPerformers;

                if (!String.IsNullOrEmpty(tag.AlbumSort))
                    this.AlbumSort = Util.ToSortName(tag.AlbumSort);
                if (!String.IsNullOrEmpty(tag.Album))
                    this.Album = tag.Album;

                if (tag.AlbumArtistsSort.Length > 0)
                    this.AlbumSort = Util.ToSortName(String.Join("; ", tag.AlbumArtistsSort));
                if (!String.IsNullOrEmpty(tag.JoinedAlbumArtists))
                    this.AlbumArtist = tag.JoinedAlbumArtists;

                if (!String.IsNullOrEmpty(tag.JoinedGenres))
                    this.Genre = tag.JoinedGenres.ToTitleCase();

                this.DiscNumber = tag.Disc;
                this.Duration = tagFile.Properties.Duration;
                this.TrackNumber = tag.Track;
                this.Year = tag.Year;
            }

            // Retrieve title and artist from filename if not set
            bool hasTitle = !String.IsNullOrEmpty(this.Title),
                 hasArtist = !String.IsNullOrEmpty(this.Artist);
            if (!hasTitle || !hasArtist) {
                var name = Path.GetFileNameWithoutExtension(file.Name);
                var hyphenIndex = name.IndexOf('-');

                if (!hasTitle)
                    this.Title = (hyphenIndex == -1 ? name : name.Substring(hyphenIndex + 1)).Trim();
                if (!hasArtist)
                    this.Artist = hyphenIndex == -1 ? null : name.Substring(0, hyphenIndex).Trim();
            }

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
        public string Title { get; set; }
        /// <summary>
        /// Gets or sets the artist of this track.
        /// If ArtistSort is null when this value is set, the ArtistSort value is automatically set
        /// to a sort variant of the given value (see String#ToSortName).
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Artist {
            get { return _artist ?? _albumArtist; }
            set { _artist = value;  }
        }
        /// <summary>
        /// Gets or sets the album of this track.
        /// If AlbumSort is null when this value is set, the AlbumSort value is automatically set
        /// to a sort variant of the given value (see String#ToSortName).
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Album { get; set; }
        /// <summary>
        /// Gets or sets the album artist of this track.
        /// If AlbumArtistSort is null when this value is set, the AlbumArtistSort value is automatically set
        /// to a sort variant of the given value (see String#ToSortName).
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string AlbumArtist {
            get { return _albumArtist ?? _artist; }
            set { _albumArtist = value; }
        }


        /// <summary>
        /// The sorted title of this track
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string TitleSort {
            get { return Util.ToSortName(_titleSort ?? Title); }
            set { _titleSort = value; }
        }
        /// <summary>
        /// The sort value of the performing artist of this track
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ArtistSort {
            get { return Util.ToSortName(_artistSort ?? Artist); }
            set { _artistSort = value; }
        }
        /// <summary>
        /// The sorted album this track appears on
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string AlbumSort {
            get { return Util.ToSortName(_albumSort ?? Album); }
            set { _albumSort = value; }
        }
        /// <summary>
        /// The sort value of the album artist of this track
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string AlbumArtistSort {
            get { return Util.ToSortName(_albumArtistSort ?? AlbumArtist ?? Artist); }
            set { _albumArtistSort = value; }
        }


        /// <summary>
        /// The genre of this track
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Genre { get; protected set; }
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
        /// Gets the artwork for this track
        /// </summary>
        public Image Artwork {
            get {

                // Get the tag for the file
                TagLib.Tag tag = null;
                TagLib.File tagFile = null;
                try {
                    tagFile = TagLib.File.Create(this.Uri.LocalPath);
                    tag = tagFile.Tag;
                }
                catch (Exception e) {
                    Log("Could not parse tags for file " + this.Uri.LocalPath);
                }

                // If we have no pictures, bail out
                if (tagFile == null || tag == null || tag.Pictures.Length == 0) return null;

                // Find the frontcover
                var picture = tag.Pictures.FirstOrDefault(p => p.Type == TagLib.PictureType.FrontCover);
                if (picture == null) picture = tag.Pictures.First();

                // Get the Image
                Image artwork = null;
                try {
                    artwork = new Bitmap(
                        new MemoryStream(picture.Data.ToArray())
                    );
                }
                catch (Exception) { }

                return artwork;
            }
        }
        

        /// <summary>
        /// The application-wide, unique key string for this item.
        /// Is equal to Uri.ToString().
        /// </summary>
        public string UniqueKey { get { return this.Uri.ToString(); } }

        #endregion



        public static IEnumerable<Track> GetAlbum(Track track) {
            return Track.Where(t => t.Album == track.Album && t.AlbumArtist == track.AlbumArtist).OrderBy(t => t.DiscNumber).ThenBy(t => t.TrackNumber).ThenBy(t => t.TitleSort);
        }


    }




}
