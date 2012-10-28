using System;
using System.Net;
using System.Collections.Generic;
using Nancy.Hosting.Self;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

namespace Touchee.Server.Http {

    /// <remarks>
    /// HTTP server for serving static files and content for the web interface
    /// </remarks>
    public class HttpServer : Touchee.Base {


        // The nancy host
        NancyHost _host;


        /// <summary>
        /// Initialises a new HttpServer.
        /// </summary>
        /// <param name="port"></param>
        public HttpServer(int port = 80) {

            // Init the Nacny self host
            _host = new NancyHost(
                //new Uri( String.Format("http://localhost:{0}/", port) ),
                new ToucheeNancyBootStrapper(),
                this.CollectUris(port)
            );
            
        }

        /// <summary>
        /// Start the HttpServer
        /// </summary>
        public void Start() {
            _host.Start();
        }


        /// <summary>
        /// Gets the available endpoint Uris for the current machine and given port
        /// </summary>
        /// <param name="port">The port for the Uri</param>
        /// <returns>The available endpoint Uris</returns>
        Uri[] CollectUris(int port) {
            var uris = new List<Uri>();
            var addresses = new List<string>();

            // Get the available addresses
            var hostname = Dns.GetHostName().ToLower();
            addresses.Add(hostname);
            addresses.Add("localhost");
            foreach (var addr in Dns.GetHostEntry(hostname).AddressList)
                addresses.Add(addr.ToString());

            // Create valid Uris
            foreach (var a in addresses) {
                Uri uri;
                var valid = Uri.TryCreate(String.Format("http://{0}:{1}", a, port), UriKind.Absolute, out uri);
                if (valid) uris.Add(uri);
            }

            return uris.ToArray();
        }


    }

}
