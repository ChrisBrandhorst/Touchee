//using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

using Touchee;
using SpotiFire;

namespace Spotify {
    
    public class ContentsHandler : Base {



        Session _session;
        Medium _medium;


        public ContentsHandler(Session session, Medium medium) {
            _session = session;
            _medium = medium;

            _session.ConnectionstateUpdated += ConnectionstateUpdated;
        }


        async void ConnectionstateUpdated(Session sender, SessionEventArgs e) {

            if (sender.ConnectionState == ConnectionState.LoggedIn) {
                await _session.PlaylistContainer;

                // Add initial set of playlists
                this.AddPlaylists(_session.PlaylistContainer.Playlists);

                // Remove and then add playlist container callback
                _session.PlaylistContainer.Playlists.CollectionChanged -= Playlists_CollectionChanged;
                _session.PlaylistContainer.Playlists.CollectionChanged += Playlists_CollectionChanged;
            }
        }





        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Playlists_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (e.NewItems != null)
                this.AddPlaylists(e.NewItems.Cast<Playlist>().ToList());
            if (e.OldItems != null)
                this.RemovePlaylists(e.OldItems.Cast<Playlist>().ToList());
        }

        void AddPlaylists(IList<Playlist> playlists) {
            foreach (var playlist in playlists) {
                this.playlist_StateChanged(playlist, new PlaylistEventArgs());

                playlist.MetadataUpdated += playlist_StateChanged;
                playlist.StateChanged += playlist_StateChanged;
                //playlist.
            }
        }


        void RemovePlaylists(IList<Playlist> playlists) {
        }


        List<Playlist> _loaded = new List<Playlist>();

        void playlist_StateChanged(Playlist sender, PlaylistEventArgs e) {
            if (!_loaded.Contains(sender) && sender.IsLoaded && sender.AllTracksLoaded()) {
                _loaded.Add(sender);

                var pl = new Spotify.Media.Playlist(sender, _medium);
                Log(pl.Name + " " + pl.AltId.ToString());
                pl.Save();

            }
        }


        //void LoadedPlaylist(Playlist playlist) {
        //    _loaded.Add(playlist);
        //    this.AddTracks(playlist, playlist.Tracks);
        //    playlist.Tracks.CollectionChanged += Tracks_CollectionChanged;
        //}


        //void Tracks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
        //    if (e.NewItems != null)
        //        this.AddTracks((Playlist)sender, e.NewItems.Cast<Track>().ToList());
        //    if (e.OldItems != null)
        //        this.RemoveTracks((Playlist)sender, e.OldItems.Cast<Track>().ToList());
        //}
        
        //void AddTracks(Playlist playlist, IList<Track> tracks) {
        //    foreach (var track in tracks) {
        //    }

        //}

        //void RemoveTracks(Playlist playlist, IList<Track> tracks) {
        //}


    }

}
