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



        #endregion



        #region Constructor


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="session">The Spotify session to handle</param>
        public ContentsHandler(Session session) {
            _session = session;
            _session.ConnectionstateUpdated += ConnectionstateUpdated;
            _masterPlaylist = (Music.Media.MasterPlaylist)Touchee.Medium.Local.Containers.First(c => c is Music.Media.MasterPlaylist);
        }


        #endregion



        #region Playlist handling


        /// <summary>
        /// Called when the connection state of the session is updated.
        /// If the session is logged in, all present playlists are added and
        /// listeners are attached to the playlist collection list.
        /// </summary>
        async void ConnectionstateUpdated(Session sender, SessionEventArgs e) {

            switch (sender.ConnectionState) {

                // Logged out
                case ConnectionState.LoggedOut:
                    MasterPlaylist.Instance.Dispose();
                    break;

                // Offline state (when logged in, but not completely yet)
                case ConnectionState.Offline:
                    break;

                // User has logged in
                case ConnectionState.LoggedIn:
                    MasterPlaylist.Instance.Save();
                    break;

                // User has been disconnected after having been logged in
                case ConnectionState.Disconnected:
                    break;

                // No clue
                case ConnectionState.Undefined:
                    break;

            }

            return;
            if (sender.ConnectionState == ConnectionState.LoggedIn) {
                await _session.PlaylistContainer;
                
                // Track initial set of playlists
                this.TrackPlaylists(_session.PlaylistContainer.Playlists);
                
                // Track starred playlist
                this.TrackPlaylist(_session.Starred);

                // Remove and then add playlist container callback
                _session.PlaylistContainer.Playlists.CollectionChanged -= Playlists_CollectionChanged;
                _session.PlaylistContainer.Playlists.CollectionChanged += Playlists_CollectionChanged;

                // Remove callback
                _session.ConnectionstateUpdated -= ConnectionstateUpdated;
            }

        }



        /// <summary>
        /// Called when one or more playlists are removed from Spotify.
        /// The Touchee representations of these playlists are then added / removed.
        /// </summary>
        void Playlists_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (e.NewItems != null)
                this.TrackPlaylists(e.NewItems.Cast<SpotiFire.Playlist>().ToList());
            if (e.OldItems != null)
                this.RemovePlaylists(e.OldItems.Cast<SpotiFire.Playlist>().ToList());
        }



        /// <summary>
        /// Tracks the given playlist.
        /// When the state or track meta data of the given playlist is changed,
        /// the playlist is presented to the StateChanged method for further processing.
        /// </summary>
        /// <param name="playlist">The playlist to track</param>
        void TrackPlaylist(SpotiFire.Playlist playlist) {
            this.playlist_StateChanged(playlist, new PlaylistEventArgs());
            playlist.MetadataUpdated += playlist_StateChanged;
            playlist.StateChanged += playlist_StateChanged;
        }


        /// <summary>
        /// Tracks the given set of playlists.
        /// When the state or track meta data of the given playlists is changed,
        /// the playlist is presented to the StateChanged method for further processing.
        /// </summary>
        /// <param name="playlists">The set of playlists to track</param>
        void TrackPlaylists(IList<SpotiFire.Playlist> playlists) {
            foreach (var playlist in playlists)
                this.TrackPlaylist(playlist);
        }


        /// <summary>
        /// Removes the set of playlists
        /// </summary>
        /// <param name="playlists">The set of playlists to remove</param>
        void RemovePlaylists(IList<SpotiFire.Playlist> playlists) {
            foreach (var playlist in playlists) {
                var altID = playlist.ToLink().ToString();
                if (Spotify.Media.Playlist.ExistsByAltID(altID)) {
                    var toucheePlaylist = Spotify.Media.Playlist.FindByAltID(altID);
                    toucheePlaylist.Dispose();
                    playlist.Tracks.CollectionChanged -= Tracks_CollectionChanged;
                    playlist.MetadataUpdated -= playlist_StateChanged;
                    playlist.StateChanged -= playlist_StateChanged;
                }
            }
        }


        /// <summary>
        /// Called when the state of a playlist changes.
        /// This method checks if the playlist is completely loaded and then updates or adds the playlist.
        /// </summary>
        async void playlist_StateChanged(SpotiFire.Playlist sender, PlaylistEventArgs e) {

            // Only continue if the playlist is completely loaded, including all tracks
            if (sender.IsLoaded && sender.AllTracksLoaded()) {
                Spotify.Media.Playlist playlist;

                // Get the alt ID
                var altID = sender.ToLink().ToString();
                
                // If we already have this playlist, update it
                if (Spotify.Media.Playlist.ExistsByAltID(altID)) {
                    playlist = null;
                }
                
                // Else, add as new playlist and listen to track events
                else {

                    playlist = new Spotify.Media.Playlist(sender, Touchee.Medium.Local);
                    playlist.Save();
                    Log(playlist.Name + " " + altID.ToString());
                    
                    await this.AddTracks(sender, sender.Tracks);
                    sender.Tracks.CollectionChanged += Tracks_CollectionChanged;
                }

            }

        }

        #endregion



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
