//using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using Touchee;
using Touchee.Components.Content;
using Music.Media;

namespace Music {
    
    /// <summary>
    /// Component for getting artwork for music items
    /// </summary>
    public class MusicArtworkProvider : IArtworkProvider {

        public Image GetArtwork(IContainer container, Options filter) {

            // For now, wo only get images based on ID
            if (!filter.ContainsKey("id")) return null;

            // Get the track by ID;
            int id = filter["id"];
            if (!Track.Exists(id)) return null;
            var track = Track.Find(id);

            // Get all tracks of the album
            var tracks = Track.GetAlbum(track);

            // Get the artwork from one of the tracks of the album
            Image artwork = null;
            tracks.FirstOrDefault(t => (artwork = track.Artwork) != null);

            return artwork;
        }

    }

}
