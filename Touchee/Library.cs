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
using Touchee.Components.Content;
using Touchee.Components.Media;
using Touchee.Components.Playback;
using Touchee.Plugins;
using Touchee.Media;

namespace Touchee {

    /// <remarks>
    /// 
    /// </remarks>
    public class Library : Base {


        #region Singleton

        /// <summary>
        /// Private constructor
        /// </summary>
        Library() { }

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
        

        #endregion



        #region Properties

        /// <summary>
        /// The library content revision number
        /// </summary>
        public uint Revision { get; protected set; }

        #endregion



        #region Init

        /// <summary>
        /// Initialises the library
        /// </summary>
        /// <param name="mediaWatcherPollingInterval">The interval at which to look for modified media</param>
        public void Init(ToucheeServer server, int mediaWatcherPollingInterval) {
            _server = server;

            // Set retry period from config
            int period;
            Program.Config.TryGetInt("artwork.retryPeriod", out period);
            _artworkRetryPeriod = new TimeSpan(0, 0, period);

            // Instantiate all available MediumWatchers
            // These watch the Medium instances and generate Containers
            _mediumWatchers = PluginManager.GetComponent<IMediumWatcher>().ToList();
            Medium.AfterCreate += Medium_AfterCreate;
            Medium.AfterDispose += Medium_AfterDispose;

            // Watch for container changes
            Container.AfterCreate += ContainerCreated;
            Container.AfterUpdate += ContainerUpdated;
            Container.AfterDispose += ContainerDisposed;
            Container.ContentsChanged += ContainerContentsChanged;

            // Init local and web media
            string name = Program.Config.GetString("name", System.Environment.MachineName);
            LocalMedium.Init(name);
            WebMedium.Init("Webcasts");
            
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
            //_server.Broadcast(GetRevisionResponse());
        }


        /// <summary>
        /// Called when a medium has been detected. Presents this medium to all watchers.
        /// If any watcher watches the medium, the media list is broadcasted.
        /// </summary>
        void Medium_AfterCreate(object sender, Collectable<Medium>.ItemEventArgs e) {
            var watchers = _mediumWatchers.Where(w => w.CanWatch(e.Item));
            if (watchers.Count() > 0) {
                this.Revised();
                _server.Broadcast(GetMediaResponse());
                foreach (var watcher in watchers)
                    watcher.Watch(e.Item);
            }
        }


        /// <summary>
        /// Called when a medium has been removed. Presents this medium to all watchers.
        /// If any watcher watches was watching the medium, the media list is broadcasted.
        /// </summary>
        void Medium_AfterDispose(object sender, Collectable<Medium>.ItemEventArgs e) {
            _mediumWatchers.ForEach(w => w.UnWatch(e.Item));
            this.Revised();
            _server.Broadcast(GetMediaResponse());
        }


        /// <summary>
        /// Called when a container has been created. 
        /// </summary>
        void ContainerCreated(object sender, Collectable<Container>.ItemEventArgs e) {
            _server.Broadcast(GetContainersResponse(e.Item.Medium));
        }


        /// <summary>
        /// Called when a container has been updated. All containers of the corresponding
        /// medium are broadcasted, in order to update the complete list.
        /// </summary>
        void ContainerUpdated(object sender, Collectable<Container>.ItemEventArgs e) {
            _server.Broadcast(GetContainersResponse(e.Item.Medium));
        }


        /// <summary>
        /// Called when a container has been disposed. All containers of the corresponding
        /// medium are broadcasted, in order to update the complete list.
        /// </summary>
        void ContainerDisposed(object sender, Collectable<Container>.ItemEventArgs e) {
            _server.Broadcast(GetContainersResponse(e.Item.Medium));
        }


        /// <summary>
        /// Called when the contents of a container have been modified.
        /// </summary>
        /// <param name="container">The container which is modified</param>
        void ContainerContentsChanged(Container container) {
            this.Revised();
            _server.Broadcast(new ContentsChangedResponse(container));
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
                _server.Broadcast(GetContainersResponse(medium));
            });
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
        /// Gets a message containing information on all available media
        /// </summary>
        public MediaResponse GetMediaResponse() {
            return new MediaResponse(Medium.All());
        }


        /// <summary>
        /// Gets a message containing information on all available containers of the given medium
        /// </summary>
        public ContainersResponse GetContainersResponse(Medium medium) {
            return new ContainersResponse(medium);
        }


        /// <summary>
        /// Gets a message containing the content for the given container, type and filter combination
        /// </summary>
        public ContentsResponse GetContentsResponse(Container container, Options filter) {
            var contentProvider = PluginManager.GetComponent<IContentProvider>(container);
            if (contentProvider == null) return null;

            var contents = contentProvider.GetContents(container, filter);
            return new ContentsResponse(container, contents);
        }


        #endregion



        #region Artwork



