using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Touchee.Server {

    /// <summary>
    /// Touchee server interface
    /// </summary>
    public interface IServer {

        /// <summary>
        /// Sends a message to a client as JSON over the websocket
        /// </summary>
        /// <param name="client">The client to send the message to</param>
        /// <param name="message">The response to send</param>
        void Send(IClient client, object response);


        /// <summary>
        /// Sends a message to all clients as JSON over the websocket
        /// </summary>
        /// <param name="response">The response to send</param>
        void Broadcast(object response);


        /// <summary>
        /// Sends an object to all clients as JSON over the websocket
        /// </summary>
        /// <param name="key">The root key</param>
        /// <param name="obj">The object to send</param>
        void Broadcast(string key, object obj);


        /// <summary>
        /// Sends a plugin-specific object to all clients as JSON over the websocket
        /// </summary>
        /// <param name="plugin">The name of the plugin</param>
        /// <param name="key">The root key</param>
        /// <param name="obj">The object to send</param>
        void Broadcast(string plugin, string key, object obj);


    }

}
