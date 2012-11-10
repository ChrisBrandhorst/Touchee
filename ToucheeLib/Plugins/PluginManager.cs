using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;

using Touchee.Components;

namespace Touchee.Plugins {


    public static class PluginManager {


        /// <summary>
        /// List containing the loaded plugins
        /// </summary>
        static List<IPlugin> Plugins = new List<IPlugin>();


        /// <summary>
        /// List containing all loaded components
        /// </summary>
        static List<IComponent> Components = new List<IComponent>();


        /// <summary>
        /// Adds the given plugin to the plugins collection
        /// </summary>
        /// <param name="plugin">Plugin to add</param>
        public static void Register(IPlugin plugin) {
            Plugins.Add(plugin);
        }


        /// <summary>
        /// Removes the given plugin from the plugins collection
        /// </summary>
        /// <param name="plugin">The plugin to remove</param>
        public static void Unregister(IPlugin plugin) {
            plugin.StopPlugin();
            Plugins.Remove(plugin);
        }


        /// <summary>
        /// Adds the given component to the component collection
        /// </summary>
        /// <param name="component"></param>
        public static void Register(IComponent component) {
            Components.Add(component);
        }


        /// <summary>
        /// Removes the given component from the component collection
        /// </summary>
        /// <param name="component">The component to remove</param>
        public static void Unregister(IComponent component) {
            Components.Remove(component);
        }


        /// <summary>
        /// Shuts down all plugins
        /// </summary>
        public static void UnregisterAll() {
            foreach (var plugin in Plugins)
                Unregister(plugin);
        }


        /// <summary>
        /// Gets all components which are assignable from the given IComponent type
        /// </summary>
        /// <typeparam name="T">The IComponent type to search for</typeparam>
        /// <returns>The list</returns>
        public static IEnumerable<T> GetComponent<T>() where T : IComponent {
            return Components.Where(p => typeof(T).IsAssignableFrom(p.GetType())).Cast<T>();
        }


        /// <summary>
        /// Gets the IComponent of the given IComponent type which resides in the same Assembly as the given object.
        /// </summary>
        /// <typeparam name="T">The IComponent type to search for</typeparam>
        /// <param name="obj">The object</param>
        /// <returns>A IComponent if one was found (if more than one defined in the Assembly, the first added is returned), otherwise the default of T</returns>
        public static T GetComponent<T>(object obj) where T : IComponent {
            var components = Components.Where(c => c.GetType().Assembly == obj.GetType().Assembly && typeof(T).IsAssignableFrom(c.GetType()));
            return components.Count() > 0 ? (T)components.First() : default(T);
        }


    }

}
