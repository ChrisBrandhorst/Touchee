using System.Collections.Generic;

namespace Touchee.Server.Responses {

    public class ContainersResponse : ToucheeResponse {
        public int MediumID;
        public List<Container> Containers;
        public ContainersResponse(Medium medium) {
            this.MediumID = medium.Id;
            this.Containers = new List<Container>(medium.Containers);
        }
    }

}
