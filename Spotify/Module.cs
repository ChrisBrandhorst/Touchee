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
            Get["/session"] = _ => SessionStatus(_);
            Post["/session", true] = async (_, ct) => await Login(_, ct);
            Delete["/session", true] = async (_, ct) => await Logout(_, ct);
        }


        /// <summary>
        /// Gets the Spotify configuration
        /// </summary>
        public Response Config(dynamic parameters) {
            return Response.AsJson(
                Plugin.Config
            );
        }


        public Response SessionStatus(dynamic parameters) {
            return Response.AsJson(
                new SessionStatus(Plugin.SessionHandler)
            );
        }


        public async Task<Response> Login(dynamic parameters, CancellationToken ct) {
            HttpStatusCode status;

            string username = Request.Form.username;
            string password = Request.Form.password;

            if (String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(password))
                status = HttpStatusCode.UnprocessableEntity;

            else {
                Error err = await Plugin.SessionHandler.Login(Request.Form.username, Request.Form.password);
                switch (err) {
                    case Error.OK:
                        status = HttpStatusCode.OK;
                        break;
                    case Error.BAD_USERNAME_OR_PASSWORD:
                        status = HttpStatusCode.Unauthorized;
                        break;
                    default:
                        status = HttpStatusCode.BadRequest;
                        break;
                }
                
            }
            
            return Response.AsJson(
                new { status = Enum.GetName(typeof(HttpStatusCode), status) },
                status
            );
        }


        public async Task<Response> Logout(dynamic parameters, CancellationToken ct) {
            Plugin.SessionHandler.Logout();
            return new Response();
        }


    }

}
