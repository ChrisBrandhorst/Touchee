using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;
using System.IO;
using System.Threading;

using Touchee.Server;
using Touchee.Server.Responses;
using Touchee.Artwork;
using Touchee.Playback;
using Touchee.Components;
using Touchee.Components.Content;
using Touchee.Components.Media;
using Touchee.Components.Playback;
using Touchee.Media;
using Touchee.Devices;

namespace Touchee {

    /// <remarks>
    /// 
    /// </remarks>
    public class Library : Base {


        #region Singleton

        /// <summary>
        /// Private constructor
        /// </summary>
        Library() {
        }

        /// <summary>
        /// The singleton instance of the library
        /// </summary>
        public static Library Instance = new Library();

        #endregion


        
        #region Private vars

        /// <summary>
        /// The server used to communicate changes in the library
        /// </summary>
        protected ToucheeServer _server;

        /// <summary>
        /// List of current medium wathcers
        /// </summary>
        List<IMediumWatcher> _mediumWatchers;

        /// <summary>
        /// List of current media watchers
        /// </summary>
        List<IMediaWatcher> _mediaWatchers;

        /// <summary>
        /// Timespan representing the period that should be waited before retrying a non-available artwork
        /// </summary>
        TimeSpan _artworkRetryPeriod;

        /// <summary>
        /// Keep track of whether we are playing
        /// </summary>
        bool _playing;

        #endregion



        #region Properties

        /// <summary>
        /// The library content revision number
        /// </summary>
        public uint Revision { get; protected set; }

        /// <summary>
        /// The current queue
        /// </summary>
        public Queue Queue { get; private set; }

        /// <summary>
        /// The current player
        /// </summary>
        public IPlayer Player { get; private set; }


        #endregion



        #region Init

        /// <summary>
        /// Initialises the library
        /// </summary>
        /// <param name="mediaWatcherPollingInterval">The interval at which to look for modified media</param>
        public void Init(ToucheeServer server, int mediaWatcherPollingInterval) {
            _server = server;

            // Set retry period from config
            int period = Program.Config.Get("artwork.retryPeriod") ?? 2592000;
            _artworkRetryPeriod = new TimeSpan(0, 0, period);

            // Instantiate all available MediumWatchers
            // These watch the Medium instances and generate Containers
            _mediumWatchers = PluginManager.GetComponent<IMediumWatcher>().ToList();
            Medium.AfterCreate += Medium_AfterCreate;
            Medium.AfterDispose += Medium_BeforeDispose;

            // Watch for container changes
            Container.AfterSave += ContainersChanged;
            Container.AfterDispose += ContainersChanged;
            Container.ContentsChanged += ContainerContentsChanged;

            // Watch for device changes
            Device.AfterCreate += DevicesChanged;
            Device.AfterDispose += DevicesChanged;
            Device.AfterUpdate += DeviceChanged;

            // Init local and web media
            string localMediumName = Program.Config.Get("name", System.Environment.MachineName);
            LocalMedium.Init(localMediumName);
            string webCastsName = Program.Config.Get("webcastsName", "Webcasts");
            WebMedium.Init(webCastsName);
            
            // Instantiate all available MediaWatchers
            // These generate Medium instances
            PluginManager.Register(new DriveMediaWatcher());
            _mediaWatchers = PluginManager.GetComponent<IMediaWatcher>().ToList();

            // Start media detection
            _mediaWatchers.ForEach(w => w.Watch(mediaWatcherPollingInterval));
        }

        #endregion



        #region Event handlers


        /// <summary>
        /// Ups the revision because a change has occured
        /// </summary>
        void Revised() {
            this.Revision++;
        }


        /// <summary>
        /// Called when a medium has been detected. Presents this medium to all watchers.
        /// If any watcher watches the medium, the media list is broadcasted.
        /// </summary>
        void Medium_AfterCreate(object sender, Collectable<Medium>.ItemEventArgs e) {
            var watchers = _mediumWatchers.Where(w => w.CanWatch(e.Item));
            if (watchers.Count() > 0) {
                this.Revised();
                _server.Broadcast(new MediaResponse());
                foreach (var watcher in watchers)
                    watcher.Watch(e.Item);
            }
        }


        /// <summary>
        /// Called when a medium has been removed. Presents this medium to all watchers.
        /// If any watcher watches was watching the medium, the media list is broadcasted.
        /// </summary>
        void Medium_BeforeDispose(object sender, Collectable<Medium>.ItemEventArgs e) {
            _mediumWatchers.ForEach(w => w.UnWatch(e.Item));
            this.Revised();
            _server.Broadcast(new MediaResponse());
        }


