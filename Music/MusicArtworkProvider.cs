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

        public Image GetArtwork(Container container, Options filter) {

            // For now, wo only get images based on album ID
            if (!filter.ContainsKey("album")) return null;

            // Get tracks by albumID
            string albumID = filter["album"];
            var tracks = Track.GetTracksOfAlbum(albumID);
            if (tracks.Count() == 0) return null;

            // Get the artwork from one of the tracks of the album
            Image artwork = null;
            tracks.FirstOrDefault(t => (artwork = t.Artwork) != null);

            return artwork;
        }

    }

}
