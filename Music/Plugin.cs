using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Touchee;
using Touchee.Components;

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


        internal static IEnumerable<string> TrackExtensions { get; private set; }
        internal static IEnumerable<string> PlaylistExtensions { get; private set; }
        internal static IEnumerable<string> Extensions { get; private set; }
        internal static MusicFileMediumWatcher Watcher { get; private set; }
        internal static MusicContentProvider ContentProvider { get; private set; }
        internal static MusicArtworkProvider ArtworkProvider { get; private set; }


        #endregion



        #region IPlugin implementation

        /// <summary>
        /// The name of this plugin
        /// </summary>
        public string Name { get { return "Local music"; } }


        /// <summary>
        /// The description of this plugin
        /// </summary>
        public string Description { get { return "Plugin providing music stored locally and on removable drives."; } }


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

            // Get folders
            string[] folders = config.GetStringArray("folders");

            // Get extensions
            TrackExtensions = config.GetStringArray("extensions.tracks");
            PlaylistExtensions = config.GetStringArray("extensions.playlists");
            Extensions = TrackExtensions.Concat(PlaylistExtensions).ToArray();

            // Create the watcher and add folders to it
            Watcher = new MusicFileMediumWatcher();
            foreach(var f in folders)
                Watcher.AddLocalFolder(f);
            PluginManager.Register(Watcher);

            // Add content provider
            ContentProvider = new MusicContentProvider();
            PluginManager.Register(ContentProvider);

            // Add artwork provider
            ArtworkProvider = new MusicArtworkProvider();
            PluginManager.Register(ArtworkProvider);

            return true;
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
