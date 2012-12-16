using System.Collections.Generic;

namespace Touchee.Server.Responses {

    public class ContainersResponse : ToucheeResponse {

        public int MediumID { get; protected set; }
        public readonly IEnumerable<Container> Items;

        public ContainersResponse(Medium medium) {
            this.MediumID = medium.Id;
            this.Items = medium.Containers;
        }

    }

}
