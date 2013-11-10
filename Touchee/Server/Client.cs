using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Touchee.Media;

namespace Touchee.Server {
    
    /// <remarks>
    /// Represents a client connected to the websocket server.
    /// Makes it possible to push messages to the client.
    /// Clients can be found using a IWebSocketConnection or IP address.
    /// </remarks>
    public class Client : Collectable<Client>, IClient {


        #region Finders

        /// <summary>
        /// Finds the Client with the given sessionID.
        /// </summary>
        /// <param name="sessionId">The sessionID to match.</param>
        /// <returns>The first Client found with the same socket connection as the argument, or null if it does not exist</returns>
        public static Client FindBySessionID(string sessionId) {
            return All().FirstOrDefault(c => c.SessionId == sessionId);
        }

        #endregion


        // The HTTP session ID of this client
        public string SessionId { get; internal set; }


        /// <summary>
        /// Instantiates a new Client object
        /// </summary>
        public Client() {
        }

    }
}
