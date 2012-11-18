using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using Touchee;
using Music.Media;

namespace Music {

    [DataContract(Namespace="")]
    public class Cache : CacheObject<Cache> {

        [DataMember]
        List<Track> Tracks { get; set; }

        [DataMember]
        List<Playlist> Playlists { get; set; }

        public override void BeforeSerialize() {
            this.Tracks = Track.Where(t => t.Medium.Type == MediumType.Local).ToList();
            this.Playlists = Playlist.Where(t => t.Medium.Type == MediumType.Local).Cast<Playlist>().ToList();
        }

        public override void AfterDeserialize() {
            Track.Clear();
            foreach (var item in Tracks)
                item.Save();
            Playlist.Clear();
            foreach (var item in Playlists)
                item.Save();
        }

    }

}
