using System.Collections.Generic;

namespace Touchee.Server.Responses {

    public class ContainersResponse : ToucheeResponse {

        public int MediumID { get; protected set; }
        public readonly List<ContainersItem> Items = new List<ContainersItem>();

        public ContainersResponse(Medium medium) {
            this.MediumID = medium.ID;
        }

        public void Add(Container container) {
            Items.Add(new ContainersItem(container));
        }

        public class ContainersItem {
            public int Id { get; protected set; }
            public string Name { get; protected set; }
            public bool Loading { get; protected set; }
            public string Type { get; protected set; }
            public string ContentType { get; protected set; }
            public string Plugin { get; protected set; }
            public string[] ViewTypes { get; protected set; }
            public ContainersItem(Container container) {
                this.Id = container.ID;
                this.Name = container.Name;
                this.Loading = container.Loading;
                this.Type = container.Type.ToString();
                this.ContentType = container.ContentType.ToString();
                this.ViewTypes = container.ViewTypes;
                this.Plugin = container.GetType().Assembly.GetName().Name;
            }
        }

    }

}