        public Image GetArtwork(Container container, Options filter) {

            // Try to get artwork from cache
            var artwork = this.GetArtworkFromCache(container, filter);

            // Get artwork from the plugin
            if (artwork == null) {
                // Get the artworkprovider of the container
                var contentArtworkProvider = PluginManager.GetComponent<IArtworkProvider>(container);
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




        #region Queueing





        #endregion







        #region Controlling


        /// <summary>
        /// Start playing a set of items
        /// </summary>
        /// <param name="container">The container in which the items reside</param>
        /// <param name="filter">Filter used to get the items from the container</param>
        public OQueue Play(Container container, Options filter) {

            // Get the plugin for the container
            var contentProvider = PluginManager.GetComponent<IContentProvider>(container);
            if (contentProvider == null) return null;

            // Get the items for this container / filter combination
            var items = contentProvider.GetItems(container, filter);

            // Bail out if no items
            if (items.Count() == 0) return null;

            // Build queue and queue info object
            var queue = new OQueue(items, container.ContentType);

            // Find existing queue of same type
            var existingQueue = OQueue.FirstOrDefault(q => q.ContentType == queue.ContentType);

            // If we have a similar queue, move repeat and shuffle settings
            if (existingQueue != null) {
                queue.Repeat = existingQueue.Repeat;
                queue.Shuffle = existingQueue.Shuffle;
            }

            // Set callbacks on queue
            queue.IndexChanged += queue_IndexChanged;
            queue.ItemsUpdated += queue_ItemsUpdated;
            queue.Finished += queue_Finished;

            // Save the queue
            queue.Save();

            // Set index, starting the queue
            int id = filter["id"];
            if (id > 0) {
                var item = items.FirstOrDefault(i => i.Id == id);
                queue.Current = item;
            }
            else
                queue.Index = filter["index"];
            
            return queue;
        }


        /// <summary>
        /// Skip to the next item in the current queue
        /// </summary>
        /// <param name="queue">The queue to apply this action on</param>
        public void Prev(OQueue queue) {
            if (queue.IsAtFirstItem)
                queue.Index = 0;
            else
                queue.GoPrev();
        }


        /// <summary>
        /// Skip to the previous item in the current queue
        /// </summary>
        /// <param name="queue">The queue to apply this action on</param>
        public void Next(OQueue queue) {
            queue.GoNext();
        }


        /// <summary>
        /// Pause the playback of the current item in the queue
        /// </summary>
        /// <param name="queue">The queue to apply this action on</param>
        public void Pause(OQueue queue) {
            if (queue.CurrentPlayer != null)
                queue.CurrentPlayer.Pause();
        }


        /// <summary>
        /// Resume the playback of the current item in the queue
        /// </summary>
        /// <param name="queue">The queue to apply this action on</param>
        public void Play(OQueue queue) {
            if (queue.CurrentPlayer != null)
                queue.CurrentPlayer.Play();
        }


        /// <summary>
        /// Gets the IPlayer plugin which can play the given item
        /// </summary>
        /// <param name="item">The item to play</param>
        /// <returns>The corresponding IPlayer</returns>
        IPlayer GetPlayerForItem(IItem item) {
            return PluginManager.GetComponent<IPlayer>().FirstOrDefault(p => p.CanPlay(item));
        }


        /// <summary>
        /// Called when the index of the current queue is changed. Starts playing the next track
        /// </summary>
        /// <param name="queue">The queue whos index has changed</param>
        /// <param name="previous">The previous item that was played</param>
        /// <param name="current">The item that is about the be played</param>
        void queue_IndexChanged(OQueue queue, IItem previous, IItem current) {

            // If we have a different type to play, start the correct player and stop colliding players
            if (previous == null || previous.GetType() != current.GetType()) {

                // Get the player for the current item
                var newPlayer = GetPlayerForItem(current);

                // None found
                if (newPlayer == null) {
                    Log("No player found for item with type " + current.GetType().ToString(), Logger.LogLevel.Error);
                    StopQueue(queue);
                    return;
                }

                // Stop queues and players that collide with the new one
                var queues = OQueue.Where(q => q != queue).ToList();
                foreach (var q in queues) {
                    if (q.CurrentPlayer is IAudioPlayer && newPlayer is IAudioPlayer)
                        StopQueue(q);
                    else if (q.CurrentPlayer is IVisualPlayer && newPlayer is IVisualPlayer)
                        StopQueue(q);
                }

                // If we are using an other player, stop the old one and set the necessary callbacks
                if (newPlayer != queue.CurrentPlayer) {
                    StopPlayer(queue.CurrentPlayer);
                    newPlayer.PlaybackFinished += player_PlaybackFinished;
                    queue.CurrentPlayer = newPlayer;
                }

            }

            // Play the current item
            queue.CurrentPlayer.Play(current);
        }


        /// <summary>
        /// Nicely stops a queue and the player it has
        /// </summary>
        /// <param name="queue">The queue to stop</param>
        void StopQueue(OQueue queue) {
            StopPlayer(queue.CurrentPlayer);
            queue.CurrentPlayer = null;
            queue.IndexChanged -= queue_IndexChanged;
            queue.ItemsUpdated -= queue_ItemsUpdated;
            queue.Finished -= queue_Finished;
            queue.Dispose();
        }


        /// <summary>
        /// Nicely stops a player
        /// </summary>
        /// <param name="player">The player to stop</param>
        void StopPlayer(IPlayer player) {
            if (player != null) {
                player.Stop();
                player.PlaybackFinished -= player_PlaybackFinished;
            }
        }


        /// <summary>
        /// Called when the playback of an item has finished
        /// </summary>
        /// <param name="player">The player who was playing the item</param>
        /// <param name="item">The item that has finished</param>
        void player_PlaybackFinished(IPlayer player, IItem item) {
            var queue = OQueue.FirstOrDefault(q => q.CurrentPlayer == player);
            if (queue != null)
                queue.GoNext();
            else
                StopPlayer(player);
        }

        
        /// <summary>
        /// Called when the contents of a queue has changed
        /// </summary>
        /// <param name="queue">The queue whos contents have changed</param>
        void queue_ItemsUpdated(OQueue queue) {
            
        }


        /// <summary>
        /// Called when a queue is done
        /// </summary>
        /// <param name="queue">The corresponding queue</param>
        void queue_Finished(OQueue queue) {
            StopQueue(queue);
        }



        #endregion

    }

}