        /// <summary>
        /// Called when a container has been created, updated or disposed
        /// </summary>
        void ContainersChanged(object sender, Collectable<Container>.ItemEventArgs e) {
            this.Revised();
            _server.Broadcast(new ContainersResponse(e.Item.Medium));
        }


        ///// <summary>
        ///// Called when a container has been updated
        ///// </summary>
        //void ContainerChanged(object sender, Collectable<Container>.ItemEventArgs e) {
        //    this.Revised();
        //    _server.Broadcast("container", e.Item);
        //}


        /// <summary>
        /// Called when the contents of a container have been modified.
        /// </summary>
        /// <param name="container">The container which is modified</param>
        void ContainerContentsChanged(Container container) {
            this.Revised();
            _server.Broadcast(new ContentsChangedResponse(container));
        }


        /// <summary>
        /// Called when a device has been created or disposed
        /// </summary>
        void DevicesChanged(object sender, Collectable<Device>.ItemEventArgs e) {
            _server.Broadcast(new DevicesResponse());
        }

         
        /// <summary>
        /// Called when a device has been changed
        /// </summary>
        void DeviceChanged(object sender, Collectable<Device>.ItemEventArgs e) {
            _server.Broadcast("device", e.Item);
        }

        #endregion



        #region Broadcasts


        // Active broadcast timers
        Dictionary<object, Timer> _broadcastTimers = new Dictionary<object, Timer>();


        /// <summary>
        /// Broadcasts the containers of a medium to the clients.
        /// Call is throttled.
        /// </summary>
        /// <param name="medium">The medium to send the containers of</param>
        void BroadcastContainers(Medium medium) {
            ThrottledBroadcast(medium, () => {
                this.Revised();
                _server.Broadcast(new ContainersResponse(medium));
            });
        }




        void BroadcastQueue(Queue queue) {
            //ThrottledBroadcast(queue, () => {
                _server.Broadcast(new QueueResponse(queue));
            //});
        }


        /// <summary>
        /// Throttles the given action in the scope of the given object.
        /// </summary>
        /// <param name="obj">The scope of the action</param>
        /// <param name="action">The action to throttle</param>
        void ThrottledBroadcast(object obj, Action action) {

            lock (_broadcastTimers) {

                if (_broadcastTimers.ContainsKey(obj)) {
                    var existingTimer = _broadcastTimers[obj];
                    _broadcastTimers.Remove(existingTimer);
                    existingTimer.Dispose();
                }

                var timer = new Timer((object state) => {
                    _broadcastTimers.Remove(obj);
                    action.Invoke();
                }, null, 1000, Timeout.Infinite);

                _broadcastTimers[obj] = timer;

            }

        }


        #endregion



        #region Responses

        /// <summary>
        /// Gets the server info object
        /// </summary>
        public ServerInfoResponse GetServerInfoResponse() {
            return new ServerInfoResponse(_server, this);
        }


        /// <summary>
        /// Gets the revision response
        /// </summary>
        public RevisionResponse GetRevisionResponse() {
            return new RevisionResponse(this);
        }


        /// <summary>
        /// Gets a message containing the content for the given container, type and filter combination
        /// </summary>
        public ContentsResponse GetContentsResponse(Container container, Options filter) {
            var contentProvider = PluginManager.GetComponentFor<IContentProvider>(container);
            if (contentProvider == null) return null;

            var contents = contentProvider.GetContents(container, filter);
            return contents == null ? null : new ContentsResponse(container, contents);
        }


        #endregion



        #region Artwork



        public Image GetArtwork(Container container, Options filter) {

            // Try to get artwork from cache
            var artwork = this.GetArtworkFromCache(container, filter);

            // Get artwork from the plugin
            if (artwork == null) {
                // Get the artworkprovider of the container
                var contentArtworkProvider = PluginManager.GetComponentFor<IArtworkProvider>(container);
                if (contentArtworkProvider != null) {
                    artwork = contentArtworkProvider.GetArtwork(container, filter);
                }
            }

            return artwork;
        }


        Image GetArtworkFromCache(Container container, Options filter) {
            return null;
        }


        ///// <summary>
        ///// Contains the cached artwork results of artwork that is pending or not found
        ///// </summary>
        //Dictionary<string, ArtworkResult> _artworkResultCache = new Dictionary<string, ArtworkResult>();

