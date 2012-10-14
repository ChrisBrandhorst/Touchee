using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Collections;

namespace Touchee {

    /// <remarks>
    /// The app!
    /// </remarks>
    internal static class Program {

        // The configuration of the app
        internal static dynamic Config { get; private set; }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            Init();
        }


        /// <summary>
        /// Checks and initialises all the components for the application.
        /// </summary>
        static bool Init() {
            bool ok;
            

            // Set the error level
            Logger.Level = Logger.LogLevel.Info;


            // Loads all settings
            Config = LoadConfig();
            if (Config == null) return false;


            // Loads all plugins
            if (!LoadPlugins()) return false;


            // Get port for the http server
            int httpPort;
            ok = Config.TryGetInt("httpPort", out httpPort, 80);
            if (!ok || httpPort <= 0)
                Logger.Log("Could not parse a valid value for httpPort setting. Using default: " + httpPort.ToString(), Logger.LogLevel.Warn);

            // Get port for the websocket server
            int websocketPort;
            ok = Config.TryGetInt("websocketPort", out websocketPort, 81);
            if (!ok || websocketPort <= 0)
                Logger.Log("Could not parse a valid value for websocketPort setting. Using default: " + websocketPort.ToString(), Logger.LogLevel.Warn);

            // Get the media detector polling interval
            int mediaPollingInterval;
            ok = Config.TryGetInt("mediaPollingInterval", out mediaPollingInterval, 3000);
            if (!ok || mediaPollingInterval <= 0)
                Logger.Log("Could not parse a valid value for mediaPollingInterval setting. Using default: " + mediaPollingInterval.ToString(), Logger.LogLevel.Warn);

            // Init the server
            var server = new Server.ToucheeServer("web", httpPort, websocketPort);


            // Init the library
            var library = Library.Instance;
            library.Init(server, mediaPollingInterval);


            // Start server
            if (!server.Start()) {
                Shutdown("Could not start server");
                return false;
            }


            // Show the form
            Logger.Log("Loading complete!", Logger.LogLevel.Info);
            var mainForm = new Forms.Main();
            Application.Run(mainForm);


            // Done
            return true;
        }

        
        /// <summary>
        /// (re)parses the config file.
        /// </summary>
        /// <returns>true if the config file was successfully parsed.</returns>
        static dynamic LoadConfig() {
            try {
                dynamic config = ConfigObject.Load(Path.Combine(
                    Path.GetDirectoryName(Application.ExecutablePath),
                    "config.yaml"
                ));
                return config;
            }
            catch (Exception e) {
                Logger.Log(e, Logger.LogLevel.Fatal);
                ShowError("Cannot load configuration. Incorrect YAML? :: " + e.Message, true);
                return null;
            }
        }


        /// <summary>
        /// Loads all plugins from the plugins folder
        /// </summary>
        /// <returns>False if the plugin folder cannot be read, otherwise true</returns>
        static bool LoadPlugins() {
            
            // Get the plugins config section
            dynamic pluginsConfig;
            Config.TryGetValue("plugins", out pluginsConfig);

            // First, process plugins in the app and main lib assembly
            try {
                LoadPlugins(Assembly.GetExecutingAssembly(), pluginsConfig);
                LoadPlugins(Assembly.Load("Touchee"), pluginsConfig);
            }
            catch (Exception e) {
                Logger.Log("Some internal plugin failed to load: " + e.Message, Logger.LogLevel.Error);
            }

            // Get the plugins path and init the list
            var pluginsPath = Path.Combine(Application.StartupPath, "plugins");
            var pluginFiles = new List<string>();

            // Look for plugins with only one DLL file directly in the plugins folder
            try {
                Directory.CreateDirectory(pluginsPath);
                pluginFiles.AddRange(Directory.GetFiles(pluginsPath, "*.dll"));
            }
            catch (Exception e) {
                Logger.Log(e, Logger.LogLevel.Fatal);
                ShowError("Could not load plugins: " + e.Message, true);
                return false;
            }

            // Look for plugins in a subfolder
            try {
                foreach (var d in Directory.GetDirectories(pluginsPath)) {
                    var dirInfo = new DirectoryInfo(d);
                    // Find the dll with the same name as the folder
                    var pluginFile = Path.Combine(d, dirInfo.Name + ".dll");
                    if (File.Exists(pluginFile))
                        pluginFiles.Add(pluginFile);
                    else
                        Logger.Log(String.Format("Invalid plugin: {0}.dll not found in {1}", dirInfo.Name, d), Logger.LogLevel.Error);
                }

            }
            catch (Exception e) {
                Logger.Log(e, Logger.LogLevel.Fatal);
                ShowError("Could not load plugins: " + e.Message, true);
                return false;
            }
            
            // Loop through all plugin files
            foreach (string filename in pluginFiles) {
                try {
                    // Load the assembly
                    var assembly = Assembly.LoadFile(filename);
                    // Load the plugins from the assembly
                    LoadPlugins(assembly, pluginsConfig);
                }
                catch(Exception e) {
                    Logger.Log("Unable to load DLL as Touchee plugin: " + filename + " : " + e.Message, Logger.LogLevel.Error);
                }
            }

            return true;
        }


