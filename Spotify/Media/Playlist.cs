using System;
using System.Collections.Generic;
using System.Linq;

using Touchee;
using Touchee.Media.Music;

namespace Spotify.Media {

    /// <remarks>
    /// 
    /// </remarks>
    public class Playlist : Music.Media.Playlist {

        
        #region Constructor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="medium"></param>
        /// <param name="spPlaylist"></param>
        public Playlist(SpotiFire.Playlist spPlaylist, Medium medium) : base(spPlaylist.Name, medium) {
            
            var link = spPlaylist.GetLink();
            this.AltId = link.ToString();
            link.Dispose();
            
            this.Master = medium.MasterContainer;
        }


        #endregion

    }





    public class StarredPlaylist : Playlist {
        public StarredPlaylist(SpotiFire.Playlist spPlaylist, Medium medium) : base(spPlaylist, medium) { }
        public override string Type { get { return "starred_playlist"; } }
        public override string Name { get { return "Starred"; } }
    }







}
