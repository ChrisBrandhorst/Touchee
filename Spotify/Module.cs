using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Nancy;
using SpotiFire;
using Touchee;
using Touchee.Artwork;

namespace Spotify {

    public class SpotifyModule : NancyModule {

        public SpotifyModule() : base("/spotify") {
            Get["/config"] = _ => Config(_);
            //Post["/login"] = _ => Login(_);
            Post["/login", true] = async (_, ct) => await Login(_, ct);
        }


        /// <summary>
        /// Gets the Spotify configuration
        /// </summary>
        public Response Config(dynamic parameters) {
            return Response.AsJson(
                Plugin.Config
            );
        }


        public async Task<Response> Login(dynamic parameters, CancellationToken ct) {
            var response = new Response();

            string username = Request.Form.username;
            string password = Request.Form.password;

            if (String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(password))
                response.StatusCode = HttpStatusCode.UnprocessableEntity;

            else {
                Error err = await Plugin.SessionHandler.Login(Request.Form.username, Request.Form.password);
                switch (err) {
                    case Error.OK:
                        response.StatusCode = HttpStatusCode.OK;
                        break;
                    case Error.BAD_USERNAME_OR_PASSWORD:
                        response.StatusCode = HttpStatusCode.Unauthorized;
                        break;
                    default:
                        response.StatusCode = HttpStatusCode.BadRequest;
                        break;
                }
                
            }

            return response;

        }

    }

}
