using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
//using System.Text;
using System.Threading.Tasks;

using Touchee;
using SpotiFire;
using Spotify.Media;

namespace Spotify {
    

    /// <summary>
    /// Manages the content present for a Spotify session.
    /// All tracks present in playlists is arranged into albums and playlists.
    /// </summary>
    public class ContentsHandler : Base {


        #region Privates

        /// <summary>
        /// The session
        /// </summary>
        Session _session;
        

        Music.Media.MasterPlaylist _masterPlaylist;
        ConnectionState _previousConnectionState;

        #endregion



        #region Constructor


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="session">The Spotify session to handle</param>
        public ContentsHandler(Session session) {
            _session = session;
            _session.ConnectionstateUpdated += ConnectionstateUpdated;
            var masterContainer = Touchee.Medium.Local.MasterContainer;
            if (masterContainer == null)
                Log("Master container for local medium not found. This should not happen!", Logger.LogLevel.Error);
            else
                _masterPlaylist = (Music.Media.MasterPlaylist)masterContainer;
        }


        #endregion



        #region Connection State Updated


        /// <summary>
        /// Called when the connection state of the session is updated.
        /// If the session is logged in, all present playlists are added and
        /// listeners are attached to the playlist collection list.
        /// </summary>
        async void ConnectionstateUpdated(Session sender, SessionEventArgs e) {

            switch (sender.ConnectionState) {

                // Logged out
                case ConnectionState.LoggedOut:

                    if (_previousConnectionState == ConnectionState.LoggedIn) {
                        
                        // Remove all playlists
                        MasterPlaylist.Instance.Dispose();
                        var playlists = Touchee.Medium.Local.Containers.OfType<Spotify.Media.Playlist>();
                        lock (playlists) {
                            foreach (var playlist in playlists)
                                playlist.Dispose();
                        }
                    }

                    break;

                // Offline state (when logged in, but not completely yet)
                case ConnectionState.Offline:
                    break;

                // User has logged in
                case ConnectionState.LoggedIn:

                    // Save the master playlist
                    MasterPlaylist.Instance.Save();

                    // Wait for the playlist container
                    await _session.PlaylistContainer;

                    // Track initial set of playlists and starred list
                    Log("PL: " + _session.PlaylistContainer.Playlists.Count);
                    this.AddPlaylists(_session.PlaylistContainer.Playlists);
                    this.AddPlaylist(_session.Starred);

                    // Add event handler for removal/adding of playlists
                    _session.PlaylistContainer.Playlists.CollectionChanged += Playlists_CollectionChanged;

                    break;
                    
                // User has been disconnected after having been logged in
                case ConnectionState.Disconnected:
                    break;

                // No clue
                case ConnectionState.Undefined:
                    break;

            }

            _previousConnectionState = sender.ConnectionState;

        }


        #endregion



        #region PlaylistContainer CollectionChanged


        /// <summary>
        /// Called when one or more playlists are changed in Spotify.
        /// Since it is possible that playlists are added / removed whilst the app is not connected,
        /// we just perform a sync instead of adding/removing playlists based on these calls.
        /// </summary>
        void Playlists_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (e.NewItems != null)
                this.AddPlaylists(e.NewItems.Cast<SpotiFire.Playlist>());
            if (e.OldItems != null)
                this.RemovePlaylists(e.OldItems.Cast<SpotiFire.Playlist>());
        }
        

        /// <summary>
        /// Tracks the given set of playlists.
        /// When the state or track meta data of the given playlists is changed,
        /// the playlist is presented to the StateChanged method for further processing.
        /// </summary>
        /// <param name="playlists">The set of playlists to track</param>
        void AddPlaylists(IEnumerable<SpotiFire.Playlist> playlists) {
            foreach (var playlist in playlists)
                this.AddPlaylist(playlist);
        }


        /// <summary>
        /// Removes the set of playlists
        /// </summary>
        /// <param name="playlists">The set of playlists to remove</param>
        void RemovePlaylists(IEnumerable<SpotiFire.Playlist> playlists) {
            foreach (var playlist in playlists)
                this.RemovePlaylist(playlist);
        }

        
        #endregion



        #region Playlist adding / updating / removing


        /// <summary>
        /// Called when a playlist is added to the playlist container.
        /// When the playlist is fully loaded, the playlist is 'introduced', meaning events
        /// changing the meta-data of this playlist will be tracked.
        /// If no pending changes are present, the playlist will be stored.
        /// Also, every playlist that is added will have its state changes tracked.
        /// </summary>
        void AddPlaylist(SpotiFire.Playlist playlist) {
            lock (playlist) {
                //LogPlaylist(playlist, "added");
                if (playlist.IsFullyLoaded()) {
                    Introduce(playlist);
                    if (!playlist.HasPendingChanges)
                        CreateOrUpdate(playlist);
                }
                playlist.StateChanged += playlist_StateChanged;
            }
        }

        
        /// <summary>
        /// Called when the state of a playlist changes.
        /// If the playlist is in the playlist container and fully loaded, it introduces the 
        /// playlist if that has not happened before.
        /// If no pending changes are present and no update is in progress, the playlist
        /// will be stored / updated.
        /// </summary>
        void playlist_StateChanged(SpotiFire.Playlist sender, PlaylistEventArgs e) {
            lock (sender) {
                //LogPlaylist(sender, "state changed");
                if (this.IsInSession(sender) && sender.IsFullyLoaded()) {
                    if (!sender.IsKnown())
                        Introduce(sender);
                    if (!sender.HasPendingChanges && !sender.IsUpdateInProgress())
                        CreateOrUpdate(sender);
                }
            }
        }


