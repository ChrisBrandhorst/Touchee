using System;
//using System.Collections;
//using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Touchee;
using Touchee.Plugins;

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
        /// Starts the plugin.
        /// </summary>
        /// <param name="config">The configuration object for this plugin</param>
        /// <returns>True if the plugin was successfully started</returns>
        public bool StartPlugin(dynamic config) {

            // Get params
            // TODO: get un/pw from user input instead of config file
            string username = null, password = null;
            byte[] key = new byte[0];
            try {
                username = config["username"];
                password = config["password"];
                string keyString = config["key"];
                key = keyString.Trim(new char[] { ' ', '\n' }).Split(' ').Select(c => Convert.ToByte(c, 16)).ToArray();
            }
            catch (Exception) {
                Log("No or invalid config values for username, password, key", Logger.LogLevel.Error);
            }
            
            // Start handler
            SpotifyHandler.Instance.Init(username, password, key);


            // Create the watcher and add folders to it
            

            // Add content provider
            

            // Add artwork provider
            

            return true;
        }


        /// <summary>
        /// Shuts down the plugin.
        /// </summary>
        /// <returns>True</returns>
        public bool StopPlugin() {

            // Stop the watcher(s)
            

            // Unregister plugin
            

            return true;
        }

        #endregion




    }

}
