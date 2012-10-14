using System;
using Nancy;
using Touchee.Server.Responses;
using Touchee.Playback;

namespace Touchee.Server.Http.Modules {

    public class ControlModule : ToucheeNancyModule {

        public ControlModule() : base("/") {
            Get["/play/containers/{containerId}/{filter}"] = x => StartPlayback(x);
            Get["/queues/{queueId}/play"] = x => Play(x);
            Get["/queues/{queueId}/pause"] = x => Pause(x);
            Get["/queues/{queueId}/prev"] = x => Prev(x);
            Get["/queues/{queueId}/next"] = x => Next(x);
        }

        public Response StartPlayback(dynamic parameters) {
            // Get the container
            var container = GetContainerFromParams(parameters);
            if (container == null) return null;

            // Build the filter
            var filter = Touchee.Options.Build(parameters.filter);

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
