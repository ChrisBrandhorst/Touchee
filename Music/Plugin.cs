using System;
using System.Collections;
using System.Collections.Generic;


using Touchee;
using Touchee.Plugins;

namespace Music {

    /// <remarks>
    /// Local music plugin
    /// </remarks>
    public class Plugin : Base, IPlugin {


        #region Privates

        Watcher _watcher;

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
            if (folders.Length == 0) {
                Log("No or invalid folder configuration value");
                return false;
            }

            // Get extensions
            string[] extensions = config.GetStringArray("extensions");
            if (extensions.Length == 0) {
                Log("No or invalid extensions configuration value");
                return false;
            }

            _watcher = new Watcher(extensions);
            _watcher.AddFolders(folders);
            PluginManager.Register(_watcher);

            return true;

        }


        /// <summary>
        /// Shuts down the plugin.
        /// </summary>
        /// <returns>True</returns>
        public bool StopPlugin() {
            PluginManager.Unregister(_watcher);
            return true;
        }

        #endregion


    }

}
