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

        [DataMember]
        List<string> Directories { get; set; }

        public override void BeforeSerialize() {
            this.Tracks = Track.All().ToList();
            this.Playlists = Playlist.Where(t => t.Medium.Type == MediumType.Local).Cast<Playlist>().ToList();
            this.Directories = Plugin.Watcher.CollectedLocalDirectories.Select(d => d.FullName).ToList();
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