        ///// <summary>
        ///// Gets the artwork for the given item
        ///// </summary>
        ///// <param name="container">The container in which the item resides</param>
        ///// <param name="item">The item for which to find artwork</param>
        ///// <param name="client">The client for which the artwork is retrieved</param>
        ///// <param name="uri">The uri which was called</param>
        ///// <returns>An ArtworkResult object containing the artwork and its status and type</returns>
        //public ArtworkResult Artwork(Container container, IItem item, Client client, Uri uri) {
        //    // Build empty result object
        //    var noResult = new ArtworkResult();
            
        //    // No item? No result
        //    if (item == null) return noResult;

        //    // Get unique key
        //    var uniqueKey = ArtworkHelper.GetUniqueKey(item);
        //    if (uniqueKey == null) return noResult;

        //    // Return artwork for unique key
        //    return Artwork(container, uniqueKey, item, null, client, uri);
        //}


        ///// <summary>
        ///// Gets the artwork for the given filter
        ///// </summary>
        ///// <param name="container">The container in which the item resides</param>
        ///// <param name="filter">The filter for which to find artwork</param>
        ///// <param name="client">The client for which the artwork is retrieved</param>
        ///// <param name="uri">The uri which was called</param>
        ///// <returns>An ArtworkResult object containing the artwork and its status and type</returns>
        //public ArtworkResult Artwork(Container container, Options filter, Client client, Uri uri) {

        //    // Get hash input from filter
        //    var uniqueKey = String.Join(",", 
        //        new SortedDictionary<string, string>(filter).Select(
        //            kv => kv.Key + ":" + kv.Value
        //        )
        //    ).ToLower();

        //    // Return artwork for hash input
        //    return Artwork(container, uniqueKey, null, filter, client, uri);
        //}


        ///// <summary>
        ///// Gets the artwork for the given unique key, which was sourced from the given item or filter.
        ///// Either item or filter should be given.
        ///// If the artwork is not found in the cache, the call is delegated to a thread to retrieve the
        ///// item from a plugin or service.
        ///// </summary>
        ///// <param name="container">The container in which the artwork subject resides</param>
        ///// <param name="uniqueKey">The hash input value of the artwork</param>
        ///// <param name="item">The item for which to find artwork</param>
        ///// <param name="filter">The filter for which to find artwork</param>
        ///// <param name="client">The client for which the artwork is retrieved</param>
        ///// <param name="uri">The uri which was called</param>
        ///// <returns>An ArtworkResult object</returns>
        //ArtworkResult Artwork(Container container, string uniqueKey, IItem item, Options filter, Client client, Uri uri) {

        //    // Result var
        //    ArtworkResult result;

        //    // Check if this artwork is in the results cache
        //    // Artwork is only present there if was tried at least once and it is not (yet) available
        //    lock (_artworkResultCache) {
        //        if (_artworkResultCache.ContainsKey(uniqueKey)) {

        //            // Get the result cache
        //            result = _artworkResultCache[uniqueKey];

        //            // If the artwork was previously unavailable, but the retry period has passed, remove it from the status list so we can retry it
        //            if (result.Status == ArtworkStatus.Unavailable && result.DateTime + _artworkRetryPeriod < DateTime.Now)
        //                _artworkResultCache.Remove(uniqueKey);

        //            // Otherwise, return the existing status
        //            else
        //                return result;

        //        }
        //        else
        //            result = new ArtworkResult();
            
        //    }

        //    // Check if we have any input
        //    if (item == null && filter == null) {
        //        Log("This should never happen! Y U implement wrong!?!?!?", Logger.LogLevel.Error);
        //        return result;
        //    }
            
        //    // Get form cache
        //    result = ArtworkHelper.GetFromCache(uniqueKey);

        //    // Set type
        //    result.Type = filter == null ? ArtworkHelper.GetDefaultArtworkType(item) : ArtworkHelper.GetDefaultArtworkType(filter);

        //    // We have cache!
        //    // TODO???: invalidate cache by checking creation date of cache file
        //    if (result.Artwork != null) {
        //        return result;
        //    }

        //    // No cache and no client (url must have been called from outside the app), so we make the client wait
        //    else if (client == null) {
        //        return GetNonCachedArtwork(result, container, uniqueKey, item, filter, client, uri);
        //    }

