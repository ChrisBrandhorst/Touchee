using System;
using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;

using Touchee;
using Touchee.Components.FileSystem;

using Music.Media;

namespace Music {


    /// <summary>
    /// Finds music media in a folder and places it in the appropriate medium
    /// </summary>
    public class MusicDirectoryWatcher : DirectoryWatcher {


        #region Constructor


        /// <summary>
        /// Constructs a new MediaFinder
        /// </summary>
        /// <param name="medium">The medium in which to place new media</param>
        /// <param name="directory">The directory to watch</param>
        /// <param name="collectionRequired">Whether an initial collection is required</param>
        public MusicDirectoryWatcher(Medium medium, DirectoryInfo directory, IEnumerable<string> extensions) : base(medium, directory, extensions) {
        }

        #endregion



        #region Watcher callbacks


        /// <summary>
        /// Called when one of the watchers collects a file
        /// </summary>
        protected override void OnFileCollected(FileInfo file, int count) {
            this.OnFileCreated(file);
        }


        /// <summary>
        /// Called when one of the watchers detected the creation of a file
        /// </summary>
        protected override void OnFileCreated(FileInfo file) {

            // A track was created
            if (IsTrack(file)) {
                var track = new Track(file);
                track.Save();
            }

            // A playlist was created
            else if (IsPlaylist(file)) {
                throw new NotImplementedException();
            }

        }


        /// <summary>
        /// Called when one of the watchers detects a change in a file
        /// </summary>
        protected override void OnFileChanged(FileInfo file) {

            // A track was changed
            if (IsTrack(file)) {
                var track = Track.GetByPath(file.FullName);
                if (track != null) {
                    track.Update(file);
                }
            }

            // A playlist was changed
            else if (IsPlaylist(file)) {
                throw new NotImplementedException();
            }

        }


        /// <summary>
        /// Called when one of the watchers detects the renaming of a file
        /// </summary>
        protected override void OnFileRenamed(FileInfo file, RenamedEventArgs e) {

            // A track was renamed
            if (IsTrack(file)) {
                var track = Track.GetByPath(e.OldFullPath);
                if (track == null)
                    this.OnFileCreated(file);
                else {
                    track.Update(file);
                }
            }

            // A playlist was renamed
            else if (IsPlaylist(file)) {
                throw new NotImplementedException();
            }


        }


        /// <summary>
        /// Called when one of the watchers detects the deletion of a file
        /// </summary>
        protected override void OnFileDeleted(FileInfo file) {

            // A track was deleted
            if (IsTrack(file)) {
                var track = Track.GetByPath(file.FullName);
                if (track != null) {
                    track.Dispose();
                }
            }

            // A playlist was deleted
            else if (IsPlaylist(file)) {
                throw new NotImplementedException();
            }

        }



        #endregion



        #region Helpers

        /// <summary>
        /// Checks whether the file represented by the given FileInfo is a track by
        /// comparing it to the known track extensions.
        /// </summary>
        /// <param name="file">The file to check</param>
        /// <returns>True if it is a track, otherwise false</returns>
        bool IsTrack(FileInfo file) {
            var extension = file.Extension.Substring(1).ToLower();
            return Plugin.TrackExtensions.Any(e => e.ToLower() == extension);
        }


        /// <summary>
        /// Checks whether the file represented by the given FileInfo is a playlist by
        /// comparing it to the known playlist extensions.
        /// </summary>
        /// <param name="file">The file to check</param>
        /// <returns>True if it is a playlist, otherwise false</returns>
        bool IsPlaylist(FileInfo file) {
            var extension = file.Extension.Substring(1).ToLower();
            return Plugin.PlaylistExtensions.Any(e => e.ToLower() == extension);
        }


        #endregion


    }


}
