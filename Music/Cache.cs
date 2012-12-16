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
            this.Directories = Plugin.Watcher.CollectedLocalDirectories.Select(d => d.FullName).ToList();
            this.Playlists = Playlist.Where(p => p.Medium == Medium.Local).Cast<Playlist>().ToList();
            this.Tracks = this.Playlists.SelectMany(p => p.Tracks).Cast<Track>().ToList();
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
