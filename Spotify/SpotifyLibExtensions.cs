using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Spotify {

    public static class SpotiFireExtension {

        public static string FirstArtist(this SpotiFire.Track track) {
            return track.Artists.Count > 0 ? track.Artists.First().Name : null;
        }

        public static bool IsAlbum(this SpotiFire.Playlist playlist) {
            if (playlist.Tracks.Count() <= 1) return true;
            var firstAlbumObj = playlist.Tracks.First().Album;
            var firstAlbum = firstAlbumObj.Name;
            var firstArtist = firstAlbumObj.Artist.Name;

            return !playlist.Tracks.Any(t => t.Album.Artist.Name != firstArtist || t.Album.Name != firstAlbum);
        }

    }

}
