using System.Collections.Generic;

namespace Touchee.Server.Responses {

    public class ContentsResponse : ToucheeResponse {

        public int ContainerID { get; protected set; }
        public object Contents { get; protected set; }

        public ContentsResponse(Container container, object contents) {
            this.ContainerID = container.Id;
            this.Contents = contents;
        }

    }

}
