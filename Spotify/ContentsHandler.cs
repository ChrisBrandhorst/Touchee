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
            Log("TR: " + (playlist.IsLoaded ? playlist.Name : "?"));

            if (playlist.IsLoaded)
                this.CreateOrUpdate(playlist);

            playlist.StateChanged += playlist_StateChanged;
            //playlist.MetadataUpdated += playlist_MetadataUpdated;
        }

        
        /// <summary>
        /// Called when the state of a playlist changes.
        /// This method checks if the playlist is completely loaded and then updates or adds the playlist.
        /// </summary>
        void playlist_StateChanged(SpotiFire.Playlist sender, PlaylistEventArgs e) {
            Log("SC: " + (sender.IsLoaded ? sender.Name : "?"));
            if (sender.IsLoaded && sender.AllTracksLoaded())
                this.CreateOrUpdate(sender);
        }


        void playlist_MetadataUpdated(SpotiFire.Playlist sender, PlaylistEventArgs e) {
            Log("MU: " + (sender.IsLoaded ? sender.Name : "?"));
        }


        Spotify.Media.Playlist CreateOrUpdate(SpotiFire.Playlist playlist) {
            Spotify.Media.Playlist toucheePlaylist = null;

            // Get the alt ID
            var altID = playlist.GetLink().ToString();

            // If we already have this playlist, update it
            if (Spotify.Media.Playlist.ExistsByAltID(altID)) {
                toucheePlaylist = (Spotify.Media.Playlist)Spotify.Media.Playlist.FindByAltID(altID);
                Log("UP: " + toucheePlaylist.Name);
            }

            // Else, add as new playlist and listen to track events
            else {

                toucheePlaylist = playlist == _session.Starred
                    ? new StarredPlaylist(playlist, Touchee.Medium.Local)
                    : new Spotify.Media.Playlist(playlist, Touchee.Medium.Local);
                toucheePlaylist.Save();
                Log("CR: " + toucheePlaylist.Name + (playlist.IsAlbum() ? " Album" : " Playlist"));
                //await this.AddTracks(sender, sender.Tracks);
                //sender.Tracks.CollectionChanged += Tracks_CollectionChanged;
            }
            return null;
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




        void SyncPlaylists() {
            var plc = _session.PlaylistContainer;
            var spotifyPlaylists = plc.Playlists;
            lock (spotifyPlaylists) {
                if (plc.AllPlaylistsLoaded()) {

                    var spotifyAltIDs = new List<string>();
                    var toucheePlaylists = Spotify.Media.Playlist.All<Spotify.Media.Playlist>();

                    foreach (var spotifyPlaylist in spotifyPlaylists) {
                        var altID = spotifyPlaylist.GetLink().ToString();
                        spotifyAltIDs.Add(altID);

                        var toucheePlaylist = Spotify.Media.Playlist.ExistsByAltID(altID) ? (Spotify.Media.Playlist)Spotify.Media.Playlist.FindByAltID(altID) : null;
                        if (toucheePlaylist == null) {
                            toucheePlaylist = spotifyPlaylist == _session.Starred
                                ? new StarredPlaylist(spotifyPlaylist, Touchee.Medium.Local)
                                : new Spotify.Media.Playlist(spotifyPlaylist, Touchee.Medium.Local);
                            toucheePlaylist.Save();
                            Log("Created: " + toucheePlaylist.Name);
                        }
                        else {
                            Log("Updated: " + toucheePlaylist.Name);
                        }

                    }

                    var removedPlaylists = toucheePlaylists.Where((p, i) => !spotifyAltIDs.Contains(p.AltId));
                    lock (removedPlaylists) {
                        foreach (var playlist in removedPlaylists) {
                            Log("Dispose: " + playlist.Name);
                            playlist.Dispose();
                        }
                    }

                }
            }
        }


        /// <summary>
        /// Called when the state of a playlist changes.
        /// This method checks if the playlist is completely loaded and then updates or adds the playlist.
        /// </summary>
        Spotify.Media.Playlist CreateOrUpdatePlaylist(SpotiFire.Playlist playlist) {
            Spotify.Media.Playlist toucheePlaylist = null;

            lock (playlist) {
                // Only continue if the playlist is completely loaded, including all tracks
                if (playlist.IsLoaded && playlist.AllTracksLoaded()) {

                    // Get the alt ID
                    var altID = playlist.GetLink().ToString();

                    // If we already have this playlist, update it
                    if (Spotify.Media.Playlist.ExistsByAltID(altID)) {
                        toucheePlaylist = (Spotify.Media.Playlist)Spotify.Media.Playlist.FindByAltID(altID);
                        Log("Updated");
                    }

                    // Else, add as new playlist and listen to track events
                    else {

                        toucheePlaylist = playlist == _session.Starred
                            ? new StarredPlaylist(playlist, Touchee.Medium.Local)
                            : new Spotify.Media.Playlist(playlist, Touchee.Medium.Local);
                        toucheePlaylist.Save();
                        Log("Created");
                        //await this.AddTracks(sender, sender.Tracks);
                        //sender.Tracks.CollectionChanged += Tracks_CollectionChanged;
                    }

                }
                else
                    Log("Not loaded");
            }

            return toucheePlaylist;
        }





        #region Track handling


        async void Tracks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            return;
            if (e.NewItems != null)
                await this.AddTracks((SpotiFire.Playlist)sender, e.NewItems.Cast<SpotiFire.Track>().ToList());
            if (e.OldItems != null)
                this.RemoveTracks((SpotiFire.Playlist)sender, e.OldItems.Cast<SpotiFire.Track>().ToList());
        }

        async Task AddTracks(SpotiFire.Playlist playlist, IList<SpotiFire.Track> tracks) {
            foreach (var track in tracks) {
                await track;
                var altID = track.GetLink().ToString();
                if (!Spotify.Media.SpotifyTrack.ExistsByAltID(altID)) {
                    var toucheeTrack = new Spotify.Media.SpotifyTrack(track);
                    toucheeTrack.Save();
                    _masterPlaylist.Add(toucheeTrack);
                }
            }
        }

        void RemoveTracks(SpotiFire.Playlist playlist, IList<Track> tracks) {
        }


        #endregion


    }

}
