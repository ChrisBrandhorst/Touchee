using System;
//using System.Collections;
//using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Touchee;
using Touchee.Components;
using Touchee.Components.Media;

using Spotify.Media;

namespace Spotify {


    /// <summary>
    /// The viewtypes used
    /// </summary>
    public static class ViewTypes {
        public const string Track = "track";
        public const string Artist = "artist";
        public const string Album = "album";
        public const string Genre = "genre";
    }



    /// <remarks>
    /// Spotify plugin
    /// </remarks>
    public class Plugin : Base, IPlugin {


        #region Statics

        internal static Watcher Watcher { get; private set; }
        internal static ContentProvider ContentProvider { get; private set; }

        #endregion



        #region Privates

        SessionHandler _sessionHandler;
        ContentsHandler _contentsHandler;
        
        string _username;
        string _password;
        byte[] _key;

        #endregion



        #region IPlugin implementation

        /// <summary>
        /// The name of this plugin
        /// </summary>
        public string Name { get { return "Spotify"; } }


        /// <summary>
        /// The description of this plugin
        /// </summary>
        public string Description { get { return "Plugin providing music from Spotify."; } }


        /// <summary>
        /// The version of this plugin
        /// </summary>
        public Version Version { get { return new Version(1, 0, 0, 0); } }


        /// <summary>
        /// Whether this plugin provides some front-end functionality
        /// </summary>
        public bool ProvidesFrontend { get { return true; } }


        /// <summary>
        /// Starts the plugin.
        /// </summary>
        /// <param name="config">The configuration object for this plugin</param>
        /// <param name="context">The context for this plugin</param>
        /// <returns>True if the plugin was successfully started</returns>
        public bool StartPlugin(dynamic config, IPluginContext context) {
            
            // Get params
            // TODO: get un/pw from user input instead of config file
            try {
                _username = config["username"];
                _password = config["password"];
                string keyString = config["key"];
                _key = keyString.Trim(new char[] { ' ', '\n' }).Split(' ').Select(c => Convert.ToByte(c, 16)).ToArray();
            }
            catch (Exception) {
                Log("No or invalid config values for username, password, key", Logger.LogLevel.Error);
            }

            // Add content provider
            ContentProvider = new ContentProvider();
            PluginManager.Register(ContentProvider);

            // Start the watcher
            Watcher = new Watcher();
            PluginManager.Register(Watcher);

            // Bind to watching event
            Watcher.StartedWatching += StartedWatching;


            return true;
        }


        /// <summary>
        /// Called when the local medium has arrived.
        /// </summary>
        void StartedWatching(IMediumWatcher watcher, Medium medium) {

            // Start session handler
            _sessionHandler = new SessionHandler();
            var session = _sessionHandler.Init(_username, _password, _key).Result;

            // Start contents handler
            _contentsHandler = new ContentsHandler(session);

        }


        /// <summary>
        /// Shuts down the plugin.
        /// </summary>
        /// <returns>True</returns>
        public bool StopPlugin() {

            // Stop the watcher(s)
            Watcher.UnWatchAll();

            // Unregister plugin
            PluginManager.Unregister(Watcher);
            PluginManager.Unregister(ContentProvider);
            PluginManager.Unregister(this);

            return true;
        }

        #endregion




    }

}
