using System;
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
            Get["/{containerID}"] = _ => GetContents(_);
            Get["/{containerID}/{filter}"] = _ => GetContents(_);
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
                new ContainersResponse(medium)
            );
        }


        /// <summary>
        /// Gets the contents of the given container
        /// </summary>
        public Response GetContents(dynamic parameters) {
            if (Container == null) return null;
            ContentsResponse contents = Library.GetContentsResponse(Container, Filter);
            return Response.AsJson(contents);
        }


        /// <summary>
        /// Gets artwork
        /// </summary>
        public Response GetArtwork(dynamic parameters) {
            if (Container == null) return null;

            Image artwork = Library.GetArtwork(Container, Filter);

            // Output artwork
            if (artwork != null) {

                // Size artwork
                // TODO: check if size is really an int
                if (Request.Query.ContainsKey("size")) {
                    int size = Request.Query["size"];
                    var sized = artwork.Resize(new Size(size, size), ResizeMode.ContainAndShrink);
                    artwork.Dispose();
                    artwork = sized;
                }

                // Save to stream
                var stream = new MemoryStream();
                artwork.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Seek(0, SeekOrigin.Begin);

                Response response;

                // Check if colors should be added
                if (Request.Query.ContainsKey("colors")) {
                    var colors = Touchee.Meta.ArtworkColors.Generate(artwork);
                    response = Response.AsJson<Touchee.Meta.ArtworkColors>(colors);
                }

                else {
                    // Create response
                    response = Response.FromStream(stream, "image/png");
                    this.SetArtworkCache(response);
                }

                // Throw away artwork
                artwork.Dispose();
                return response;
            }

            else {
                var response = Response.AsText("");
                response.StatusCode = HttpStatusCode.NotFound;
                this.SetArtworkCache(response);
                return response;
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
