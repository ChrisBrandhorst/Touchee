using System.Collections.Generic;

namespace Touchee.Server.Responses {

    public class MediaResponse : ToucheeResponse {

        public readonly IEnumerable<Medium> Items;

        public MediaResponse(IEnumerable<Medium> media) {
            this.Items = media;
        }

    }

}