        /// <summary>
        /// Loads the plugins found in the given assembly
        /// </summary>
        /// <param name="assembly">The assembly to get the plugins from</param>
        /// <param name="pluginsConfig">Plugins config section</param>
        static void LoadPlugins(Assembly assembly, dynamic pluginsConfig) {

            // Get reference to plugin interface
            Type pluginType = typeof(IPlugin);

            // Get the type which implements IPlugin
            var types = assembly.GetTypes().Where(p => pluginType.IsAssignableFrom(p) && p != pluginType);

            // No plugin found
            if (types.Count() == 0) {
                Logger.Log("No types found which implements IPlugin in assembly: " + assembly.FullName);
                return;
            }
            
            // Go through all plugins
            foreach (var type in types) {
                bool ok = true;
                IPlugin plugin;

                // Get the configuration
                dynamic pluginConfig = null;
                if (pluginsConfig != null)
                    pluginsConfig.TryGetValue(type.Name, out pluginConfig);

                // Create instance of plugin
                try {
                    plugin = (IPlugin)Activator.CreateInstance(type);
                }
                catch (Exception e) {
                    Logger.Log("Cannot instantiate plugin: " + type.Name + " : " + e.Message, Logger.LogLevel.Error);
                    continue;
                }

                // Boot the plugin
                ok = plugin.Start(pluginConfig);

                // Store plugin if successfull
                if (ok) {
                    Plugins.Add(plugin);
                    Logger.Log("Plugin loaded: " + plugin.Name);
                }
                else {
                    Logger.Log("Start of plugin failed: " + type.Name, Logger.LogLevel.Error);
                }
            }
        }


        /// <summary>
        /// Called when a fatal error has occured in some Touchee object.
        /// </summary>
        /// <param name="toucheeObject">The Touchee object where the error occured</param>
        /// <param name="message">The error message</param>
        static void base_FatalErrorOccured(Touchee.Base toucheeObject, string message) {
            Shutdown(message);
        }


        /// <summary>
        /// Tries to shutdown the application.
        /// </summary>
        /// <returns>true if the shutdown was successfull, otherwise false.</returns>
        public static bool Shutdown() {
            Plugins.ShutdownAll();
            Environment.Exit(1);
            return true;
        }


        /// <summary>
        /// Tries to shutdown the application.
        /// </summary>
        /// <param name="message">The error message to be shown.</param>
        /// <returns>true if the shutdown was successfull, otherwise false.</returns>
        public static bool Shutdown(string message) {
            Logger.Log(message, Logger.LogLevel.Fatal);
            ShowError(message, true);
            return Shutdown();
        }


        /// <summary>
        /// Shows an error message box.
        /// </summary>
        /// <param name="error">The error message to be shown.</param>
        /// <param name="fatal">Whether the error is fatal, i.e. the application will shut down.</param>
        public static void ShowError(string error, bool fatal = false) {
            string msg = "An error occured:\n\n" + error;

            if (fatal) msg += "\n\nThe application will shut down.";
            MessageBox.Show(msg, Application.ProductName + " - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


        /// <summary>
        /// If an assembly is not found, this method checks if the requesting assembly is a plugin and then
        /// searches for the assembly in the lib folder of that plugin.
        /// </summary>
        /// <returns>The resolved assembly or null</returns>
        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
            Assembly assembly = null;
            if (args.RequestingAssembly == null) return assembly;

            // Get info from of the assembly file
            var fileInfo = new FileInfo(args.RequestingAssembly.Location);

            // If this is a plugin, check the lib\ folder of that plugin
            if (fileInfo.Directory.FullName == Path.Combine(Environment.CurrentDirectory, "plugins", Path.GetFileNameWithoutExtension(fileInfo.Name))) {
                var libPath = Path.Combine(fileInfo.DirectoryName, "lib");
                if (Directory.Exists(libPath)) {
                    // Go through all dlls, to see if the requested assembly is present
                    foreach (var dll in Directory.GetFiles(libPath, "*.dll")) {
                        try {
                            var ass = Assembly.LoadFrom(dll);
                            if (ass.GetName().ToString() == args.Name)
                                return ass;
                        }
                        catch (Exception) { }
                    }
                }
            }

            return assembly;
        }


    }

}