        //    // No cache but a client, get artwork in different thread to free HTTP request
        //    else {
        //        result.Status = ArtworkStatus.Pending;
        //        new Thread(() => GetNonCachedArtwork(result, container, uniqueKey, item, filter, client, uri)).Start();
        //        return result;
        //    }

        //}


        ///// <summary>
        ///// Gets the artwork for the given unique key, which was sourced from the given item or filter.
        ///// Either item or filter should be given. The artwork is sourced from a plugin or service.
        ///// If artwork is found, it is stored in the cache and the client is notified of the availability.
        ///// </summary>
        ///// <param name="container">The container in which the artwork subject resides</param>
        ///// <param name="uniqueKey">The unique key value of the artwork</param>
        ///// <param name="item">The item for which to find artwork</param>
        ///// <param name="filter">The filter for which to find artwork</param>
        ///// <param name="client">The client for which the artwork is retrieved</param>
        ///// <param name="uri">The uri which was called</param>
        ///// <returns>An image if artwork was found, otherwise null</returns>
        //ArtworkResult GetNonCachedArtwork(ArtworkResult result, Container container, string uniqueKey, IItem item, Options filter, Client client, Uri uri) {

        //    // Artwork result object
        //    result.Status = ArtworkStatus.Pending;

        //    // Check if we have any input
        //    if (item == null && filter == null) {
        //        Log("This should never happen! Y U implement wrong!?!?!?", Logger.LogLevel.Error);
        //        return result;
        //    }

        //    // If we are already processing this image, bail out
        //    lock (_artworkResultCache) {
        //        if (_artworkResultCache.ContainsKey(uniqueKey)) {
        //            Log("This should never happen! Y U implement wrong!?!?!?", Logger.LogLevel.Error);
        //            return _artworkResultCache[uniqueKey];
        //        }
        //    }

        //    // Ensure pending artwork is always removed from the results cache
        //    try {
                
        //        // We are processing this image
        //        lock (_artworkResultCache) {
        //            _artworkResultCache[uniqueKey] = result;
        //        }

        //        // Get the image from the plugin
        //        var contentArtworkProvider = PluginManager.GetComponent<IContentArtworkProvider>(container);
        //        if (contentArtworkProvider != null) {
        //            Image artwork;
        //            result.Status = filter == null ? contentArtworkProvider.GetArtwork(container, item, out artwork) : contentArtworkProvider.GetArtwork(container, filter, out artwork);
        //            result.Artwork = artwork;
        //        }

        //        // No image yet? Get from artwork service
        //        if (result.Artwork == null)
        //            result = filter == null ? ArtworkHelper.GetFromArtworkService(item) : ArtworkHelper.GetFromArtworkService(filter);

        //        // if we have an image, store it in cache
        //        if (result.Artwork != null) {

        //            // Resize if image is too large
        //            if (result.Artwork.Width > 1024 || result.Artwork.Height > 1024) {
        //                using (var sourceArtwork = result.Artwork) {
        //                    result.Artwork = sourceArtwork.Resize(new Size(1024, 1024), ResizeMode.ContainAndShrink);
        //                }
        //            }

        //            // Save to cache
        //            ArtworkHelper.SaveToCache(result.Artwork, uniqueKey);
        //        }

        //        // Remove from cache if we have retrieved an image
        //        lock (_artworkResultCache) {
        //            if (result.Status == ArtworkStatus.Retrieved)
        //                _artworkResultCache.Remove(uniqueKey);
        //        }

        //        // Notify client we are done
        //        if (client != null)
        //            _server.Send(client, new ArtworkResponse(uri.PathAndQuery.TrimStart(new char[]{'/'}), null));

        //    }

        //    // Ensure procesing status is removed when an exception occurs
        //    catch (Exception) {
        //        lock (_artworkResultCache) {
        //            if (_artworkResultCache.ContainsKey(uniqueKey) && _artworkResultCache[uniqueKey].Status == ArtworkStatus.Pending)
        //                _artworkResultCache.Remove(uniqueKey);
        //        }
        //    }

        //    return result;
        //}




        #endregion



        #region Enqueueing


        /// <summary>
        /// Builds a new queue with a given set of items and starts playback from the given index
        /// </summary>
        /// <param name="container">The container the items are located in</param>
        /// <param name="filter">The filter used to search for the items</param>
        /// <param name="start">The start index</param>
        /// <returns>The new queue</returns>
        public Queue BuildQueue(Container container, Options filter, int start = 0) {

            // Get the queue items and bail out if no items
            var items = GetItems(container, filter);
            if (items == null || items.Count() == 0) return null;

            // Build the queue
            var queue = ApplyQueue(new Queue(items, container));

            // Set index, starting the queue
            if (start < queue.UpcomingCount) queue.Index = start;

            return queue;
        }


