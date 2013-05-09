using System.Collections.Generic;

namespace Touchee.Server.Responses {

    public class MediaResponse : List<Medium> {

        public MediaResponse() {
            this.AddRange(Medium.All());
        }

    }

}
