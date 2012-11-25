//using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

using Touchee;

namespace Music.Media {
    
    public class MasterPlaylist : Playlist {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="medium"></param>
        public MasterPlaylist(Medium medium) : base(medium) {
        }

        /// <summary>
        /// The order number to be used for sorting the containers in the frontend.
        /// If this value is -1, the container is sorted by its name.
        /// </summary>
        public override int Order { get { return 0; } }

        /// <summary>
        /// The type of the container, e.g. what the container 'looks like'
        /// </summary>
        public override string Type { get { return ContainerType.Master; } }

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
