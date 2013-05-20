using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Fleck;
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
        /// Finds the Client with the given connection.
        /// </summary>
        /// <param name="connection">The connection to match.</param>
        /// <returns>The first Client found with the same socket connection as the argument, or null if it does not exist</returns>
        public static Client FindByWebSocketConnection(IWebSocketConnection connection) {
            return All().FirstOrDefault(c => c._connection == connection);
        }

        /// <summary>
        /// Finds the Client with the given sessionID.
        /// </summary>
        /// <param name="sessionId">The sessionID to match.</param>
        /// <returns>The first Client found with the same socket connection as the argument, or null if it does not exist</returns>
        public static Client FindBySessionID(string sessionId) {
            return All().FirstOrDefault(c => c.SessionId == sessionId);
        }

        /// <summary>
        /// Finds the Clients that are connected from the given IP address
        /// </summary>
        /// <param name="ipAddress">The IP address to match</param>
        /// <returns>Array containing the matched Client objects</returns>
        public static Client[] FindByIPAddress(System.Net.IPAddress ipAddress) {
            return FindByIPAddress(ipAddress.ToString());
        }

        /// <summary>
        /// Finds the Clients that are connected from the given IP address
        /// </summary>
        /// <param name="ipAddress">The IP address to match</param>
        /// <returns>Array containing the matched Client objects</returns>
        public static Client[] FindByIPAddress(string ipAddress) {
            return All().Where(c => c._connection.ConnectionInfo.ClientIpAddress == ipAddress).ToArray();
        }

        #endregion


        // The connection this client is connected with
        IWebSocketConnection _connection;
        
        // The HTTP session ID of this client
        public string SessionId { get; internal set; }

        /// <summary>
        /// Instantiates a new Client object
        /// </summary>
        /// <param name="connection">The websocket connection for the client</param>
        public Client(IWebSocketConnection connection) {
            _connection = connection;
        }

        /// <summary>
        /// Send a message over the websocket connection
        /// </summary>
        /// <param name="message">The message to send</param>
        public void Send(string message) {
            _connection.Send(message);
        }

    }
}
