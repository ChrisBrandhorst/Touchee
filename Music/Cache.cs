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

        public override void BeforeSerialize() {
            this.Tracks = Track.All().ToList();
        }

        public override void AfterDeserialize() {
            Track.Clear();
            foreach (var item in Tracks)
                item.Save();
        }

    }

}
