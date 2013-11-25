using System;
using Nancy;
using Touchee.Server;

namespace Touchee.Server.Http.Modules {
    
    public abstract class ToucheeNancyModule : NancyModule {


        /// <summary>
        /// The cookie key for the application
        /// </summary>
        static readonly string CookieKey = "ToucheeSession";


        /// <summary>
        /// Construct a new ToucheeNancyModule
        /// </summary>
        public ToucheeNancyModule() : base() { }


        /// <summary>
        /// Construct a new ToucheeNancyModule with a base path
        /// </summary>
        /// <param name="modulePath">The base path for this module</param>
        public ToucheeNancyModule(string modulePath) : base(modulePath) {
            Before += FetchContext;
            After += SetSessionId;
        }


        /// <summary>
        /// Gets the Touchee Library from which to retrieve media data
        /// </summary>
        public Library Library { get { return Library.Instance; } }


        /// <summary>
        /// Gets the Client object associated with the current request
        /// </summary>
        public IClient Client { get; private set; }


        /// <summary>
        /// Gets the Container associated with the current request (if any)
        /// </summary>
        public Container Container { get; private set; }


        /// <summary>
        /// Gets the filter assosicated with the current request (if any)
        /// </summary>
        public Options Filter { get; private set; }


        /// <summary>
        /// Pre-sets relevant data from the context
        /// </summary>
        /// <param name="context">The request context</param>
        /// <returns>Null</returns>
        Response FetchContext(NancyContext context) {
            Client = null;
            Container = null;
            Filter = null;

            // Get client from session ID
            if (Request.Cookies.ContainsKey(CookieKey)) {
                var sessionId = Request.Cookies[CookieKey];
                Client = Touchee.Server.Client.FindBySessionID(sessionId);
            }

            // Get container
            if (context.Parameters.containerID != null) {
                int containerID = context.Parameters.containerID;
                if (Container.Exists(containerID))
                    Container = Container.Find(containerID);
            }

            // Get filter
            if (context.Parameters.filter != null) {
                string filterStr = context.Parameters.filter;
                int itemID;
                if (Int32.TryParse(filterStr, out itemID))
                    filterStr = "id/" + filterStr;
                Filter = Touchee.Options.Build(filterStr);
            }
            else
                Filter = new Touchee.Options();
            
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
