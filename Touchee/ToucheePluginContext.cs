using Touchee.Components;
using Touchee.Server;

namespace Touchee {

    /// <summary>
    /// The plugin context
    /// </summary>
    public class ToucheePluginContext : IPluginContext {

        public ToucheePluginContext(IServer server) {
            this.Server = server;
        }

        public IServer Server { get; protected set; }

    }

}
