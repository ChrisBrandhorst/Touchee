using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
//using System.Text;
//using System.Threading.Tasks;

using Touchee;
using Touchee.Components.FileSystem;
using Touchee.Components.Media;

using Music.Media;

using TagLib;

namespace Music {


    /// <summary>
    /// Bla
    /// </summary>
    public class Watcher : Base, IMediumWatcher {



        #region Singleton


        /// <summary>
        /// Private constructor
        /// </summary>
        Watcher() { }


        /// <summary>
        /// The singleton instance of the watcher
        /// </summary>
        public static Watcher Instance = new Watcher();


        #endregion



        #region Privates


        // Media finders internal list
        List<MediaFinder> _mediaFinders = new List<MediaFinder>();

        // Temporary list keeping local directories while the local medium has not yet been watched
        List<DirectoryInfo> _localFoldersTemp = new List<DirectoryInfo>();

        // The local medium
        Medium _localMedium;

        // Cache debounce
        Debouncer _saveCache;


        #endregion



        #region Init


        /// <summary>
        /// Initializes the watcher
        /// </summary>
        public void Init() {
            this.InitAutoCache();
        }


        #endregion



        #region IMediumWatcher implementation


        /// <summary>
        /// Start watching the given medium. Only if a medium with type Local or FileStorage
        /// is given, is it going to be watched.
        /// </summary>
        /// <param name="medium">The Medium to watch</param>
        public bool Watch(Medium medium) {

            // Check if we are already watching this medium
            if (_mediaFinders.Any(dw => dw.Medium == medium)) return false;

            switch (medium.Type) {

                // We got the local medium
                case MediumType.Local:
                    _localMedium = medium;
                    this.LocalMediumArrived();
                    break;

                // We have a disc / usb storage
                case MediumType.FileStorage:

                    // Check if we have a DriveMedium (should always be the case)
                    if (!(medium is DriveMedium)) return false;
                    var driveMedium = (DriveMedium)medium;

                    // Create finder for drive
                    this.CreateMediaFinder(driveMedium, driveMedium.DriveInfo.RootDirectory, true);
                    break;

                // We don't process other media
                default:
                    return false;
            }

            return true;
        }


        /// <summary>
        /// Stop watching the given medium
        /// </summary>
        /// <param name="medium">The medium to stop watching</param>
        public bool UnWatch(Medium medium) {

            // Get all media finders for the given medium
            var mediaFinders = _mediaFinders.Where(mf => mf.Medium == medium).ToList();

            // Nothing found? Bail out
            if (mediaFinders.Count() == 0) return false;

            // Stop and remove all found finders
            foreach (var mf in mediaFinders) {
                _mediaFinders.Remove(mf);
                mf.Stop();
            }

            // Clear data from medium
            foreach (var c in medium.Containers.Cast<Playlist>()) {
                foreach (var t in c.Tracks.Cast<Track>())
                    t.Dispose();
                c.Dispose();
            }

            // Clear local medium
            if (_localMedium == medium) {
                _localMedium = null;
                _localFoldersTemp.Clear();
            }
            
            return true;
        }


        /// <summary>
        /// Stops watching all media
        /// </summary>
        public bool UnWatchAll() {
            var ret = false;
            foreach (var m in _mediaFinders.Select(mf => mf.Medium))
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
                this.CreateMediaFinder(_localMedium, directoryInfo, true);
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

            // We need to collect if we have no cache
            bool collectionRequired = !System.IO.File.Exists(CachePath);

            // Load cache file if we have it
            if (!collectionRequired)
                Cache.Deserialize(CachePath);
    
            // Start all media finders
            lock (_localFoldersTemp) {
                foreach (var directoryInfo in _localFoldersTemp) {
                    this.CreateMediaFinder(_localMedium, directoryInfo, collectionRequired);
                }
            }
            _localFoldersTemp.Clear();
        }


        #endregion



        #region Directory Watcher handling


        /// <summary>
        /// Creates and starts a media finder. If colletion is required, sets the finder up for collecting.
        /// </summary>
        /// <param name="medium">The medium in which to place the folder</param>
        /// <param name="directoryInfo">The folder to watch</param>
        /// <param name="needsCollecting">Whether the finder needs collecting</param>
        void CreateMediaFinder(Medium medium, DirectoryInfo directoryInfo, bool collectionRequired) {

            // Create finder
            var mediaFinder = new MediaFinder(medium, directoryInfo, collectionRequired);
            _mediaFinders.Add(mediaFinder);

            // Collect if required
            if (collectionRequired)
                CollectSequentially(mediaFinder);

            // Start watching
            mediaFinder.Start();
        }


        /// <summary>
        /// Starts collection of the given media finder and continues collecting all media finders
        /// which require collecting.
        /// </summary>
        /// <param name="mediaFinder">The media finder to start collecting with</param>
        void CollectSequentially(MediaFinder mediaFinder) {

            // Bail out if we're already collecting. The media finder will be picked up later.
            if (this.IsCollecting()) return;

            // Set callback and start collecting
            mediaFinder.CollectingCompleted += mediaFinder_CollectingCompleted;
            mediaFinder.Collect();
        }
        void mediaFinder_CollectingCompleted(MediaFinder mediaFinder, int count) {

            // Remove callback
            mediaFinder.CollectingCompleted -= mediaFinder_CollectingCompleted;

            // Get next media finder to collect
            var toCollect = _mediaFinders.FirstOrDefault(mf => mf.CollectionState == CollectionState.CollectionRequired);
            if (toCollect != null)
                CollectSequentially(toCollect);
        }


        /// <summary>
        /// Returns whether any of the present media finders is collecting.
        /// </summary>
        /// <returns></returns>
        bool IsCollecting() {
            return _mediaFinders.Any(mf => mf.CollectionState == CollectionState.Collecting);
        }



        #endregion



        #region Caching


        /// <summary>
        /// Initializes the auto caching mechanism.
        /// </summary>
        void InitAutoCache() {

            // Set debouncer
            _saveCache = new Debouncer(() => Cache.Serialize(CachePath), new TimeSpan(0, 0, 10));

            // Set callbacks from media types which should be saved
            Track.AfterSave += Track_Cache;
            Track.AfterDispose += Track_Cache;
            Playlist.AfterSave += Playlist_Cache;
            Playlist.AfterDispose += Playlist_Cache;
        }

        void Playlist_Cache(object sender, Collectable<Container>.ItemEventArgs e) { _saveCache.Call(); }
        void Track_Cache(object sender, Collectable<Track>.ItemEventArgs e) { _saveCache.Call(); }


        /// <summary>
        /// The locatio of the XML cache for the plugin
        /// </summary>
        string CachePath {
            get {
                return Path.Combine(new FileInfo(this.GetType().Assembly.Location).DirectoryName, "local.xml");
            }
        }


        #endregion



        
    }

}
