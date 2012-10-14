using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Touchee {
    public interface IPlugin {
        string Name { get; }
        bool Start(dynamic config);
        bool Shutdown();
    }
}
