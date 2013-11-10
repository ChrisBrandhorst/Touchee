using System.Linq;

using Nancy;
using Nancy.Bootstrapper;
using Nancy.Conventions;
using Nancy.Diagnostics;
using Nancy.Session;
using Nancy.TinyIoc;

using Newtonsoft.Json;
using Touchee.Components;

namespace Touchee.Server.Http {

    /// <summary>
    /// Nancy bootstrapper for the Touchee HttpServer
    /// </summary>
    public class ToucheeNancyBootStrapper : DefaultNancyBootstrapper {


        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines) {
            StaticConfiguration.Caching.EnableRuntimeViewDiscovery = true;
            StaticConfiguration.Caching.EnableRuntimeViewUpdates = true;

            pipelines.AfterRequest += new GzipCompressionFilter();
        }


        protected override void ConfigureApplicationContainer(TinyIoCContainer container) {
            base.ConfigureApplicationContainer(container);
            
            container.Register(typeof(JsonSerializer), typeof(ToucheeJsonSerializer));
        }


        protected override void ConfigureConventions(NancyConventions conventions) {
            
            foreach (var component in PluginManager.FrontendPlugins) {
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

    }

}
