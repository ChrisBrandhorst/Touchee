using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.IO;

using Touchee.Server;
using Touchee.Server.Responses;
using Touchee.Plugins;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Touchee.Server {

    /// <remarks>
    /// The master server
    /// </remarks>
    public class ToucheeServer : Base {


        #region Privates

        Server.Http.HttpServer _httpServer;
        Server.Websocket.WebsocketServer _websocketServer;
        Nancy.ISerializer _serializer;
        int _httpServerPort;

        #endregion



        #region Properties


        /// <summary>
        /// The port of the websocket of this server
        /// </summary>
        public int WebsocketPort { get; protected set; }


        /// <summary>
        /// The server info
        /// </summary>
        public ServerInfoResponse ServerInfo { get { return new ServerInfoResponse(this); } }


        #endregion


        /// <summary>
        /// Initialises a new server instance
        /// </summary>
        /// <param name="httpServerPort">The port at which the HTTP server should run</param>
        /// <param name="websocketPort">The port at which the websocket server should run</param>
        public ToucheeServer(int httpServerPort = 80, int websocketPort = 81) {

            // Set local parameters
            _httpServerPort = httpServerPort;
            this.WebsocketPort = websocketPort;

            // Init serializer
            _serializer = new Http.JsonNetSerializer();

            // Init HTTP server
            _httpServer = new Server.Http.HttpServer(httpServerPort);

            // Init websocket server
            _websocketServer = new Server.Websocket.WebsocketServer(websocketPort);
        }


        /// <summary>
        /// Start the server by starting the HTTP server, the websocket server and the media detector.
        /// </summary>
        /// <returns>true when the server was successfully started</returns>
        public bool Start() {

            // Start HTTP server
            try {
                Log("Starting HTTP server on port " + _httpServerPort.ToString());
                _httpServer.Start();
                Log("HTTP server started");
            }
            catch (Exception e) {
                Log("Could not start HTTP server", e, Logger.LogLevel.Fatal);
                return false;
            }

            // Start websocket server
            try {
                Log("Starting websocket server on port " + this.WebsocketPort.ToString());
                _websocketServer.Start();
                Log("Websocket server started");
            }
            catch (Exception e)
            {
                Log("Could not start websocket server", e, Logger.LogLevel.Fatal);
                return false;
            }

            return true;
        }


        #region Messages

        /// <summary>
        /// Converts the given message to a JSON representation
        /// </summary>
        /// <param name="response">The response to be serialized</param>
        /// <returns>The message in JSON form</returns>
        public string Serialize(ToucheeResponse response) {
            var dict = new Dictionary<string, object>();
            var key = Regex.Replace(response.GetType().Name.FirstToLower(), "Response$", "");
            dict[key] = response;

            string serialized;
            using (var stream = new MemoryStream()) {
                _serializer.Serialize("application/json", dict, stream);
                stream.Position = 0;
                serialized = new StreamReader(stream).ReadToEnd();
            }

            return serialized;
        }



        #endregion


        #region Communication


        /// <summary>
        /// Sends a message to a client as JSON over the websocket
        /// </summary>
        /// <param name="client">The client to send the message to</param>
        /// <param name="message">The response to send</param>
        public void Send(Client client, ToucheeResponse response) {
            client.Send(Serialize(response));
        }


        /// <summary>
        /// Sends a message to all clients as JSON over the websocket
        /// </summary>
        /// <param name="response">The response to send</param>
        public void Broadcast(ToucheeResponse response) {
            var serialized = Serialize(response);
            Client.ForEach(c => c.Send(serialized));
        }


        #endregion


    }


    /// <remarks>
    /// Custom JSON serializer used for HTML Encoding strings.
    /// </remarks>
    internal class ToucheeJsonConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return typeof(string).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            return HttpUtility.HtmlDecode(existingValue.ToString());
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            writer.WriteValue( HttpUtility.HtmlEncode(value.ToString()) );
        }
    }

}
