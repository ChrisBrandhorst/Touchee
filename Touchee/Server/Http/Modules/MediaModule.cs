using Nancy;

namespace Touchee.Server.Http.Modules {

    public class MediaModule : ToucheeNancyModule {

        public MediaModule() : base("/media") {
            Get["/"] = x => Response.AsJson(Library.GetMedia());
        }

    }

}
