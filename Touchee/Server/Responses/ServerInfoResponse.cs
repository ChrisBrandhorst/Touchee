using System;
using System.Collections;
using System.Linq;
using Touchee.Plugins;

namespace Touchee.Server.Responses {

    /// <summary>
    /// Server info object
    /// </summary>
    public class ServerInfoResponse : ToucheeResponse {

        /// <summary>
        /// The name of the server
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// The welcome message given to the user
        /// </summary>
        public string WelcomeMessage { get; protected set; }

        /// <summary>
        /// The port the websocket should connect to
        /// </summary>
        public int WebsocketPort { get; protected set; }

        /// <summary>
        /// List of devices present on the server
        /// </summary>
        public ArrayList Devices { get; protected set; }

        /// <summary>
        /// Identifiers of plugins which have a front-end component and as such
        /// should be initialised on the client side.
        /// </summary>
        public string[] Plugins { get; protected set; }

        /// <summary>
        /// The current time of th server
        /// </summary>
        public long UtcTime { get; protected set; }

        /// <summary>
        /// The UTC offset of the server
        /// </summary>
        public long UtcOffset { get; protected set; }

        /// <summary>
        /// The revision number of the library
        /// </summary>
        public uint Revision { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ServerInfoResponse(ToucheeServer server, Library library) {
            DateTime now = DateTime.Now;
            this.Name           = Medium.Local == null ? System.Environment.MachineName : Medium.Local.Name;
            this.WelcomeMessage = Program.Config.GetString("welcomeMessage", "Welcome to Touchee");
            this.WebsocketPort  = server.WebsocketPort;
            this.UtcTime        = (long)now.TimeStamp();
            this.UtcOffset      = (long)TimeZone.CurrentTimeZone.GetUtcOffset(now).TotalMinutes;
            this.Devices        = Program.Config.GetValue("devices", null);
            this.Plugins        = PluginManager.FrontendComponents.Select(p => p.GetType().Assembly.GetName().Name.ToUnderscore()).ToArray();
            this.Revision       = library.Revision;
        }

    }

}
