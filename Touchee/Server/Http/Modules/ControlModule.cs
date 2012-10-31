using System;
using Nancy;
using Touchee.Server.Responses;
using Touchee.Playback;

namespace Touchee.Server.Http.Modules {

    public class ControlModule : ToucheeNancyModule {

        public ControlModule() : base("/control") {
            //Get["/media/{mediaID}/containers/{containerID}/play/{filter}"] = _ => StartPlayback(_);
            Post["/play"] = _ => StartPlayback(_);
            Get["/queues/{queueId}/play"] = _ => Play(_);
            Get["/queues/{queueId}/pause"] = _ => Pause(_);
            Get["/queues/{queueId}/prev"] = _ => Prev(_);
            Get["/queues/{queueId}/next"] = _ => Next(_);
        }

        public Response StartPlayback(dynamic parameters) {
            // Get the container
            var container = GetContainerFromParams(Request.Form);
            if (container == null) return null;

            // Build the filter
            var filter = Touchee.Options.Build(Request.Form.filter);

            // Play it
            Library.Play(container, filter);
            return null;
        }

        public Response Play(dynamic parameters) {
            var queue = GetQueue(parameters);
            if (queue != null)
                Library.Play(queue);
            return null;
        }

        public Response Pause(dynamic parameters) {
            var queue = GetQueue(parameters);
            if (queue != null)
                Library.Pause(queue);
            return null;
        }

        public Response Prev(dynamic parameters) {
            var queue = GetQueue(parameters);
            if (queue != null)
                Library.Prev(queue);
            return null;
        }

        public Response Next(dynamic parameters) {
            var queue = GetQueue(parameters);
            if (queue != null)
                Library.Next(queue);
            return null;
        }

        Queue GetQueue(dynamic parameters) {
            int queueID = parameters.queueId;
            if (!(queueID > 0)) return null;
            return Queue.Exists(queueID) ? Queue.Find(queueID) : null;
        }

    }

}
