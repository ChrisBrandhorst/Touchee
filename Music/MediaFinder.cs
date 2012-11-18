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
    public class MediaFinder {


        #region Privates

        // The directory watcher used internally
        DirectoryWatcher _directoryWatcher;

        #endregion



        #region Properties


        /// <summary>
        /// The medium for this finder
        /// </summary>
        public Medium Medium { get; protected set; }


        /// <summary>
        /// The directory which is watched
        /// </summary>
        public DirectoryInfo Directory { get; protected set; }


        /// <summary>
        /// The collection state of the watcher
        /// </summary>
        public CollectionState CollectionState { get { return _directoryWatcher.CollectionState; } }


        #endregion



        #region Constructor


        /// <summary>
        /// Constructs a new MediaFinder
        /// </summary>
        /// <param name="medium">The medium in which to place new media</param>
        /// <param name="directory">The directory to watch</param>
        /// <param name="collectionRequired">Whether an initial collection is required</param>
        public MediaFinder(Medium medium, DirectoryInfo directory, bool collectionRequired) {

            // Set properties
            this.Medium = medium;
            this.Directory = directory;
            
            // Create watcher
            _directoryWatcher = new DirectoryWatcher(directory, Plugin.Extensions);

            // Set events
            _directoryWatcher.CollectingCompleted += directoryWatcher_CollectingCompleted;
            _directoryWatcher.FileCollected += FileCollected;
            _directoryWatcher.FileCreated += FileCreated;
            _directoryWatcher.FileChanged += FileChanged;
            _directoryWatcher.FileRenamed += FileRenamed;
            _directoryWatcher.FileDeleted += FileDeleted;

            // Set collecting requirement
            if (collectionRequired)
                _directoryWatcher.MarkAsCollectionRequired();
        }

        #endregion



        #region Actions


        /// <summary>
        /// Starts the directory watcher collection
        /// </summary>
        public void Collect() {
            _directoryWatcher.Collect();
        }
        

        /// <summary>
        /// Starts the directory watching
        /// </summary>
        public void Start() {
            _directoryWatcher.Start();
        }


        /// <summary>
        /// Stops the directory watching
        /// </summary>
        public void Stop() {
            _directoryWatcher.Stop();
        }


        #endregion



        #region Watcher callbacks


        /// <summary>
        /// Called when the collecting has completed
        /// </summary>
        void directoryWatcher_CollectingCompleted(DirectoryWatcher watcher, int count) {
            if (this.CollectingCompleted != null)
                this.CollectingCompleted.Invoke(this, count);
        }



        /// <summary>
        /// Called when one of the watchers collects a file
        /// </summary>
        void FileCollected(DirectoryWatcher watcher, FileInfo file, int count) {
            this.FileCreated(watcher, file);
        }


        /// <summary>
        /// Called when one of the watchers detected the creation of a file
        /// </summary>
        void FileCreated(DirectoryWatcher watcher, FileInfo file) {

            // A track was created
            if (IsTrack(file)) {
                var track = new Track(this.Medium, file);
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
        void FileChanged(DirectoryWatcher watcher, FileInfo file) {

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
        void FileRenamed(DirectoryWatcher watcher, FileInfo file, RenamedEventArgs e) {

            // A track was renamed
            if (IsTrack(file)) {
                var track = Track.GetByPath(e.OldFullPath);
                if (track == null)
                    this.FileCreated(watcher, file);
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
        void FileDeleted(DirectoryWatcher watcher, FileInfo file) {

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



        #region Events

        public event CollectingCompleted CollectingCompleted;

        #endregion


    }



    public delegate void CollectingCompleted(MediaFinder watcher, int count);


}