        /// <summary>
        /// Builds a new, empty queue
        /// </summary>
        /// <returns></returns>
        public Queue BuildQueue() {
            return ApplyQueue(new Queue());
        }


        /// <summary>
        /// Applies the given queue as the current queue
        /// </summary>
        /// <param name="queue">The queue to apply</param>
        /// <returns>The applied queue</returns>
        Queue ApplyQueue(Queue queue) {
            queue.ItemsUpdated += QueueItemsUpdated;
            queue.IndexChanged += QueueIndexChanged;
            return this.Queue = queue;
        }


        /// <summary>
        /// Gets the items from the given container, based on a filter
        /// </summary>
        /// <param name="container">The container the items are located in</param>
        /// <param name="filter">The filter used to search for the items</param>
        /// <returns>An IEnumerable containing the requested items, or null if no ContentProvider was found for the given Container</returns>
        public IEnumerable<IItem> GetItems(Container container, Options filter) {

            // Get the plugin for the container
            var contentProvider = PluginManager.GetComponentFor<IContentProvider>(container);
            if (contentProvider == null) return null;

            // Get the items for this container / filter combination
            return contentProvider.GetItems(container, filter);
        }


        /// <summary>
        /// Called when the contents of the current Queue are changed
        /// </summary>
        void QueueItemsUpdated(Queue queue) {
            BroadcastQueue(queue);
        }


        /// <summary>
        /// Called when the index of the current Queue is changed
        /// </summary>
        /// <param name="queue"></param>
        void QueueIndexChanged(Queue queue) {
            BroadcastQueue(queue);

            if (queue.Current != null)
                Play(queue.Current.Item);
            else {
                ClearPlayer();
                if (queue == this.Queue)
                    this.Queue = null;
            }
        }


        #endregion



        #region Playback

        
        /// <summary>
        /// Starts playback
        /// </summary>
        /// <param name="item">The item to play</param>
        void Play(IItem item) {

            // Get the player for the item
            var newPlayer = GetPlayerForItem(item);

            // None found
            if (newPlayer == null) {
                Log("No player found for item with type " + item.GetType().ToString(), Logger.LogLevel.Error);
                return;
            }

            // If we have a new player, set it up
            if (Player == null && newPlayer != Player) {
                ClearPlayer();
                Player = newPlayer;
                Player.ItemFinished += Player_PlaybackFinished;
                Player.StatusUpdated += Player_StatusUpdated;
            }

            // Play the item
            Player.Play(item);
        }


        /// <summary>
        /// Clears the current player
        /// </summary>
        void ClearPlayer() {
            if (Player != null) {
                Player.Stop();
                Player.ItemFinished -= Player_PlaybackFinished;
                Player.StatusUpdated -= Player_StatusUpdated;
                Player = null;
            }
        }


        /// <summary>
        /// Called when the current Player has finished playback of an item
        /// </summary>
        /// <param name="player">The player that was playing</param>
        /// <param name="item">The item that was playing</param>
        void Player_PlaybackFinished(IPlayer player, IItem item) {
            Queue.GoNext();
        }


        /// <summary>
        /// Called when the current player has updated
        /// </summary>
        /// <param name="player">The player that has been updated</param>
        void Player_StatusUpdated(IPlayer player) {
            
            // (de)activate devices based on player status
            if (player.Playing != _playing) {
                _playing = player.Playing;
                foreach (var device in Device.Where(d => d.SupportsCapability(DeviceCapabilities.AutoSetActive))) {
                    device.AutoSetActive(_playing);
                }
            }

            // Broadcast status to clients
            _server.Broadcast(new PlaybackResponse(player));
        }


        /// <summary>
        /// Gets the IPlayer plugin which can play the given item
        /// </summary>
        /// <param name="item">The item to play</param>
        /// <returns>The corresponding IPlayer</returns>
        IPlayer GetPlayerForItem(IItem item) {
            return PluginManager.GetComponent<IPlayer>().FirstOrDefault(p => p.CanPlay(item));
        }

        #endregion

        


    }


    public class NoQueueException : Exception {
        public NoQueueException() : base() { }
    }
    public class NoPlayerException : Exception {
        public NoPlayerException() : base() { }
    }

}