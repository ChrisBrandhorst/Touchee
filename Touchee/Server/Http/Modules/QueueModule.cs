using System;
using Nancy;
using Touchee.Server.Responses;
using Touchee.Playback;

namespace Touchee.Server.Http.Modules {

    public class QueueModule : ToucheeNancyModule {

        public QueueModule() : base("/queue") {
            var path = "/media/{mediaID}/containers/{containerID}/{params}";
            Post["/reset" + path] = _ => Reset(_);
            Post["/unshift" + path] = _ => Unshift(_);
            Post["/push" + path] = _ => Push(_);
        }


        public Response Reset(dynamic parameters) {
            return null;
        }

        public Response Unshift(dynamic parameters) {
            return null;
        }

        public Response Push(dynamic parameters) {
            return null;
        }



    }

}
