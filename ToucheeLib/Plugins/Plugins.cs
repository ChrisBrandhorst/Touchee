using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using System.Reflection;

namespace Touchee {

    public static class Plugins {


        /// <summary>
        /// The internal list of plugins
        /// </summary>
        static List<IPlugin> _plugins = new List<IPlugin>();


        /// <summary>
        /// Internal list of contents plugins that have  a custom frontend
        /// </summary>
        public readonly static List<IContentsPlugin> CustomFrontendPlugins = new List<IContentsPlugin>();


        /// <summary>
        /// Adds the given plugin to the plugins collection
        /// </summary>
        /// <param name="plugin">Plugin to add</param>
        public static void Add(IPlugin plugin) {
            _plugins.Add(plugin);
            if (plugin is IContentsPlugin) {
                var dir = new FileInfo(plugin.GetType().Assembly.Location).DirectoryName;
                if (File.Exists(Path.Combine(dir, "web", "plugin.js")))
                    CustomFrontendPlugins.Add((IContentsPlugin)plugin);
            }
        }


        /// <summary>
        /// Shuts down all plugins
        /// </summary>
        public static void ShutdownAll() {
            foreach (var plugin in _plugins)
                plugin.Shutdown();
        }


        /// <summary>
        /// Returns the IContentsPlugin that is in the same assembly as the given object
        /// </summary>
        /// <param name="obj">The object to handle</param>
        /// <returns>The IContentsPlugin, or null if none found</returns>
        public static IContentsPlugin GetContentsPluginFor(object obj) {
            var plugin = GetPluginFor(obj, typeof(IContentsPlugin));
            return plugin as IContentsPlugin;
        }


        /// <summary>
        /// Returns the IPlugin which implements the given pluginType whos type resides in the same Assembly as the given object.
        /// </summary>
        /// <param name="obj">The object</param>
        /// <param name="pluginType">The type of the plugin</param>
        /// <returns>A IPlugin if one was found, otherwise null</returns>
        static IPlugin GetPluginFor(object obj, Type pluginType) {
            var plugins = _plugins.Where(p => p.GetType().Assembly == obj.GetType().Assembly && pluginType.IsAssignableFrom(p.GetType()));
            return plugins.Count() > 0 ? plugins.First() : null;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> Get<T>() {
            return _plugins.Where(p => typeof(T).IsAssignableFrom(p.GetType())).Cast<T>();
        }



    }

}
