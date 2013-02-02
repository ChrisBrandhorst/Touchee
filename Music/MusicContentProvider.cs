using System.Collections.Generic;
using System.Linq;

using Touchee;
using Touchee.Components.Content;
using Music.Media;

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


        public IEnumerable<IItem> GetItems(IContainer container, Options filter) {
            if (container is Playlist) {
                return ((Playlist)container).Tracks;
            }
            else {
                return null;
            }
        }

        public object GetContents(IContainer container, Options filter) {
            return this.GetItems(container, filter);
        }

        #endregion


    }

}
