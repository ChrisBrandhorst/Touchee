using System.Linq;

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

        protected override void ApplicationStartup(Nancy.TinyIoc.TinyIoCContainer container, IPipelines pipelines) {
            StaticConfiguration.DisableCaches = false;
        }

        protected override void ConfigureConventions(NancyConventions conventions) {
            
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

    }

}
