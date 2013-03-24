namespace Touchee.Server.Responses {

    /// <summary>
    /// Contents updated
    /// </summary>
    public class ContentsChangedResponse : ToucheeResponse {

        public int MediumID { get; protected set; }
        public int ContainerID { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ContentsChangedResponse(Container container) {
            this.MediumID = container.Medium.Id;
            this.ContainerID = container.Id;
        }

    }

}
