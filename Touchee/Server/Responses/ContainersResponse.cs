using System.Collections.Generic;

namespace Touchee.Server.Responses {

    public class ContainersResponse : List<Container> {
        public ContainersResponse(Medium medium) {
            this.AddRange(medium.Containers);
        }
    }

}