        /// <summary>
        /// Called when an update action of a introduced playlist is started or has ended.
        /// If the playlist is in the playlist container and the update is completed,
        /// the playlist will be stored / updated.
        /// </summary>
        void playlist_UpdateInProgress(SpotiFire.Playlist sender, PlaylistEventArgs e) {
            lock (sender) {
                //LogPlaylist(sender, "update in progress", e);
                sender.SetUpdateInProgress(!e.UpdateComplete);
                if (this.IsInSession(sender) && e.UpdateComplete)
                    CreateOrUpdate(sender);
            }
        }


        /// <summary>
        /// Creates or updates the Touchee data for the given SpotiFire playlist
        /// </summary>
        /// <param name="playlist">The SpotiFire playlist that should be created or updated</param>
        /// <returns>The resulting Touchee Playlist</returns>
        Spotify.Media.Playlist CreateOrUpdate(SpotiFire.Playlist playlist) {
            var toucheePlaylist = GetToucheePlaylist(playlist);

            // Process only 'real' playlists
            if (playlist.Type != PlaylistType.Playlist) {
                //Log("IGNORING PLAYLIST: " + playlist.Type.ToString());
                return null;
            }

            // Collect some vars
            var isAlbum = playlist.IsAlbum();
            var isStarred = playlist == _session.Starred;

            // Playlist is an Album, so should not be seen as playlist, but the tracks need to be added to the library
            if (isAlbum && !isStarred) {
                // This playlist is not known yet, so the tracks for this playlist should be tracked.
                // This can occur more than once!
                if (toucheePlaylist == null) {
                    //LogPlaylistSimple(playlist, "album");
                    this.TrackTracks(playlist);
                }
                // If the playlist is known, it means that the playlist "has become an album".
                // So we delete the playlist. The track events are already bound
                else {
                    //LogPlaylistSimple(playlist, "playlist became album");
                    toucheePlaylist.Dispose();
                }
            }

            // Playlist is actually a playlist and not yet present
            else if (toucheePlaylist == null) {
                //LogPlaylistSimple(playlist, "create");
                toucheePlaylist = isStarred
                    ? new StarredPlaylist(playlist, Touchee.Medium.Local)
                    : new Spotify.Media.Playlist(playlist, Touchee.Medium.Local);
                toucheePlaylist.Save();
                this.TrackTracks(playlist);
            }

            // We have to update the playlist
            else {
                //LogPlaylistSimple(playlist, "update");
                toucheePlaylist.Update(playlist);
            }

            return toucheePlaylist;
        }

        
        /// <summary>
        /// Removes the playlist
        /// </summary>
        /// <param name="playlist">The playlist to remove</param>
        void RemovePlaylist(SpotiFire.Playlist playlist) {
            var toucheePlaylist = GetToucheePlaylist(playlist);
            if (toucheePlaylist != null)
                toucheePlaylist.Dispose();
            Forget(playlist);
        }


        #endregion



        #region Playlist other events


        /// <summary>
        /// Called when the metadata of one or more tracks of a playlist is changed.
        /// </summary>
        void playlist_MetadataUpdated(SpotiFire.Playlist sender, PlaylistEventArgs e) {
            //LogPlaylist(sender, "metadata updated");
        }


        /// <summary>
        /// Called when a playlist is removed
        /// </summary>
        void playlist_Renamed(SpotiFire.Playlist sender, PlaylistEventArgs e) {
            var toucheePlaylist = GetToucheePlaylist(sender);
            if (toucheePlaylist != null)
                toucheePlaylist.Update(sender);
        }


        /// <summary>
        /// Called when the image of a playlist is changed
        /// </summary>
        void playlist_ImageChanged(SpotiFire.Playlist sender, PlaylistEventArgs e) {
            throw new NotImplementedException();
        }


        #endregion



        #region Track handling


        Dictionary<Spotify.Media.Track, int> _trackUsage = new Dictionary<Spotify.Media.Track, int>();
        int _trackCount = 0;

        void TrackTracks(SpotiFire.Playlist playlist) {
            if (playlist.IsTrackingTracks())
                return;

            playlist.SetTrackingTracks(true);
            this.AddTracks(playlist);
            playlist.Tracks.CollectionChanged += tracks_CollectionChanged;
        }

