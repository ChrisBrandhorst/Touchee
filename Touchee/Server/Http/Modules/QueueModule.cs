using System;
using Nancy;
using Nancy.ModelBinding;
using Touchee.Server.Responses;
using Touchee.Playback;

namespace Touchee.Server.Http.Modules {

    /// <summary>
    /// Queue handling
    /// </summary>
    public class QueueModule : ToucheeNancyModule {

        public QueueModule() : base("/queue") {
            var path = "/media/{mediaID}/containers/{containerID}/{filter}";
            
            Get["/"] = _ => GetQueue();

            Post["/prev"] = _ => Prev();
            Post["/next"] = _ => Next();

            Post["/shuffle"] = _ => Shuffle();
            Post["/repeat"] = _ => Repeat();

            Post["/reset" + path] = _ => Reset();
            Post["/prioritize" + path] = _ => Prioritize();
            Post["/push" + path] = _ => Push();
            Post["/clear_upcoming"] = _ => ClearUpcoming();
            Post["/clear_priority"] = _ => ClearPriority();
        }

        /// <summary>
        /// Gets the current queue
        /// </summary>
        public Response GetQueue() {
            return Library.Queue == null
                ? new ConflictResponse()
                : Response.AsJson( new QueueResponse(Library.Queue) );
        }

        /// <summary>
        /// Go back
        /// </summary>
        public Response Prev() {
            if (Library.Queue == null)
                return new ConflictResponse();
            else {
                if (Library.Queue.IsAtFirstItem)
                    Library.Queue.Index = 0;
                else
                    Library.Queue.GoPrev();
                return null;
            }
        }

        /// <summary>
        /// Go forward
        /// </summary>
        public Response Next() {
            if (Library.Queue == null)
                return new ConflictResponse();
            else {
                Library.Queue.GoNext();
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Response Shuffle() {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        public Response Repeat() {
            if (Library.Queue == null)
                return new ConflictResponse();
            else {
                var rp = this.Bind<RepeatParameters>();
                if (rp.Mode == null)
                    return new Response() { StatusCode = HttpStatusCode.BadRequest };
                else {
                    Library.Queue.Repeat = (RepeatMode)Enum.Parse(typeof(RepeatMode), rp.Mode.ToTitleCase());
                    return null;
                }
            }
        }

        /// <summary>
        /// Replaces the entire queue
        /// </summary>
        public Response Reset() {
            var qp = this.Bind<QueueParameters>();
            Library.ResetQueue(Container, Filter, qp.Start);
            return null;
        }

        /// <summary>
        /// Appends items to the beginning of the queue, prioritizing them
        /// </summary>
        public Response Prioritize() {
            return null;
        }

        /// <summary>
        /// Pushes items to the end of the queue or the end of the priority queue,
        /// depending on whether the queue is running through the full container or not.
        /// </summary>
        public Response Push() {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        public Response ClearUpcoming() {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        public Response ClearPriority() {
            return null;
        }

    }



    class QueueParameters {
        public int Start { get; protected set; }
    }
    class RepeatParameters {
        public string Mode { get; protected set; }
    }
}
