using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Touchee;
using Touchee.Components.FileSystem;
using Touchee.Components.Media;

using Music.Media;

namespace Music {


    /// <summary>
    /// 
    /// </summary>
    public class MusicFileMediumWatcher : FileMediumWatcher<MusicDirectoryWatcher> {


        #region Privates

        // Debounce function for cache saving
        //Debouncer _saveCache;

        #endregion



        #region Constructor

        /// <summary>
        /// Constructs a new music file medium watcher
        /// </summary>
        public MusicFileMediumWatcher() {

            //// Set debouncer
            //_saveCache = new Debouncer(() => Cache.Serialize(CachePath), new TimeSpan(0, 0, 10));

            //// Set callbacks from media types which should be saved
            //Track.AfterSave += Track_Cache;
            //Track.AfterDispose += Track_Cache;
            //Playlist.AfterSave += Playlist_Cache;
            //Playlist.AfterDispose += Playlist_Cache;
        }

        #endregion



        #region Caching


        ///// <summary>
        ///// Gets called when a playlist is changed
        ///// </summary>
        //void Playlist_Cache(object sender, Collectable<Container>.ItemEventArgs e) {
        //    _saveCache.Call();
        //}


        ///// <summary>
        ///// Gets called when a track is changed
        ///// </summary>
        //void Track_Cache(object sender, Collectable<Track>.ItemEventArgs e) {
        //    _saveCache.Call();
        //}


        ///// <summary>
        ///// The location of the XML cache for the plugin
        ///// </summary>
        //string CachePath {
        //    get {
        //        return Path.Combine(new FileInfo(this.GetType().Assembly.Location).DirectoryName, "local.xml");
        //    }
        //}


        #endregion



        #region Medium watcher stuff


        /// <summary>
        /// Called when a medium is detected.
        /// </summary>
        /// <param name="medium">The medium that has been detected</param>
        protected override void OnWatch(Medium medium) {

            // Create a master playlist for the medium
            var masterPlaylist = new MasterPlaylist(medium);
            masterPlaylist.Save();
            medium.Containers.Add(masterPlaylist);


            var pl1 = new Playlist("Playlist Test 1", medium, masterPlaylist);
            pl1.Save();
            var pl2 = new Playlist("Playlist Test 2", medium, masterPlaylist);
            pl2.Save();

            //// Deserialize the cache of the local medium
            //if (medium == Medium.Local && File.Exists(CachePath))
            //    Cache.Deserialize(CachePath);
        }


        /// <summary>
        /// When a medium is unwatched, we dispose of all the playlists and tracks from that medium
        /// </summary>
        /// <param name="medium">The medium that is being unwatched</param>
        protected override void OnUnWatch(Medium medium) {
            // TODO
            // Needs to remove items from the lists

            // Dispose all playlists and tracks for this medium
            foreach (var playlist in medium.Containers.Cast<Playlist>()) {
                foreach (var track in playlist.Tracks.Cast<Track>()) {
                    track.Dispose();
                }
                playlist.Dispose();
            }
            medium.Containers.Clear();
            
        }


        #endregion



        #region Directory Watcher stuff


        /// <summary>
        /// Create a directory watcher for the given medium and directory.
        /// </summary>
        protected override MusicDirectoryWatcher CreateDirectoryWatcher(Medium medium, DirectoryInfo directoryInfo) {
            var musicDirectoryWatcher = new MusicDirectoryWatcher(medium, directoryInfo, Plugin.Extensions);

            // If the directory is not collected yet, mark it as to collect
            if (!CollectedLocalDirectories.Any(d => d.FullName == directoryInfo.FullName)) {
                musicDirectoryWatcher.MarkAsCollectionRequired();
                musicDirectoryWatcher.CollectingCompleted += musicDirectoryWatcher_CollectingCompleted;
            }

            return musicDirectoryWatcher;
        }


        /// <summary>
        /// Called when a DirectoryWatcher has completed collecting
        /// </summary>
        /// <param name="watcher">The watcher that has completed collecting</param>
        /// <param name="count">The number of items found</param>
        void musicDirectoryWatcher_CollectingCompleted(DirectoryWatcher watcher, int count) {
            watcher.CollectingCompleted -= musicDirectoryWatcher_CollectingCompleted;
            var done = DirectoryWatchers.Where(dw => dw.Medium == watcher.Medium).All(dw => dw.CollectionState == CollectionState.Collected);
            if (done) {
                var masterPlaylist = watcher.Medium.MasterContainer;
                masterPlaylist.Loaded = true;
                masterPlaylist.Save();
            }
        }


        /// <summary>
        /// Returns the set of local directories that have been collected
        /// </summary>
        public IEnumerable<DirectoryInfo> CollectedLocalDirectories {
            get {
                return DirectoryWatchers.Where(dw => dw.Medium == _localMedium && dw.CollectionState == CollectionState.Collected).Select(dw => dw.Directory);
            }
        }

        #endregion


    }

}
