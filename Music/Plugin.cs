using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Touchee;
using Touchee.Plugins;

namespace Music {


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
    /// Local music plugin
    /// </remarks>
    public class Plugin : Base, IPlugin {


        #region Statics


        public static IEnumerable<string> TrackExtensions { get; protected set; }
        public static IEnumerable<string> PlaylistExtensions { get; protected set; }
        public static IEnumerable<string> Extensions { get; protected set; }
        public static Watcher Watcher = Watcher.Instance;


        #endregion



        #region IPlugin implementation

        /// <summary>
        /// The name of this plugin
        /// </summary>
        public string Name { get { return "Local music"; } }


        /// <summary>
        /// The description of this plugin
        /// </summary>
        public string Description { get { return "Content provider for music stored locally."; } }


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

            // Get folders
            string[] folders = config.GetStringArray("folders");

            // Get extensions
            TrackExtensions = config.GetStringArray("extensions.tracks");
            PlaylistExtensions = config.GetStringArray("extensions.playlists");
            Extensions = TrackExtensions.Concat(PlaylistExtensions).ToArray();

            // Create the watcher and add folders to it
            Watcher.Init();
            foreach(var f in folders)
                Watcher.AddLocalFolder(f);
            PluginManager.Register(Watcher);

            return true;

        }


        /// <summary>
        /// Shuts down the plugin.
        /// </summary>
        /// <returns>True</returns>
        public bool StopPlugin() {

            // Stop the watcher(s)
            Watcher.UnWatchAll();

            // Clear data
            Media.Track.Clear();
            Media.Playlist.Clear();

            // Unregister plugin
            PluginManager.Unregister(Watcher);
            return true;
        }

        #endregion


    }

}
