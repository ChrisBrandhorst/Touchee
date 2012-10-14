using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Touchee.Server.Http.Modules {

    public class RootModule : ToucheeNancyModule {

        public RootModule() : base("/") {
            Get["/"] = _ => View["index"];
        }

    }

}


