using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Touchee;

namespace Spotify.Media {

    public class SpotifyMedium : Medium {


        #region Singleton

        /// <summary>
        /// Private constructor
        /// </summary>
        SpotifyMedium() : base("Spotify") {
            this.MasterPlaylist = new MasterPlaylist(this);
        }

        /// <summary>
        /// The singleton instance of the library
        /// </summary>
        public static SpotifyMedium Instance = new SpotifyMedium();

        #endregion





        public MasterPlaylist MasterPlaylist { get; private set; }

    }

}
