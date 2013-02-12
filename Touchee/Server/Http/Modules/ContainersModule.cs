﻿using System;
using System.Drawing;
using System.IO;
using Nancy;
using Touchee;
using Touchee.Server.Responses;
using Touchee.Artwork;

namespace Touchee.Server.Http {
    public enum ArtworkSize {
        Small = 43,
        Medium = 128,
        Large = 748
    }
}

namespace Touchee.Server.Http.Modules {

    public class ContainersModule : ToucheeNancyModule {

        public ContainersModule() : base("/media/{mediaID}/containers") {
            Get["/"] = _ => Index(_);
            Get["/{containerID}/contents"] = _ => GetContents(_);
            Get["/{containerID}/contents/{filter}"] = _ => GetContents(_);
            Get["/{containerID}/artwork/{filter}"] = _ => GetArtwork(_);
        }


        /// <summary>
        /// Get the list of all containers
        /// </summary>
        public Response Index(dynamic parameters) {
            int mediaID = parameters.mediaID;
            if (mediaID == 0) return null;
            
            var medium = Medium.Find(mediaID);
            return Response.AsJson(
                Library.GetContainers(medium)
            );
        }


        /// <summary>
        /// Gets the contents of the given container
        /// </summary>
        public Response GetContents(dynamic parameters) {
            var container = GetContainerFromParams(parameters);
            if (container == null) return null;

            ContentsResponse contents = Library.GetContents(
                container,
                Touchee.Options.Build(parameters.filter)
            );

            return Response.AsJson(contents);
        }


        /// <summary>
        /// Gets artwork
        /// </summary>
        public Response GetArtwork(dynamic parameters) {

            // Get container
            var container = GetContainerFromParams(parameters);
            if (container == null) return null;

            string filterStr = parameters.filter;
            int itemID;
            if (Int32.TryParse(filterStr, out itemID))
                filterStr = "id/" + filterStr;

            Options filter = Touchee.Options.Build(filterStr);

            Image artwork = Library.GetArtwork(container, filter);

            // Output artwork
            if (artwork != null) {

                if (filter.ContainsKey("size")) {
                    int size = filter["size"];
                    var sized = artwork.Resize(new Size(size, size), ResizeMode.ContainAndShrink);
                    artwork.Dispose();
                    artwork = sized;
                }

                //if (filter.ContainsKey("artwork"))
                    Touchee.Meta.ArtworkColors.Generate(artwork);

                var stream = new MemoryStream();
                artwork.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                artwork.Dispose();
                stream.Seek(0, SeekOrigin.Begin);

                var resp = Response.FromStream(stream, "image/png");
                this.SetArtworkCache(resp);

                return resp;
            }

            else {
                var resp = Response.AsText("");
                resp.StatusCode = HttpStatusCode.NotFound;
                this.SetArtworkCache(resp);
                return resp;
            }
        }


        /// <summary>
        /// Sets the cache period for the artwork
        /// </summary>
        int artworkcacheDuration = -1;
        void SetArtworkCache(Response response) {
            int seconds = artworkcacheDuration;
            if (seconds < 0) {
                Program.Config.TryGetInt("artwork.cacheDuration", out seconds);
                artworkcacheDuration = Math.Max(seconds, 0);
            }
            response.Headers.Add("Cache-Control", "max-age=" + seconds.ToString());
        }


    }

}
