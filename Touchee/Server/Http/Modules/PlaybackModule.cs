using System;
using Nancy;
using Nancy.ModelBinding;
using Touchee.Server.Responses;
using Touchee.Playback;
using Touchee.Devices;

namespace Touchee.Server.Http.Modules {
    
	/// <summary>
	/// Playback handling
	/// </summary>
    public class PlaybackModule : ToucheeNancyModule {

        public PlaybackModule() : base("/playback") {
            Get["/"] = _ => GetStatus();
            Put["/pause"] = _ => Pause();
            Put["/play"] = _ => Play();
            Put["/position"] = _ => Position();
        }

		/// <summary>
		/// Returns the current playback status
		/// </summary>
        public Response GetStatus() {
            return Response.AsJson( new PlaybackResponse(Library.Player) );
        }

        /// <summary>
        /// Pause plauback
        /// </summary>
        public Response Pause() {
            if (Library.Player == null)
                return new ConflictResponse();
            else {
                Library.Player.Pause();
                return null;
            }
        }

        /// <summary>
        /// Continue playback
        /// </summary>
        public Response Play() {
            if (Library.Player == null)
                return new ConflictResponse();
            else {
                Library.Player.Play();
                return null;
            }
        }

        /// <summary>
        /// Adjust the position
        /// </summary>
        public Response Position() {
            if (Library.Player == null)
                return new ConflictResponse();
            else {
                Library.Player.Position = Request.Form["value"];
                return null;
            }
        }


    }

}
