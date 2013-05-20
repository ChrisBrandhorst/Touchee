using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Touchee.Components {

    public interface IPlugin {
        string Name { get; }
        string Description { get; }
        Version Version { get; }
        bool StartPlugin(dynamic config, IPluginContext context);
        bool StopPlugin();
    }

}
