using System.Collections.Generic;

namespace Touchee.Server.Responses {

    public class NewContentsResponse : ToucheeResponse {

        public int ContainerID { get; protected set; }
        public object Contents { get; protected set; }
        //public string Plugin { get; protected set; }

        public NewContentsResponse(Container container, object contents) {
            this.ContainerID = container.Id;
            this.Contents = contents;
            //this.Plugin = container.GetType().Assembly.GetName().Name;
        }

    }

}