        void tracks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            var tracks = (SpotiFire.Collections.ITrackCollection)sender;
            var playlist = tracks.Playlist;
            //var tracks = (SpotiFire.Collections.IObservableSPList<SpotiFire.Track>)sender;
            LogPlaylistSimple(playlist, "track collection changed");
        }

        void AddTracks(SpotiFire.Playlist playlist) {
            this.AddTracks(playlist, playlist.Tracks);
        }

        void AddTracks(SpotiFire.Playlist playlist, IEnumerable<SpotiFire.Track> tracks) {
            var toucheePlaylist = GetToucheePlaylist(playlist);
            foreach (var track in tracks) {
                lock (_trackUsage) {
                    var toucheeTrack = Spotify.Media.Track.FindOrDefaultByAltID<Spotify.Media.Track>(track.GetLink().ToString());

                    // Track does not exist in Touchee yet, so we add save it and add it to the master container
                    if (toucheeTrack == null) {
                        toucheeTrack = new Spotify.Media.Track(track);
                        _trackUsage[toucheeTrack] = 1;
                        toucheeTrack.Save();
                        _masterPlaylist.Add(toucheeTrack);
                    }

                    // Track already exists, so we up the track usage counter
                    else
                        _trackUsage[toucheeTrack] = _trackUsage[toucheeTrack] + 1;
                    

                    // Add to the playlist
                    if (toucheePlaylist != null)
                        toucheePlaylist.Add(toucheeTrack);

                    _trackCount++;
                    if (_trackCount % 50 == 0) Log(_trackCount.ToString() + " Spotify tracks found");
                }
            }
        }

        void RemoveTracks(SpotiFire.Playlist playlists, IEnumerable<SpotiFire.Track> tracks) {
            foreach (var track in tracks) {

            }
        }


        #endregion







        #region Utility


        /// <summary>
        /// Introduce a playlist to the environment. This sets the Known value of the playlist
        /// to true and binds the necessary events.
        /// TODO: move content-related change binds to somewhere else, so they are only called
        /// when we already have a touchee representation of the playlist
        /// </summary>
        void Introduce(SpotiFire.Playlist playlist) {
            playlist.SetKnown(true);
            playlist.ImageChanged += playlist_ImageChanged;
            playlist.MetadataUpdated += playlist_MetadataUpdated;
            playlist.Renamed += playlist_Renamed;
            playlist.UpdateInProgress += playlist_UpdateInProgress;
        }


        /// <summary>
        /// Forget a playlist. This sets the Known value of the Playlist to false and unbinds
        /// all the events.
        /// </summary>
        void Forget(SpotiFire.Playlist playlist) {
            playlist.SetKnown(false);
            playlist.ImageChanged -= playlist_ImageChanged;
            playlist.MetadataUpdated -= playlist_MetadataUpdated;
            playlist.Renamed -= playlist_Renamed;
            playlist.UpdateInProgress -= playlist_UpdateInProgress;
            playlist.StateChanged -= playlist_StateChanged;
            playlist.Tracks.CollectionChanged -= tracks_CollectionChanged;
        }


        /// <summary>
        /// Gets the Touchee Playlist corresponding to eht given SpotiFire playlist
        /// </summary>
        /// <param name="playlist">The SpotiFire playlist to look for</param>
        /// <returns>The corresponding Touchee playlist, or null of none is found</returns>
        Spotify.Media.Playlist GetToucheePlaylist(SpotiFire.Playlist playlist) {
            return playlist.Type == PlaylistType.Playlist
                ? Spotify.Media.Playlist.FindOrDefaultByAltID<Spotify.Media.Playlist>(playlist.GetLink().ToString())
                : null;
        }


        /// <summary>
        /// Checks whether the given playlist is present in the session
        /// </summary>
        /// <param name="playlist">The playlist to check</param>
        /// <returns>True if the playlist resides in the PlaylistContainer, otherwise false</returns>
        bool IsInSession(SpotiFire.Playlist playlist) {
            return _session.PlaylistContainer.Playlists.Contains(playlist) || playlist == _session.Starred;
        }


        #endregion



        #region Debug logging


        void LogPlaylist(SpotiFire.Playlist playlist, string ev, PlaylistEventArgs e = null) {
            lock (this) {
                Console.WriteLine(ev.ToUpper());
                Console.WriteLine("  Playlist: " + (playlist.IsLoaded ? playlist.Name : "<unknown>"));
                Console.WriteLine("  - Loaded:             " + playlist.IsLoaded.ToString());
                Console.WriteLine("  - Known:              " + playlist.IsKnown().ToString());
                if (playlist.IsLoaded) {
                    Console.WriteLine("  - IsCollaborative:    " + playlist.IsCollaborative.ToString());
                    Console.WriteLine("  - HasPendingChanges:  " + playlist.HasPendingChanges.ToString());
                    Console.WriteLine("  - IsUpdateInProgress: " + playlist.IsUpdateInProgress().ToString());
                    Console.WriteLine("  - AllTracksLoaded:    " + playlist.AllTracksLoaded().ToString());
                }
                if (e != null) {
                    Console.WriteLine("  - Update completed:   " + e.UpdateComplete.ToString());
                }
            }
        }
        void LogPlaylistSimple(SpotiFire.Playlist playlist, string ev, PlaylistEventArgs e = null) {
            lock (this) {
                Console.WriteLine("= " + ev.ToUpper() + " - " + (playlist.IsLoaded ? playlist.Name : "<unknown>"));
            }
        }


        #endregion


    }

}
