using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

using SpotiFire;

namespace Spotify {

    public static class SpotiFireExtension {


        /// <summary>
        /// Returns the name of the first artist of the track
        /// </summary>
        /// <param name="track">The track to find the artist of</param>
        /// <returns>The name of the first artist</returns>
        public static string FirstArtist(this Track track) {
            return track.Artists.Count > 0 ? track.Artists.First().Name : null;
        }


        /// <summary>
        /// Checks if the given playlist actually is an album. A playlist is said to be an album when
        /// all tracks are from the same album.
        /// </summary>
        /// <param name="playlist">The playlist to check</param>
        /// <returns>True if all tracks are from the same album, otherwise false</returns>
        public static bool IsAlbum(this Playlist playlist) {
            if (playlist.Tracks.Count() <= 1) return true;
            var album = playlist.Tracks.First().Album;
            return !playlist.Tracks.Any(t => t.Album != album);
        }


        /// <summary>
        /// Checks whether all tracks in the given playlist are loaded.
        /// </summary>
        /// <param name="playlist">The playlist to check</param>
        /// <returns>True if all tracks in the playlist are loaded. Otherwise false.</returns>
        public static bool AllTracksLoaded(this Playlist playlist) {
            var tracks = playlist.Tracks;
            lock (tracks) {
                return tracks.All(t => t.IsLoaded);
            }
        }


        public static bool AllTracksReady(this Playlist playlist) {
            var tracks = playlist.Tracks;
            lock (tracks) {
                return tracks.All(t => t.IsReady);
            }
        }
        public static bool IsFullyReady(this Playlist playlist) {
            return playlist.IsLoaded && playlist.AllTracksReady();
        }


        /// <summary>
        /// Checks whether the playlist is fully loaded. That is, if the playlist and
        /// all the containing tracks are loaded.
        /// </summary>
        /// <param name="playlist">The playlist to check</param>
        /// <returns>True if the playlist and all tracks in the playlist are loaded and the playlist does not have any pending changes. Otherwise false.</returns>
        public static bool IsFullyLoaded(this Playlist playlist) {
            return playlist.IsLoaded && !playlist.HasPendingChanges && playlist.AllTracksLoaded();
        }




        //public static bool IsDifferentFrom(this Playlist playlist, Spotify.Media.Playlist toucheePlaylist) {

        //}

        class PlaylistHandlerStatus {
            public bool UpdateInProgress;
            public bool TrackingTracks;
        }

        static Dictionary<SpotiFire.Playlist, PlaylistHandlerStatus> _playlistStatus = new Dictionary<SpotiFire.Playlist, PlaylistHandlerStatus>();

        public static bool IsKnown(this SpotiFire.Playlist playlist) {
            return _playlistStatus.ContainsKey(playlist);
        }
        public static void SetKnown(this SpotiFire.Playlist playlist, bool known) {
            if (known) {
                if (!playlist.IsKnown())
                    _playlistStatus[playlist] = new PlaylistHandlerStatus();
            }
            else
                _playlistStatus.Remove(playlist);
        }

        public static bool IsUpdateInProgress(this SpotiFire.Playlist playlist) {
            return playlist.IsKnown() && _playlistStatus[playlist].UpdateInProgress;
        }
        public static void SetUpdateInProgress(this SpotiFire.Playlist playlist, bool inProgress) {
            _playlistStatus[playlist].UpdateInProgress = inProgress;
        }

        public static bool IsTrackingTracks(this SpotiFire.Playlist playlist) {
            return playlist.IsKnown() && _playlistStatus[playlist].TrackingTracks;
        }
        public static void SetTrackingTracks(this SpotiFire.Playlist playlist, bool tracking) {
            _playlistStatus[playlist].TrackingTracks = tracking;
        }


    }

}
