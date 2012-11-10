using System;
using System.IO;

using Touchee;
using Touchee.Plugins;

using Un4seen.Bass;
using Un4seen.Bass.AddOn.Mix;


namespace BassNetPlayer {

    /// <remarks>
    /// Audio player for Touchee utilizing the Bass.NET library.
    /// </remarks>
    public class Plugin : Base, IPlugin {


        /// <summary>
        /// The name of this plugin
        /// </summary>
        public string Name { get { return "BASS.NET audio player"; } }


        /// <summary>
        /// The description of this plugin
        /// </summary>
        public string Description { get { return "Audio player for Touchee using the BASS.NET library."; } }


        /// <summary>
        /// The version of this plugin
        /// </summary>
        public Version Version { get { return new Version(1, 0, 0, 0); } }


        /// <summary>
        /// The player
        /// </summary>
        Player _player;


        /// <summary>
        /// Starts the plugin.
        /// </summary>
        /// <param name="config">The configuration object for this plugin</param>
        /// <returns>Always true</returns>
        public bool StartPlugin(dynamic config) {
            var path = Path.Combine(new FileInfo(this.GetType().Assembly.Location).DirectoryName, @"lib\Bass.Net");
            Bass.LoadMe(path);
            BassMix.LoadMe(path);
            Bass.BASS_PluginLoadDirectory(Path.Combine(path, "plugins"));
            _player = new Player();
            PluginManager.Register(_player);
            return true;
        }


        /// <summary>
        /// Shuts down the plugin.
        /// </summary>
        /// <returns>True</returns>
        public bool StopPlugin() {
            PluginManager.Unregister(_player);
            return true;
        }


    }

}
