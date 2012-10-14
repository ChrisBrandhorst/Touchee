using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Text.RegularExpressions;

namespace Touchee.Server.Websocket {

    /// <remarks>
    /// A websocket server
    /// </remarks>
    public class WebsocketServer : Touchee.Base {


        // Private vars
        Fleck.WebSocketServer _server;
        int _port;
        
        /// <summary>
        /// Instantiates a websocket server
        /// </summary>
        /// <param name="port">The port</param>
        public WebsocketServer(int port) {
            _port = port;
            _server = new Fleck.WebSocketServer("ws://localhost:" + port.ToString());
        }


        /// <summary>
        /// Starts the websocket server. Generates a new Client object for each opened socket
        /// and disposes that Client when the connection is closed.
        /// </summary>
        public void Start() {
            _server.Start(socket => {
                socket.OnOpen = () => {
                    new Client(socket).Save();
                    Log("Client connected: " + socket.ConnectionInfo.ClientIpAddress.ToString());
                };
                socket.OnClose = () => {
                    var client = Client.FindByWebSocketConnection(socket);
                    if (client != null) {
                        client.Dispose();
                        Log("Client disconnected: " + socket.ConnectionInfo.ClientIpAddress.ToString());
                    }
                };
                socket.OnMessage = (message) => {
                    var client = Client.FindByWebSocketConnection(socket);
                    if (client != null)
                        this.OnMessage(client, message);
                };
            });
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        void OnMessage(Client client, string message) {

            // Split message
            var match = Regex.Match(message, @"([A-Z_]+)\s(.*)");
            
            // See if message is valid
            if (match.Groups.Count < 3) {
                Log("Invalid message syntax: '" + message + "'", Logger.LogLevel.Error);
                return;
            }

            // Get parts
            var action = match.Groups[1].Value.ToLower().ToCamelCase();
            var args = match.Groups[2].Value;

            // Fire action
            var method = this.GetType().GetMethod(action, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (method != null)
                method.Invoke(this, new object[]{client, args});
            else
                Log("Unknown action: " + action, Logger.LogLevel.Error);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="args"></param>
        void Identify(Client client, string args) {
            client.sessionId = args;
        }





        class Actions {
            internal const string Identify = "IDENTIFY";
        }


    }

}
