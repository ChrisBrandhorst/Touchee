using System;
using System.Collections.Generic;
using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

using Touchee;
using Touchee.Media.Music;

namespace Music.Media {
    
    public class MasterPlaylist : Playlist {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="medium"></param>
        public MasterPlaylist(Medium medium) : base(medium, medium.Name) { }

        /// <summary>
        /// The type of the container, e.g. what the container 'looks like'
        /// </summary>
        public override string Type { get { return "master_playlist"; } }

        /// <summary>
        /// Whether this container is a master container
        /// </summary>
        public override bool IsMaster { get { return true; } }

        /// <summary>
        /// String array containing names of views by which the contents can be viewed
        /// The first view should be the default one
        /// </summary>
        public override string[] ViewTypes {
            get {
                return new string[]{
                    Music.ViewTypes.Track,
                    Music.ViewTypes.Artist,
                    Music.ViewTypes.Album,
                    Music.ViewTypes.Genre
                };
            }
        }

    }
}
