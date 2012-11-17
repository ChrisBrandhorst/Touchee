using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
//using System.Text;
//using System.Threading.Tasks;

using Touchee;
using Touchee.Components.FileSystem;
using Touchee.Components.Media;

using TagLib;

namespace Music {

    public class Watcher : Base, IMediumWatcher {


        #region Privates


        // The local medium
        Medium _localMedium;

        // Storage for directory watchers
        List<DirectoryWatcher> _directoryWatchers = new List<DirectoryWatcher>();

        // The extensions to watch
        IEnumerable<string> _extensions;


        #endregion



        #region Constructor

        /// <summary>
        /// 
        /// </summary>
        public Watcher(IEnumerable<string> extensions) {
            _extensions = extensions;
        }

        #endregion



        #region IMediumWatcher implementation


        /// <summary>
        /// Start watching the given medium. Only if a medium with type Local is given, is it goint to be watched.
        /// </summary>
        /// <param name="medium">The Medium to watch</param>
        public bool Watch(Medium medium) {

            // Do nothing if we already have a local medium
            if (medium.Type != MediumType.Local || _localMedium != null) return false;
            _localMedium = medium;

            // Start the watchers 
            this.StartDirectoryWatcher();

            return true;
        }


        /// <summary>
        /// Stop watching the given medium
        /// </summary>
        /// <param name="medium">The medium to stop watching</param>
        public bool UnWatch(Medium medium) {
            if (_localMedium == medium) {
                _localMedium = null;
                
                // TODO: stop directorywatchers, clear contents
                foreach (var dw in _directoryWatchers)
                    dw.Stop();

                return true;
            }
            return false;
        }


        /// <summary>
        /// Stops watching all media
        /// </summary>
        public bool UnWatchAll() {
            this.UnWatch(_localMedium);
            return true;
        }


        #endregion



        #region DirectoryWatcher management


        /// <summary>
        /// The locatio of the XML cache for the plugin
        /// </summary>
        string CachePath {
            get {
                return Path.Combine(new FileInfo(this.GetType().Assembly.Location).DirectoryName, "cache.xml");
            }
        }


        /// <summary>
        /// Intializes a directory watcher for the given folder
        /// </summary>
        /// <param name="folder">The folder to watch</param>
        public void AddFolder(string folder) {

            // Create dir info
            var directoryInfo = new DirectoryInfo(folder);

            // Check if already watching
            var exists = _directoryWatchers.Any(dw => String.Compare(dw.Directory.FullName, directoryInfo.FullName, true) == 0);
            if (exists) return;

            // Create watcher, set extensions
            var directoryWatcher = new DirectoryWatcher(directoryInfo, _extensions);
            _directoryWatchers.Add(directoryWatcher);

            // Set events
            directoryWatcher.FileCollected += FileCollected;
            directoryWatcher.FileCreated += FileCreated;
            directoryWatcher.FileChanged += FileChanged;
            directoryWatcher.FileRenamed += FileRenamed;
            directoryWatcher.FileDeleted += FileDeleted;

            // If we already received a local medium, collect and start watching
            if (_localMedium != null) {
                directoryWatcher.Collect();
                directoryWatcher.Start();
            }
        }


        /// <summary>
        /// Initializes directory watchers for the given folders
        /// </summary>
        /// <param name="folders">The folders to watch</param>
        public void AddFolders(IEnumerable<string> folders) {
            foreach (var f in folders)
                this.AddFolder(f);
        }


        /// <summary>
        /// Starts the watching of any of the uncollected directory watchers.
        /// When it completes, continues with the next until all directory watchers are collected and watching.
        /// </summary>
        void StartDirectoryWatcher() {

            // If we have a cache file
            if (System.IO.File.Exists(CachePath)) {

                // Load the cache
                Cache.Deserialize(CachePath);
                Log(String.Format("Loaded {0} tracks from cache", Media.Track.All().Count()));

                // Start all watchers
                foreach (var dw in _directoryWatchers)
                    dw.Start();
            }

            // No cache, so collect files
            else {

                // Get uncollected watcher
                var directoryWatcher = _directoryWatchers.FirstOrDefault(dw => !dw.Collected);

                // No more watchers to go, so cache result
                if (directoryWatcher == null) {
                    Cache.Serialize(CachePath);
                    Log(String.Format("Found {0} tracks", Media.Track.All().Count()));
                }

                // Else collect and start
                else {
                    directoryWatcher.FileCollectingCompleted += directoryWatcher_FileCollectingCompleted;
                    directoryWatcher.Collect();
                    directoryWatcher.Start();
                }
            }

        }
        void directoryWatcher_FileCollectingCompleted(DirectoryWatcher watcher, int count) {
            watcher.FileCollectingCompleted -= directoryWatcher_FileCollectingCompleted;
            this.StartDirectoryWatcher();
        }


        #endregion



        #region Watcher callbacks


        void FileCollected(DirectoryWatcher watcher, FileInfo file, int count) {
            var track = new Media.Track(file);
            track.Save();
        }

        void FileCreated(DirectoryWatcher watcher, FileInfo file) {
            var track = new Media.Track(file);
            track.Save();
        }

        void FileChanged(DirectoryWatcher watcher, FileInfo file) {
            var track = Media.Track.GetByPath(file.FullName);
            if (track != null)
                track.Update(file);
        }

        void FileRenamed(DirectoryWatcher watcher, FileInfo file, RenamedEventArgs e) {
            var track = Media.Track.GetByPath(e.OldFullPath);
            if (track == null)
                FileCreated(watcher, file);
            else
                track.Update(file);
        }

        void FileDeleted(DirectoryWatcher watcher, FileInfo file) {
            var track = Media.Track.GetByPath(file.FullName);
            if (track != null)
                track.Dispose();
        }



        #endregion


    }

}
