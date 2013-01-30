using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Touchee;
using Touchee.Components.Media;

namespace Touchee.Components.FileSystem {


    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TDirectoryWatcher"></typeparam>
    public abstract class FileMediumWatcher<TDirectoryWatcher> : Base, IMediumWatcher where TDirectoryWatcher : DirectoryWatcher {


        #region Privates


        // Directory watchers internal list
        protected List<TDirectoryWatcher> _directoryWatchers = new List<TDirectoryWatcher>();

        // Temporary list keeping local directories while the local medium has not yet been watched
        List<DirectoryInfo> _localFoldersTemp = new List<DirectoryInfo>();

        // The local medium
        protected Medium _localMedium;


        #endregion



        #region IMediumWatcher implementation


        /// <summary>
        /// Start watching the given medium. Only if a medium with type Local or FileStorage
        /// is given, is it going to be watched.
        /// </summary>
        /// <param name="medium">The Medium to watch</param>
        public bool Watch(Medium medium) {

            // Check if we are already watching this medium
            if (_directoryWatchers.Any(dw => dw.Medium == medium)) return false;

            // We got the local medium
            if (medium == Medium.Local) {
                _localMedium = medium;
                this.OnWatch(medium);
                this.LocalMediumArrived();
            }

            // We got a basic file storage medium
            else if (medium is FileStorageMedium) {
                var fileStorageMedium = (FileStorageMedium)medium;
                this.OnWatch(medium);
                this.CreateDirectoryWatcher(fileStorageMedium, fileStorageMedium.DriveInfo.RootDirectory, true);
            }

            // We don't process other media
            else {
                return false;
            }

            return true;
        }
        protected virtual void OnWatch(Medium medium) { }

        /// <summary>
        /// Stop watching the given medium
        /// </summary>
        /// <param name="medium">The medium to stop watching</param>
        public bool UnWatch(Medium medium) {

            // Get all directory watchers for the given medium
            var directoryWatchers = _directoryWatchers.Where(dw => dw.Medium == medium).ToList();

            // Nothing found? Bail out
            if (directoryWatchers.Count() == 0) return false;

            // Stop and remove all found watchers
            lock (_directoryWatchers) {
                foreach (var dw in directoryWatchers) {
                    _directoryWatchers.Remove(dw);
                    dw.Stop();
                }
            }

            // Clear data from medium
            this.OnUnWatch(medium);

            // Clear local medium
            if (_localMedium == medium) {
                _localMedium = null;
                _localFoldersTemp.Clear();
            }

            return true;
        }
        protected virtual void OnUnWatch(Medium medium) { }


        /// <summary>
        /// Stops watching all media
        /// </summary>
        public bool UnWatchAll() {
            var ret = false;
            foreach (var m in _directoryWatchers.Select(mf => mf.Medium))
                ret |= this.UnWatch(m);
            return ret;
        }


        #endregion



        #region Handling local medium


        /// <summary>
        /// Adds a folder for watching within the local medium
        /// </summary>
        /// <param name="path">The path of the folder to watch</param>
        public void AddLocalFolder(string path) {
            var directoryInfo = new DirectoryInfo(path);

            // If we have a local medium, set the finder for the path
            // This will always be called after the cache has loaded, so we require collecting here
            if (_localMedium != null) {
                this.CreateDirectoryWatcher(_localMedium, directoryInfo, true);
            }

            // Else, store the folder for later use
            else {
                _localFoldersTemp.Add(directoryInfo);
            }

        }


        /// <summary>
        /// Is called when the local medium has arrived.
        /// Checks if a cache is available: if so, loads the cache.
        /// If not, collects the files from the available folders.
        /// Finally, starts all media finders for all the folders.
        /// </summary>
        void LocalMediumArrived() {
            lock (_localFoldersTemp) {
                foreach (var directoryInfo in _localFoldersTemp) {
                    this.CreateDirectoryWatcher(_localMedium, directoryInfo, false);
                }
            }
            _localFoldersTemp.Clear();
        }

        #endregion



        #region Directory Watcher handling


        /// <summary>
        /// Creates and starts a directory watcher. If collection is forced, sets the watcher up for collecting.
        /// </summary>
        /// <param name="medium">The medium in which to place the folder</param>
        /// <param name="directoryInfo">The folder to watch</param>
        /// <param name="forceCollecting">Whether to force collecting of the watcher</param>
        void CreateDirectoryWatcher(Medium medium, DirectoryInfo directoryInfo, bool forceCollecting) {

            // TODO: Check if dir is not already watched
            
            // Create directory watcher
            var directoryWatcher = GetDirectoryWatcher(medium, directoryInfo);
            _directoryWatchers.Add(directoryWatcher);
            
            // Collect if required
            if (forceCollecting || directoryWatcher.CollectionState == CollectionState.CollectionRequired)
                CollectSequentially(directoryWatcher);

            // Start watching
            directoryWatcher.Start();
        }
        protected abstract TDirectoryWatcher GetDirectoryWatcher(Medium medium, DirectoryInfo directoryInfo);


        /// <summary>
        /// Starts collection of the given directory watcher and continues collecting all watchers
        /// which require collecting.
        /// </summary>
        /// <param name="directoryWatcher">The directory watcher to start collecting with</param>
        void CollectSequentially(DirectoryWatcher directoryWatcher) {

            // Bail out if we're already collecting. The media finder will be picked up later.
            if (this.IsCollecting()) return;

            // Set callback and start collecting
            directoryWatcher.CollectingCompleted += directoryWatcher_CollectingCompleted;
            directoryWatcher.Collect();
        }
        void directoryWatcher_CollectingCompleted(DirectoryWatcher directoryWatcher, int count) {

            // Remove callback
            directoryWatcher.CollectingCompleted -= directoryWatcher_CollectingCompleted;

            // Get next watcher to collect
            var toCollect = _directoryWatchers.FirstOrDefault(dw => dw.CollectionState == CollectionState.CollectionRequired);
            if (toCollect != null)
                CollectSequentially(toCollect);
        }


        /// <summary>
        /// Returns whether any of the present directory watchers is collecting.
        /// </summary>
        /// <returns></returns>
        bool IsCollecting() {
            return _directoryWatchers.Any(dw => dw.CollectionState == CollectionState.Collecting);
        }



        #endregion




    }


}
