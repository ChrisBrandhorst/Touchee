using Nancy;
using Touchee.Server.Responses;

namespace Touchee.Server.Http.Modules {

    public class MediaModule : ToucheeNancyModule {

        public MediaModule() : base("/media") {
            Get["/"] = x => Response.AsJson(new MediaResponse());
        }

    }

}
