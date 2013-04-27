using System;
using System.IO;
using System.Drawing;
using System.Linq;

using Touchee;
using Touchee.Media;
using Touchee.Media.Music;

namespace Music.Media {

    /// <summary>
    /// Track from a file
    /// </summary>
    public class FileTrack : Track, IFileTrack {


        #region Statics


        /// <summary>
        /// Gets the first track which corresponds with the given file path
        /// </summary>
        /// <param name="path">The path to search for</param>
        /// <returns>The track with the given path, or null if none found</returns>
        public static FileTrack GetByPath(string path) {
            return (FileTrack)Track.GetByUri(new Uri(path));
        }


        #endregion

        


        #region Constructor

        /// <summary>
        /// Constructs a new Track object
        /// </summary>
        /// <param name="file">The FileInfo object of the music file</param>
        public FileTrack(FileInfo file) {
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

            // Set properties if we have a tag
            if (tagFile != null) {

                if (!String.IsNullOrEmpty(tag.TitleSort))
                    this.TitleSort = tag.TitleSort;
                if (!String.IsNullOrEmpty(tag.Title))
                    this.Title = tag.Title;

                if (!String.IsNullOrEmpty(tag.JoinedPerformersSort))
                    this.ArtistSort = tag.JoinedPerformersSort;
                if (!String.IsNullOrEmpty(tag.JoinedPerformers))
                    this.Artist = tag.JoinedPerformers;

                if (!String.IsNullOrEmpty(tag.AlbumSort))
                    this.AlbumSort = tag.AlbumSort;
                if (!String.IsNullOrEmpty(tag.Album))
                    this.Album = tag.Album;

                if (tag.AlbumArtistsSort.Length > 0)
                    this.AlbumArtistSort = String.Join("; ", tag.AlbumArtistsSort);
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




        #region Properties

        /// <summary>
        /// The artwork for this track
        /// </summary>
        public override Image Artwork {
            get {

                // Get the tag for the file
                TagLib.Tag tag = null;
                TagLib.File tagFile = null;
                try {
                    tagFile = TagLib.File.Create(this.Uri.LocalPath);
                    tag = tagFile.Tag;
                }
                catch (Exception) {
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

        #endregion


    }


}
