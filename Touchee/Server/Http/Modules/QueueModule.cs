using System;
using Nancy;
using Nancy.ModelBinding;
using Touchee.Server.Responses;
using Touchee.Playback;

namespace Touchee.Server.Http.Modules {

    class QueueParameters {
        public int Start { get; protected set; }
    }


    public class QueueModule : ToucheeNancyModule {

        public QueueModule() : base("/queue") {
            var path = "/media/{mediaID}/containers/{containerID}/{filter}";
            Get["/"] = _ => GetQueue(_);
            Post["/prev"] = _ => Prev(_);
            Post["/next"] = _ => Next(_);
            Post["/pause"] = _ => Pause(_);
            Post["/play"] = _ => Play(_);
            Post["/reset" + path] = _ => Reset(_);
            Post["/prioritize" + path] = _ => Prioritize(_);
            Post["/push" + path] = _ => Push(_);
            Post["/clear_upcoming"] = _ => ClearUpcoming(_);
            Post["/clear_priority"] = _ => ClearPriority(_);
        }

        /// <summary>
        /// Gets the current queue
        /// </summary>
        public Response GetQueue(dynamic parameters) {
            return Response.AsJson(
                new QueueResponse(Library.Queue)
            );
        }

        // Transport actions
        public Response Prev(dynamic parameters) { Library.Prev(); return null; }
        public Response Next(dynamic parameters) { Library.Next(); return null; }
        public Response Pause(dynamic parameters) { Library.Pause(); return null; }
        public Response Play(dynamic parameters) { Library.Play(); return null; }

        /// <summary>
        /// Replaces the entire queue
        /// </summary>
        public Response Reset(dynamic parameters) {
            var qp = this.Bind<QueueParameters>();
            Library.ResetQueue(Container, Filter, qp.Start);
            return null;
        }

        /// <summary>
        /// Appends items to the beginning of the queue, prioritizing them
        /// </summary>
        public Response Prioritize(dynamic parameters) {
            return null;
        }

        /// <summary>
        /// Pushes items to the end of the queue or the end of the priority queue,
        /// depending on whether the queue is running through the full container or not.
        /// </summary>
        public Response Push(dynamic parameters) {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        public Response ClearUpcoming(dynamic parameters) {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        public Response ClearPriority(dynamic parameters) {
            return null;
        }

    }

}
