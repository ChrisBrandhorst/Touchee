using Nancy;

namespace Touchee.Server.Http.Modules {

    public class RootModule : ToucheeNancyModule {

        public RootModule() : base("/") {
            Get["/"] = _ => View["index"];
        }

    }

}


