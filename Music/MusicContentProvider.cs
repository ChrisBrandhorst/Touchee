using System;
using System.Collections.Generic;
using System.Linq;

using Touchee;
using Touchee.Components.Content;
using Music.Media;
using Touchee.Media.Music;

namespace Music {
    
    /// <summary>
    /// 
    /// </summary>
    public class MusicContentProvider : IContentProvider {


        public MusicContentProvider() {
        }


        #region IContentProvider implementation


        /// <summary>
        /// Whether this plugin provides some custom frontend
        /// </summary>
        public bool ProvidesFrontend { get { return true; } }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public object GetContents(Container container, Options filter) {
            return this.GetItems(container, filter);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public IEnumerable<IItem> GetItems(Container container, Options filter) {
            if (!(container is Playlist)) return null;

            IEnumerable<ITrack> ret = null;
            var allTracks = ((Playlist)container).Tracks;

            string view = filter["view"];
            switch (view) {

                // All tracks
                case "track":
                    ret = allTracks
                        .OrderByOrdinal(t => t.TitleSort)
                        .ThenByOrdinal(t => t.ArtistSort);
                    break;
                
                // All tracks for the given artist or genre
                case "artist":
                case "genre":
                    string group = filter[view];
                    if (group != null) group = group.ToLower();
                    switch (view) {
                        case "artist": ret = allTracks.Where(t => t.IsByArtist(group)); break;
                        case "genre":  ret = allTracks.Where(t => t.IsOfGenre(group)); break;
                    }
                    ret = ret
                        .OrderByOrdinal(t => t.AlbumArtistSort)
                        .ThenByOrdinal(t => t.AlbumSort)
                        .ThenBy(t => t.DiscNumber)
                        .ThenBy(t => t.TrackNumber)
                        .ThenByOrdinal(t => t.TitleSort);
                    break;

                // All tracks for the given album
                case "album":
                    ret = Track.GetAlbum(Track.Find(filter["album"]));
                    break;

            }

            return ret;
        }

        #endregion


    }

}
