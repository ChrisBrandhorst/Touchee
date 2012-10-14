using System.Collections.Generic;

namespace Touchee.Server.Responses {

    public class MediaResponse : ToucheeResponse {
        
        public readonly List<MediaItem> Items = new List<MediaItem>();

        public void Add(Medium medium) {
            Items.Add(new MediaItem(medium));
        }

        public class MediaItem {
            public int Id { get; protected set; }
            public string Name { get; protected set; }
            public string Type { get; protected set; }
            public MediaItem(Medium medium) {
                this.Id = medium.ID;
                this.Name = medium.Name;
                this.Type = medium.Type.ToString();
            }
        }

    }

}
