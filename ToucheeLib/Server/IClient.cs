using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Touchee.Server {

    /// <remarks>
    /// Represents a client connected to the websocket server.
    /// Makes it possible to push messages to the client.
    /// Clients can be found using a IWebSocketConnection or IP address.
    /// </remarks>
    public interface IClient {

        // The HTTP session ID of this client
        string SessionId { get; }

        /// <summary>
        /// Send a message over the websocket connection
        /// </summary>
        /// <param name="message">The message to send</param>
        void Send(string message);

    }
}
