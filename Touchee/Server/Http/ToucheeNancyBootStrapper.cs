﻿using System.Linq;

using Nancy;
using Nancy.Bootstrapper;
using Nancy.Conventions;
using Nancy.Diagnostics;
using Nancy.Session;

using Touchee.Plugins;

namespace Touchee.Server.Http {

    /// <summary>
    /// Nancy bootstrapper for the Touchee HttpServer
    /// </summary>
    public class ToucheeNancyBootStrapper : DefaultNancyBootstrapper {

        //public ToucheeNancyBootStrapper() { }

        //protected override void ApplicationStartup(TinyIoC.TinyIoCContainer container, IPipelines pipelines) {
        //    base.ApplicationStartup(container, pipelines);

        //    CookieBasedSessions.Enable(pipelines);
        //}

        protected override void ConfigureConventions(NancyConventions conventions) {
            base.ConfigureConventions(conventions);

            foreach (var component in PluginManager.FrontendComponents) {
                var pluginName = component.GetType().Assembly.GetName().Name;
                conventions.StaticContentsConventions.Add(
                    StaticContentConventionBuilder.AddDirectory("app/plugins/" + pluginName.ToUnderscore(), "plugins/" + pluginName + "/web")
                );
            }

            conventions.StaticContentsConventions.Add(
                StaticContentConventionBuilder.AddDirectory("", "web")
            );
            conventions.ViewLocationConventions.Add(
                (viewName, model, context) => {
                    return string.Concat("web/", viewName);
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
