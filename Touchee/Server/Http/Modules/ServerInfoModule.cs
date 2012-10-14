using Nancy;
using Touchee.Server.Responses;

namespace Touchee.Server.Http.Modules {

    public class ServerInfoModule : ToucheeNancyModule {

        public ServerInfoModule() : base("/server_info") {
            Get["/"] = x => Response.AsJson(Library.GetServerInfo());
        }

    }

}
