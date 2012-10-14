using System;
using Nancy;
using Touchee.Server;

namespace Touchee.Server.Http.Modules {
    
    public class ToucheeNancyModule : NancyModule {


        /// <summary>
        /// The cookie key for the application
        /// </summary>
        static readonly string CookieKey = "ToucheeSession";


        /// <summary>
        /// Construct a new ToucheeNancyModule
        /// </summary>
        public ToucheeNancyModule() : this(string.Empty) { }


        /// <summary>
        /// Construct a new ToucheeNancyModule with the given base path
        /// </summary>
        /// <param name="modulePath">The base path for this module</param>
        public ToucheeNancyModule(string modulePath) : base(modulePath) {
            Before += GetClient;
            After += SetSessionId;
        }


        /// <summary>
        /// Gets the Touchee Library from which to retrieve media data
        /// </summary>
        public Library Library { get { return Library.Instance; } }


        /// <summary>
        /// Gets the Client object associated with the current request
        /// </summary>
        public Client Client { get; private set; }


        /// <summary>
        /// Retrieve the Container from the given parameters
        /// </summary>
        /// <param name="parameters">The parameters which may contain a containerId param</param>
        /// <returns>The matched Container, otherwise null</returns>
        public Container GetContainerFromParams(dynamic parameters) {
            Container container = null;

            int containers_id = parameters.containerId;
            if (containers_id > 0 && Container.Exists(containers_id))
                container = Container.Find(containers_id);
            return container;
        }


        /// <summary>
        /// Sets the Client for this request
        /// </summary>
        /// <param name="context">The request context</param>
        /// <returns>Null</returns>
        Response GetClient(NancyContext context) {
            if (Request.Cookies.ContainsKey(CookieKey)) {
                var sessionId = Request.Cookies[CookieKey];
                this.Client = Client.FindBySessionID(sessionId);
            }
            else
                this.Client = null;
            return null;
        }
        

        /// <summary>
        /// Sets a Session ID if needed
        /// </summary>
        /// <param name="context">The request context</param>
        /// <returns>Null</returns>
        void SetSessionId(NancyContext context) {
            if (!Request.Cookies.ContainsKey(CookieKey)) {
                context.Response.Cookies.Add(
                    new Nancy.Cookies.NancyCookie(CookieKey, Guid.NewGuid().ToString())
                );
            }
        }

    }

}
