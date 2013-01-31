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

            //if (itemID > 0) {
                
            //}

            // Get query object
            DynamicDictionary query = Request.Query;


            return null;

            //// Get container
            //var container = GetContainerFromParams(parameters);
            //if (container == null) return null;

            //// Get query object
            //DynamicDictionary query = Request.Query;
            
            //// Get requested artwork size
            //ArtworkSize size;
            //Enum.TryParse<ArtworkSize>(Request.Query.size, true, out size);
            //int width = query.Get<int>("width");
            //int height = query.Get<int>("height");
            //int ratio = query.Get<int>("ratio", 1);
            //ResizeMode resizeMode;
            //Enum.TryParse<ResizeMode>(Request.Query.resizeMode, true, out resizeMode);

            //// No params? Go for default
            //if ((int)size == 0 && width + height == 0)
            //    size = ArtworkSize.Medium;

            //// No enum size available? Get from width and height
            //if ((int)size == 0) {
            //    size = ArtworkSize.Small;
            //    if (width > (int)size || height > (int)size)
            //        size = ArtworkSize.Medium;
            //    if (width > (int)size || height > (int)size)
            //        size = ArtworkSize.Large;
            //}

            //// Go from enum to int
            //else
            //    width = height = (int)size;

            //// Apply ratio
            //width *= ratio;
            //height *= ratio;

            //// Get the itemID if it exists
            //int itemID = query.Get<int>("item");

            //// Vars
            //Options filter = null;
            //IItem item = null;
            //ArtworkResult result = null;
            //bool setCache = false;

            //// Get the artwork based on the input
            //if (itemID > 0) {
            //    item = container.GetItem(itemID);
            //    result = Library.Artwork(container, item, Client, Request.Url);
            //}
            //else {
            //    filter = Touchee.Options.Build(Request.Query.item.Value);
            //    if (filter.Count > 0)
            //        result = Library.Artwork(container, filter, Client, Request.Url);
            //    else
            //        return null;
            //}

            //// Set source artwork (before resize);
            //var sourceArtwork = result.Artwork;

            //// Unknown result? Give 404
            //if (result.Status == ArtworkStatus.Unknown) {
            //    sourceArtwork = null; // Just to be sure
            //}

            //// Else, give default image if we have no artwork (yet)
            //else if (sourceArtwork == null) {

            //    // Get some vars
            //    var unavailable = result.Status == ArtworkStatus.Unavailable;
            //    var type = result.Type;
            //    var imagePath = String.Format("/web/app/assets/images/artwork/{0}/{1}.png", size.ToString().ToLower(), type);

            //    // We actually just want to do a redirect here, but Webkit browsers seem to cache 307 responses (which they shouldn't accoring to HTTP spec)
            //    //Response.Status = unavailable ? System.Net.HttpStatusCode.MovedPermanently : System.Net.HttpStatusCode.TemporaryRedirect;
            //    //Response.AddHeader("Location", Request.Uri.GetLeftPart(UriPartial.Authority) + "/app/assets/images/artwork/" + size + "/" + typeStr + ".png");

            //    // So instead we just present the default image
            //    try {
            //        sourceArtwork = new Bitmap(Response.RootPath + imagePath, true);
            //    }
            //    catch (Exception) {
            //        sourceArtwork = null;
            //    }

            //    // Set cache if artwork is unavailable
            //    if (unavailable)
            //        setCache = true;
                
            //}

            //// Else, set cache if artwork was found
            //else {
            //    setCache = true;
            //}

            //// We can serve an image
            //Response resp = null;
            //if (sourceArtwork != null) {
            //    Image targetArtwork;

            //    // Resize artwork if requested
            //    if (width == 0 ^ height == 0) {
            //        var d = width == 0 ? height : width;
            //        targetArtwork = sourceArtwork.Resize(new Size(d, d), resizeMode);
            //        sourceArtwork.Dispose();
            //    }
            //    else if (width > 0 && height > 0) {
            //        targetArtwork = sourceArtwork.Resize(new Size(width, height), resizeMode);
            //        sourceArtwork.Dispose();
            //    }
            //    else
            //        targetArtwork = sourceArtwork;

            //    // Output artwork
            //    var stream = new MemoryStream();
            //    targetArtwork.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            //    targetArtwork.Dispose();
            //    stream.Seek(0, SeekOrigin.Begin);
            //    resp = Response.FromStream(stream, "image/png");

            //    // Set cache if requested
            //    if (setCache)
            //        SetArtworkCache(resp);
            //}
            
            //return resp;
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
