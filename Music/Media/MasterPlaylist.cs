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
        public MasterPlaylist(Medium medium) : base(medium, medium.Name) {

            FileTrack.AfterSave += (s, e) => this.NotifyContentChanged();
            FileTrack.AfterDispose += (s, e) => this.NotifyContentChanged();
            
        }

        /// <summary>
        /// The type of the container, e.g. what the container 'looks like'
        /// </summary>
        public override string Type { get { return "master_playlist"; } }

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

        /// <summary>
        /// The tracks of this playlist
        /// </summary>
        public override IEnumerable<ITrack> Tracks { get { return FileTrack.All().Cast<ITrack>(); } }

        /// <summary>
        /// Invalid method
        /// </summary>
        public override void Add(Touchee.Media.Music.ITrack track) {
            throw new InvalidOperationException("Cannot add a track to the master playlist. This playlist is managed automatically.");
        }

        /// <summary>
        /// Invalid method
        /// </summary>
        public override void Add(Touchee.Media.Music.ITrack track, uint index) {
            throw new InvalidOperationException("Cannot add a track to the master playlist. This playlist ismanaged automatically.");
        }

        /// <summary>
        /// Invalid method
        /// </summary>
        public override bool Remove(Touchee.Media.Music.ITrack track) {
            throw new InvalidOperationException("Cannot remove a track from the master playlist. This playlist ismanaged automatically.");
        }

    }
}
