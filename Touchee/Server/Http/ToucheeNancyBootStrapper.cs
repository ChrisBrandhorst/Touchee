//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

using Nancy;
using Nancy.Bootstrapper;
using Nancy.Conventions;
using Nancy.Diagnostics;
using Nancy.Session;

namespace Touchee.Server.Http {

    /// <summary>
    /// Nancy bootstrapper for the Touchee HttpServer
    /// </summary>
    public class ToucheeNancyBootStrapper : DefaultNancyBootstrapper {

        string _rootPath;

        public ToucheeNancyBootStrapper(string rootPath = "content") {
            _rootPath = rootPath;
        }

        //protected override void ApplicationStartup(TinyIoC.TinyIoCContainer container, IPipelines pipelines) {
        //    base.ApplicationStartup(container, pipelines);

        //    CookieBasedSessions.Enable(pipelines);
        //}

        protected override void ConfigureConventions(NancyConventions conventions) {
            base.ConfigureConventions(conventions);

            conventions.StaticContentsConventions.Add(
                StaticContentConventionBuilder.AddDirectory("", _rootPath)
            );
            conventions.ViewLocationConventions.Add(
                (viewName, model, context) => {
                    return string.Concat(_rootPath + "/", viewName);
                }
            );
        }

        protected override DiagnosticsConfiguration DiagnosticsConfiguration {
            get { return new DiagnosticsConfiguration { Password = "dashboard" }; }
        }

        protected override NancyInternalConfiguration InternalConfiguration {
            get {
                // Insert at position 0 so it takes precedence over the built in one.
                return NancyInternalConfiguration.WithOverrides(c => {
                    c.Serializers.Insert(0, typeof(JsonNetSerializer));
                    //c.RoutePatternMatcher = typeof(ToucheeRoutePatternMatcher);
                });
            }
        }
    }

}
