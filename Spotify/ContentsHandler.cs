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



        #region Playlist adding / updating


        /// <summary>
        /// Tracks the given playlist.
        /// When the state or track meta data of the given playlist is changed,
        /// the playlist is presented to the StateChanged method for further processing.
        /// </summary>
        /// <param name="playlist">The playlist to track</param>
        void AddPlaylist(SpotiFire.Playlist playlist) {
            //LogPlaylist(playlist, "added");
            //if (playlist.IsFullyLoaded())
            //    this.CreateOrUpdate(playlist);
            //playlist.StateChanged += playlist_StateChanged;
            //playlist.MetadataUpdated += playlist_MetadataUpdated;
            //playlist.Renamed += playlist_Renamed;
            //playlist.UpdateInProgress += playlist_UpdateInProgress;

            lock (playlist) {
                LogPlaylist(playlist, "added");

                if (playlist.IsFullyLoaded()) {
                    playlist.SetKnown(true);
                    Bind(playlist);
                    if (!playlist.HasPendingChanges)
                        CreateOrUpdate(playlist);
                }

                playlist.StateChanged += playlist_StateChanged;
            }

        }

        
        /// <summary>
        /// Called when the state of a playlist changes.
        /// This method checks if the playlist is completely loaded and then updates or adds the playlist.
        /// </summary>
        void playlist_StateChanged(SpotiFire.Playlist sender, PlaylistEventArgs e) {
            //LogPlaylist(sender, "state changed");
            //if (this.IsInContainer(sender) && sender.IsFullyLoaded())
            //    this.CreateOrUpdate(sender);

            lock (sender) {
                LogPlaylist(sender, "state changed");

                if (this.IsInContainer(sender) && sender.IsFullyLoaded()) {

                    if (!sender.IsKnown()) {
                        sender.SetKnown(true);
                        Bind(sender);
                    }

                    if (!sender.HasPendingChanges) {
                        if (!sender.IsKnown()) {
                            CreateOrUpdate(sender);
                        }
                        else if (!sender.GetHandlerStatus().UpdateInProgress) {
                            CreateOrUpdate(sender);
                        }

                    }
                }

            }
            
        }



        void playlist_UpdateInProgress(SpotiFire.Playlist sender, PlaylistEventArgs e) {
            //LogPlaylist(sender, "update in progress", e);

            lock (sender) {
                LogPlaylist(sender, "update in progress", e);
                var status = sender.GetHandlerStatus();
                status.UpdateInProgress = !e.UpdateComplete;
                if (this.IsInContainer(sender)) {
                    if (e.UpdateComplete)
                        CreateOrUpdate(sender);
                }
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
                Log("IGNORING PLAYLIST: " + playlist.Type.ToString());
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
                    //Log("TT: " + (playlist.HasPendingChanges ? "... " : "") + playlist.Name);
                    LogPlaylist(playlist, "track tracks");
                    //this.TrackTracks(playlist);
                }
                // If the playlist is known, it means that the playlist "has become an album".
                // So we delete the playlist. The track events are already bound
                else {
                    //Log("RP: " + (playlist.HasPendingChanges ? "... " : "") + toucheePlaylist.Name);
                    LogPlaylist(playlist, "playlist became album");
                    toucheePlaylist.Dispose();
                }
            }

            // Playlist is actually a playlist and not yet present
            else if (toucheePlaylist == null) {
                toucheePlaylist = isStarred
                    ? new StarredPlaylist(playlist, Touchee.Medium.Local)
                    : new Spotify.Media.Playlist(playlist, Touchee.Medium.Local);
                toucheePlaylist.Save();
                //Log("CR: " + (playlist.HasPendingChanges ? "... " : "") + toucheePlaylist.Name);
                LogPlaylist(playlist, "create");
                //this.TrackTracks(playlist);
            }

            // We have to update the playlist
            else {
                var changed = toucheePlaylist.Update(playlist);
                if (changed)
                    toucheePlaylist.Save();
                //Log("UP: " + (playlist.HasPendingChanges ? "... " : "") + toucheePlaylist.Name);
                LogPlaylist(playlist, "update");
            }

            return toucheePlaylist;
        }


        #endregion



        #region Playlist removal


        /// <summary>
        /// Removes the playlist
        /// </summary>
        /// <param name="playlist">The playlist to remove</param>
        void RemovePlaylist(SpotiFire.Playlist playlist) {
            var altID = playlist.GetLink().ToString();
            if (Spotify.Media.Playlist.ExistsByAltID(altID)) {
                playlist.StateChanged -= playlist_StateChanged;
                //playlist.MetadataUpdated -= playlist_MetadataUpdated;
                var toucheePlaylist = Spotify.Media.Playlist.FindByAltID(altID);
                toucheePlaylist.Dispose();
                Log("RM: " + toucheePlaylist.Name);
                //playlist.Tracks.CollectionChanged -= Tracks_CollectionChanged;
            }
        }


        #endregion


        
        #region Track handling


        void TrackTracks(SpotiFire.Playlist playlist) {

            this.AddTracks(playlist);

            //playlist.Tracks.CollectionChanged += Tracks_CollectionChanged;
        }

        void Tracks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            throw new NotImplementedException();
        }

        void AddTracks(SpotiFire.Playlist playlist) {
            this.AddTracks(playlist, playlist.Tracks);
        }

        void AddTracks(SpotiFire.Playlist playlist, IEnumerable<SpotiFire.Track> tracks) {
            foreach (var track in tracks) {

            }
        }

        void RemoveTracks(SpotiFire.Playlist playlists, IEnumerable<SpotiFire.Track> tracks) {
            foreach (var track in tracks) {

            }
        }


        #endregion


        /// <summary>
        /// Called when the metadata of one or more tracks of a playlist is changed.
        /// </summary>
        void playlist_MetadataUpdated(SpotiFire.Playlist sender, PlaylistEventArgs e) {
            var loaded = sender.IsFullyLoaded();
            var ready = sender.IsFullyReady();
            //LogPlaylist(sender, "metadata updated");
        }



        //Dictionary<SpotiFire.Playlist, bool> _playlistUpdateStatus = new Dictionary<SpotiFire.Playlist, bool>();

        //bool IsUpdateInProgress(SpotiFire.Playlist playlist) {
        //    return _playlistUpdateStatus[playlist];
        //}
        //void SetUpdateInProgress(SpotiFire.Playlist playlist, bool inProgress) {
        //    _playlistUpdateStatus[playlist] = inProgress;
        //}
        //void RemoveUpdateStatus(SpotiFire.Playlist playlist) {
        //    _playlistUpdateStatus.Remove(playlist);
        //}



        void playlist_Renamed(SpotiFire.Playlist sender, PlaylistEventArgs e) {
            LogPlaylist(sender, "renamed");
        }

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



        void playlist_ImageChanged(SpotiFire.Playlist sender, PlaylistEventArgs e) {
            throw new NotImplementedException();
        }



        // TODO: move content-related change binds to somewhere else, so they are only called
        // when we already have a touchee representation of the playlist
        void Bind(SpotiFire.Playlist playlist) {
            playlist.ImageChanged += playlist_ImageChanged;
            playlist.MetadataUpdated += playlist_MetadataUpdated;
            playlist.Renamed += playlist_Renamed;
            playlist.UpdateInProgress += playlist_UpdateInProgress;
        }

        void UnBind(SpotiFire.Playlist playlist) {
            playlist.ImageChanged -= playlist_ImageChanged;
            playlist.MetadataUpdated -= playlist_MetadataUpdated;
            playlist.Renamed -= playlist_Renamed;
            playlist.UpdateInProgress -= playlist_UpdateInProgress;
            playlist.StateChanged -= playlist_StateChanged;
        }


        
        Spotify.Media.Playlist GetToucheePlaylist(SpotiFire.Playlist playlist) {
            Spotify.Media.Playlist toucheePlaylist = null;

            if (playlist.Type == PlaylistType.Playlist) {
                var altID = playlist.GetLink().ToString();
                if (Spotify.Media.Playlist.ExistsByAltID(altID))
                    toucheePlaylist = (Spotify.Media.Playlist)Spotify.Media.Playlist.FindByAltID(altID);
            }
            return toucheePlaylist;
        }


        /// <summary>
        /// Checks whether the given playlist is present in the PlaylistContainer of the session
        /// </summary>
        /// <param name="playlist">The playlist to check</param>
        /// <returns>True if the playlist resides in the PlaylistContainer, otherwise false</returns>
        bool IsInContainer(SpotiFire.Playlist playlist) {
            return _session.PlaylistContainer.Playlists.Contains(playlist);
        }

    }

}
