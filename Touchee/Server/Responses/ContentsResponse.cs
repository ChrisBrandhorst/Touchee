namespace Touchee.Server.Responses {

    public class ContentsResponse : ToucheeResponse {

        public int ContainerID { get; protected set; }
        public string[] Keys { get; protected set; }
        public object Data { get; protected set; }
        public object Meta { get; protected set; }
        public string Plugin { get; protected set; }

        public ContentsResponse(Container container, Contents contents) {
            this.ContainerID = container.Id;
            this.Keys = contents.Keys;
            this.Data = contents.Data;
            this.Meta = contents.Meta;
            this.Plugin = container.GetType().Assembly.GetName().Name;
        }

    }

}
