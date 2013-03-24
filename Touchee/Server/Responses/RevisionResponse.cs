namespace Touchee.Server.Responses {

    /// <summary>
    /// Server info object
    /// </summary>
    public class RevisionResponse : ToucheeResponse {

        /// <summary>
        /// The revision number of the library
        /// </summary>
        public uint Revision { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public RevisionResponse(Library library) {
            this.Revision = library.Revision;
        }

    }

}
