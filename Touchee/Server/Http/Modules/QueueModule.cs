using System;
using System.Linq;
using Nancy;
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

            Put["/prev"] = _ => Prev();
            Put["/next"] = _ => Next();
            Put["/advance/{count}"] = _ => Advance(_);

            Put["/shuffle"] = _ => Shuffle();
            Put["/repeat"] = _ => Repeat();

            Put["/reset" + path] = _ => Reset();
            Put["/prioritize" + path] = _ => Prioritize();
            Put["/push" + path] = _ => Push();
            Put["/clear_upcoming"] = _ => ClearUpcoming();
            Put["/clear_priority"] = _ => ClearPriority();
        }

        /// <summary>
        /// Gets the current queue
        /// </summary>
        public Response GetQueue() {
            return Response.AsJson(
                new QueueResponse(Library.Queue)
            );
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
        /// Advance
        /// </summary>
        public Response Advance(dynamic parameters) {
            if (Library.Queue == null)
                return new ConflictResponse();
            else {
                try {
                    Library.Queue.Advance(parameters.count);
                    return null;
                }
                catch (ArgumentOutOfRangeException) {
                    return new BadRequestResponse();
                }
            }
        }

        /// <summary>
        /// Set shuffling on/off
        /// </summary>
        public Response Shuffle() {
            return null;
        }

        /// <summary>
        /// Set the repeat mode
        /// </summary>
        public Response Repeat() {
            if (Library.Queue == null)
                return new ConflictResponse();
            else if (!Request.Form.ContainsKey("mode"))
                return new BadRequestResponse();
            else {
                Library.Queue.Repeat = (RepeatMode)Enum.Parse(typeof(RepeatMode), Request.Form["mode"].ToTitleCase());
                return null;
            }
        }

        /// <summary>
        /// Replaces the entire queue
        /// </summary>
        public Response Reset() {
            int start = 0;
            if (Request.Form.ContainsKey("start"))
                start = Request.Form["start"];
            Library.BuildQueue(Container, Filter, start);
            return null;
        }

        /// <summary>
        /// Appends items to the beginning of the queue, prioritizing them
        /// </summary>
        public Response Prioritize() {
            if (Library.Queue == null)
                return Reset();
            else {
                var items = Library.GetItems(Container, Filter);
                Library.Queue.Prioritize(items, Container);
                return null;
            }
        }

        /// <summary>
        /// Pushes items to the end of the queue or the end of the priority queue,
        /// depending on whether the queue is running through the full container or not.
        /// </summary>
        public Response Push() {
            if (Library.Queue == null)
                return Reset();
            else {
                var items = Library.GetItems(Container, Filter);
                Library.Queue.Push(items, Container);
                return null;
            }
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

}
