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
            Post["/pause"] = _ => Pause();
            Post["/play"] = _ => Play();
            Post["/volume"] = _ => MasterVolume();
        }

		/// <summary>
		/// Returns the current playback status
		/// </summary>
        public Response GetStatus() {
            return Response.AsJson( new PlaybackResponse() );
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
        /// Conet master volume level
        /// </summary>
        public Response MasterVolume() {
			var vp = this.Bind<VolumeParameters>();
            Device.MasterVolume.Volume = vp.Level;
            Device.MasterVolume.Muted = vp.Muted;
            return null;
        }


    }

    class VolumeParameters {
        public int Level { get; protected set; }
        public bool Muted { get; protected set; }
    }
}
