using Touchee.Server;
using Touchee.Components.Playback;

namespace Touchee.Components {

    /// <summary>
    /// Context definition for plugins
    /// </summary>
    public interface IPluginContext {

        /// <summary>
        /// The main Touchee server
        /// </summary>
        IServer Server { get; }

    }

}
