using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Collections;

using Touchee.Components;
using Touchee.Server;

namespace Touchee {

    /// <remarks>
    /// The app!
    /// </remarks>
    internal static class Program {


        // The configuration of the app
        internal static dynamic Config { get; private set; }

        // 
        static List<string> PluginFilesNotLoaded = new List<string>();


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
            

            // Load devices from config
            if (Config.ContainsKey("devices"))
                Devices.Device.Parse(Config["devices"]);


            // Init the server
            var server = new Server.ToucheeServer(httpPort, websocketPort);


            // Build plugin context
            IPluginContext pluginContext = new ToucheePluginContext(server);


            // Loads all plugins
            if (!LoadPlugins(pluginContext)) return false;


            // Kickstart WinLirc client
            var wlc = Devices.WinLirc.Client;


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
        /// <param name="context">The plugin context to feed the plugins</param>
        /// <returns>False if the plugin folder cannot be read, otherwise true</returns>
        static bool LoadPlugins(IPluginContext context) {
            
            // Get the plugins config section
            dynamic pluginsConfig;
            Config.TryGetValue("plugins", out pluginsConfig);

            // First, process plugins in the app and main lib assembly
            try {
                LoadPlugins(Assembly.GetExecutingAssembly(), pluginsConfig, context);
                LoadPlugins(Assembly.Load("ToucheeLib"), pluginsConfig, context);
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
                foreach (var d in Directory.GetDirectories(pluginsPath).Reverse()) {
                    pluginFiles.AddRange(Directory.GetFiles(d, "*.dll"));
                }

            }
            catch (Exception e) {
                Logger.Log(e, Logger.LogLevel.Fatal);
                ShowError("Could not load plugins: " + e.Message, true);
                return false;
            }
            
            // Loop through all plugin files
            PluginFilesNotLoaded.AddRange(pluginFiles);
            foreach (string filename in pluginFiles) {
                try {
                    PluginFilesNotLoaded.Remove(filename);
                    // Load the assembly
                    var assembly = Assembly.LoadFile(filename);
                    // Load the plugins from the assembly
                    LoadPlugins(assembly, pluginsConfig, context);
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
        /// <param name="config">Plugins config section</param>
        /// <param name="context">The plugin context to feed the plugin</param>
        static void LoadPlugins(Assembly assembly, dynamic config, IPluginContext context) {

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
            var thisAssembly = Assembly.GetExecutingAssembly();
            foreach (var type in types) {
                bool ok = true;
                IPlugin plugin;

                // Get the configuration
                dynamic pluginConfig = null;
                var name = type.Assembly == thisAssembly ? type.Name : type.Assembly.GetName().Name;
                bool disabled = false;
                if (config != null) {
                    config.TryGetValue(name, out pluginConfig);
                    if (pluginConfig != null)
                        disabled = pluginConfig.ContainsKey("disabled");
                }

                // Do next if disabled
                if (disabled) continue;

                // Create instance of plugin
                try {
                    plugin = (IPlugin)Activator.CreateInstance(type);
                }
                catch (Exception e) {
                    Logger.Log("Cannot instantiate plugin: " + name + " : " + e.Message, Logger.LogLevel.Error);
                    continue;
                }

                // Boot the plugin
                ok = plugin.StartPlugin(pluginConfig, context);

                // Store plugin if successfull
                if (ok) {
                    PluginManager.Register(plugin);
                    Logger.Log("Plugin loaded: " + plugin.Name);
                }
                else {
                    Logger.Log("Start of plugin failed: " + name, Logger.LogLevel.Error);
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

            // Unregister all plugins
            PluginManager.UnregisterAll();

            // Shutdown WinLirc client
            Devices.WinLirc.Client.Disconnect();

            // TODO: nicely shutdown servers?

            // Exit
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
        /// searches for the assembly in the lib folder of that plugin or checks other plugins for inter-plugin
        /// dependencies.
        /// </summary>
        /// <returns>The resolved assembly or null</returns>
        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
            Assembly assembly = null;
            if (args.RequestingAssembly == null) return assembly;

            // Get info from of the assembly file
            var fileInfo = new FileInfo(args.RequestingAssembly.Location);

            // If this is a plugin, check the lib\ folder of that plugin
            if (fileInfo.Directory.FullName == Path.Combine(Environment.CurrentDirectory, "plugins", Path.GetFileNameWithoutExtension(fileInfo.Name))) {
                List<string> dlls = new List<string>();

                var libPath = Path.Combine(fileInfo.DirectoryName, "lib");
                if (Directory.Exists(libPath))
                    dlls.AddRange( Directory.GetFiles(libPath, "*.dll") );
                dlls.AddRange(PluginFilesNotLoaded);

                
                // Go through all dlls, to see if the requested assembly is present
                foreach (var dll in dlls.Where(f => Util.IsNetAssembly(f))) {
                    try {
                        if (PluginFilesNotLoaded.Contains(dll))
                            PluginFilesNotLoaded.Remove(dll);
                        var ass = Assembly.LoadFrom(dll);
                        if (ass.GetName().ToString() == args.Name)
                            return ass;
                    }
                    catch (Exception) { }
                }

            }

            return assembly;
        }


    }

}
