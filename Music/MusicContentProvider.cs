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

            string view = filter.ContainsKey("view") ? filter["view"] : null;
            switch (view) {
                
                // All tracks for the given artist or genre
                case "artist":
                case "genre":
                    string group = filter[view];
                    if (group != null) group = group.ToLower();
                    switch (view) {
                        case "artist": ret = allTracks.Where(t => t.IsByArtist(group)); break;
                        case "genre": ret = allTracks.Where(t => t.IsOfGenre(group)); break;
                    }
                    ret = ret
                        .OrderByOrdinal(t => t.AlbumArtistSort)
                        .ThenByOrdinal(t => t.AlbumSort);
                    break;

                // All tracks for the given album
                case "album":
                    ret = Track.Where(t => t.AlbumID == filter["album"]);
                    break;

                // All tracks
                case "track":
                default:
                    ret = allTracks
                        .OrderByOrdinal(t => t.TitleSort)
                        .ThenByOrdinal(t => t.ArtistSort);
                    break;

            }

            // Order by disc number, track number and track name
            // TODO: do not sort non-masterplaylist
            if (view != "track") {

                if (ret is IOrderedEnumerable<ITrack>)
                    ret = ((IOrderedEnumerable<ITrack>)ret).ThenBy(t => t.DiscNumber == 0);
                else
                    ret = ret.OrderBy(t => t.DiscNumber == 0);

                ret = ((IOrderedEnumerable<ITrack>)ret)
                    .ThenBy(t => t.DiscNumber == 0)
                    .ThenBy(t => t.DiscNumber)
                    .ThenBy(t => t.TrackNumber == 0)
                    .ThenBy(t => t.TrackNumber)
                    .ThenByOrdinal(t => t.TitleSort);
            }

            return ret;
        }

        #endregion


    }

}
