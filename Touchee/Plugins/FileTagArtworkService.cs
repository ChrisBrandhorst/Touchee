using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TagLib;
using System.Drawing;
using System.IO;

using Touchee.Components;
using Touchee.Components.Services;
using Touchee.Media.Music;

namespace Touchee.Plugins {


    /// <summary>
    /// Gets album artwork from MP3, OGG, FLAC, MPC, Speex, WavPack TrueAudio, WAV, AIFF, MP4 and ASF files.
    /// </summary>
    public class FileTagArtworkService : Base, IPlugin, IAlbumArtworkService {


        #region IPlugin implementation

        /// <summary>
        /// The name of this plugin
        /// </summary>
        public string Name { get { return "FileTag Artwork"; } }


        /// <summary>
        /// The description of this plugin
        /// </summary>
        public string Description { get { return "Retrieves album artwork embedded in files."; } }


        /// <summary>
        /// The version of this plugin
        /// </summary>
        public Version Version { get { return new Version(1, 0, 0, 0); } }


        /// <summary>
        /// Whether this plugin provides some front-end functionality
        /// </summary>
        public bool ProvidesFrontend { get { return false; } }


        /// <summary>
        /// Starts this plugin
        /// </summary>
        /// <param name="config">The configuration section for this plugin</param>
        /// <param name="context">The context for this plugin</param>
        /// <returns>True</returns>
        public bool StartPlugin(dynamic config, IPluginContext context) {
            PluginManager.Register((IAlbumArtworkService)this);
            return true;
        }


        /// <summary>
        /// Stops this plugin
        /// </summary>
        /// <returns>True</returns>
        public bool StopPlugin() {
            PluginManager.Unregister((IAlbumArtworkService)this);
            return true;
        }

        #endregion


        #region IAlbumArtworkService implementation


        /// <summary>
        /// Gets an image for the album of the given track
        /// </summary>
        /// <param name="track">A track of the requested album</param>
        /// <param name="artwork">The reference object to store the resulting artwork in</param>
        /// <returns>The result status code for this service call</returns>
        public ServiceResultStatus GetAlbumArtwork(ITrack track, out Image artwork) {
            artwork = null;

            // We only do files
            if (!(track is ITrack)) return ServiceResultStatus.NoResult;

            // Get uri of file
            var uri = track.Uri;

            // We only process local files
            if (!uri.IsFile) return ServiceResultStatus.NoResult;

            // Get the tags
            TagLib.File file = null;
            try {
                file = TagLib.File.Create(
                    uri.LocalPath.Replace("\\\\localhost\\", "")
                );
            }
            catch (Exception) { }

            // If we have no pictures, bail out
            if (file == null || file.Tag.Pictures.Length == 0) return ServiceResultStatus.NoResult;

            // Find the frontcover
            var picture = file.Tag.Pictures.FirstOrDefault(p => p.Type == PictureType.FrontCover);
            if (picture == null) picture = file.Tag.Pictures.First();

            var bytes = picture.Data.ToArray();
            try {
                var stream = new MemoryStream(bytes);
                artwork = new Bitmap(stream);
            }
            catch (Exception) { }

            return artwork == null ? ServiceResultStatus.NoResult : ServiceResultStatus.Success;
        }


        /// <summary>
        /// Gets an image for the given album
        /// </summary>
        /// <param name="artist">The artist of the album</param>
        /// <param name="title">The title of the album</param>
        /// <param name="artwork">The reference object to store the resulting artwork in</param>
        /// <returns>The result status code for this service call</returns>
        public ServiceResultStatus GetAlbumArtwork(string artist, string title, out Image artwork) {
            artwork = null;
            return ServiceResultStatus.NoResult;
        }


        #endregion



    }

}