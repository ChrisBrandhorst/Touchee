using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;
using System.Net;
using System.Web;
using System.IO;

using HtmlAgilityPack;

using Touchee.Components;
using Touchee.Components.Services;
using Touchee.Media.Music;

namespace Touchee.Plugins {

    /// <summary>
    /// Service that uses the Google Images website to get artwork for albums, artists, movies and series
    /// </summary>
    public class GoogleArtworkService : Base, IPlugin, IAlbumArtworkService, IArtistArtworkService, IMovieArtworkService, ISeriesArtworkService, ICustomArtworkService {


        #region Search options

        /// <remarks>
        /// Search options
        /// </remarks>
        public class SearchOptions {
            /// <summary>
            /// Portrait aspect ratio search string
            /// </summary>
            public const string AspectRatioPortrait = "iar:t";

            /// <summary>
            /// Square aspect ratio search string
            /// </summary>
            public const string AspectRatioSquare = "iar:s";

            /// <summary>
            /// Wide aspect ratio search string
            /// </summary>
            public const string AspectRatioWide = "iar:w";

            /// <summary>
            /// Minimum size search string
            /// </summary>
            public const string MinSize = "isz:lt,islt:qsvga";

            /// <summary>
            /// Search for clipart only
            /// </summary>
            public const string ClipArt = "itp:clipart";

        }

        #endregion


        #region Constants

        /// <summary>
        /// The maximum number of image results that are checked per request
        /// </summary>
        public const int NumberOfImagesToCheck = 3;

        #endregion


        #region Properties

        /// <summary>
        /// The Google Images url that is used
        /// </summary>
        private static string _url;

        #endregion


        #region IPlugin implementation


        /// <summary>
        /// The name of this plugin
        /// </summary>
        public string Name { get { return "Google Images"; } }


        /// <summary>
        /// The description of this plugin
        /// </summary>
        public string Description { get { return "Retrieves album, artist, movie, series and custom artwork from Google Images."; } }


        /// <summary>
        /// The version of this plugin
        /// </summary>
        public Version Version { get { return new Version(1, 0, 0, 0); } }


        /// <summary>
        /// Starts this plugin
        /// </summary>
        /// <param name="config">The configuration section for this plugin</param>
        /// <returns>False if no valid URL was given in the config, otherwise true</returns>
        public bool StartPlugin(dynamic config) {
            
            // No config, no dice...
            if (config == null || config.GetType() != typeof(ConfigObject)) {
                Log("This plugin requires a GoogleArtworkService config section with a url");
                return false;
            }

            // Get the value from the config
            config.TryGetString("url", out _url);

            // OK if we now have a url
            var ok = _url != null;

            // Register components if ok
            if (ok)
                PluginManager.Register((IArtworkService)this);

            return ok;
        }

        /// <summary>
        /// Stops this plugin
        /// </summary>
        /// <returns>True</returns>
        public bool StopPlugin() {
            PluginManager.Unregister((IArtworkService)this);
            return true;
        }

        #endregion


        #region IAlbumArtworkService implementation

        /// Gets an image for the album of the given track
        /// </summary>
        /// <param name="track">A track of the requested album</param>
        /// <param name="artwork">The reference object to store the resulting artwork in</param>
        /// <returns>The result status code for this service call</returns>
        public ServiceResultStatus GetAlbumArtwork(ITrack track, out Image artwork) {
            return GetAlbumArtwork(track.FirstArtist, track.Album, out artwork);
        }

        /// <summary>
        /// Gets an image for the given album
        /// </summary>
        /// <param name="artist">The artist of the album</param>
        /// <param name="title">The title of the album</param>
        /// <param name="artwork">The reference object to store the resulting artwork in</param>
        /// <returns>The result status code for this service call</returns>
        public ServiceResultStatus GetAlbumArtwork(string artist, string title, out Image artwork) {
            return DownloadArtwork("\"" + artist + "\"", title, "", "", new string[]{ SearchOptions.MinSize, SearchOptions.AspectRatioSquare }, out artwork);
        }

        #endregion


        #region IArtistArtworkService implementation

        /// Gets an image for the artist of the given track
        /// </summary>
        /// <param name="track">A track of the requested artist</param>
        /// <param name="artwork">The reference object to store the resulting artwork in</param>
        /// <returns>The result status code for this service call</returns>
        public ServiceResultStatus GetArtistArtwork(ITrack track, out Image artwork) {
            return GetArtistArtwork(track.FirstArtist, out artwork);
        }

        /// <summary>
        /// Gets an image for the given artist
        /// </summary>
        /// <param name="artist">The artist</param>
        /// <param name="artwork">The reference object to store the resulting artwork in</param>
        /// <returns>The result status code for this service call</returns>
        public ServiceResultStatus GetArtistArtwork(string artist, out Image artwork) {
            return DownloadArtwork("", artist, "", "", new string[]{ SearchOptions.MinSize }, out artwork);
        }

        #endregion


        #region IMovieArtworkService implementation

        /// <summary>
        /// Gets a cover image for the given movie
        /// </summary>
        /// <param name="title">The title of the movie</param>
        /// <param name="artwork">The reference object to store the resulting artwork in</param>
        /// <returns>The result status code for this service call</returns>
        public ServiceResultStatus GetMovieArtwork(string title, out Image artwork) {
            return GetMovieArtwork(title, 0, out artwork);
        }

        /// <summary>
        /// Gets a cover image for the given movie
        /// </summary>
        /// <param name="title">The title of the movie</param>
        /// <param name="year">The year of the movie</param>
        /// <param name="artwork">The reference object to store the resulting artwork in</param>
        /// <returns>The result status code for this service call</returns>
        public ServiceResultStatus GetMovieArtwork(string title, int year, out Image artwork) {
            return DownloadArtwork(year > 0 ? year.ToString() : "", title, "cover poster movie imdb", "", new string[] { SearchOptions.MinSize, SearchOptions.AspectRatioPortrait }, out artwork);
        }

        #endregion


        #region ISeriesArtworkService implementation

        /// <summary>
        /// Gets a cover image for the given series
        /// </summary>
        /// <param name="title">The title of the series</param>
        /// <param name="artwork">The reference object to store the resulting artwork in</param>
        /// <returns>The result status code for this service call</returns>
        public ServiceResultStatus GetSeriesArtwork(string title, out Image artwork) {
            return GetSeriesArtwork(title, "", out artwork);
        }

        /// <summary>
        /// Gets a cover image for the given series
        /// </summary>
        /// <param name="title">The title of the series</param>
        /// <param name="season">The season of the series</param>
        /// <param name="artwork">The reference object to store the resulting artwork in</param>
        /// <returns>The result status code for this service call</returns>
        public ServiceResultStatus GetSeriesArtwork(string title, string season, out Image artwork) {
            return DownloadArtwork("\"" + season + "\"", title, "cover poster series imdb", "", new string[] { SearchOptions.MinSize, SearchOptions.AspectRatioPortrait }, out artwork);
        }

        #endregion


        #region ICustomArtworkService implementation

        /// <summary>
        /// Gets an image corresponding to the given query
        /// </summary>
        /// <param name="query">Image query</param>
        /// <returns>The result status code for this service call</returns>
        public ServiceResultStatus GetCustomArtwork(string query, out Image artwork) {
            return DownloadArtwork(query, null, null, null, new string[] { SearchOptions.ClipArt }, out artwork);
        }

        #endregion


        #region Download helpers

        /// <summary>
        /// Gets a single image for the given parameters, using the Google Image Search website
        /// </summary>
        /// <param name="allPart">Part of the query for which all words must exist in the result</param>
        /// <param name="exactPart">Part of the query for which the exact phrase must exist in the result</param>
        /// <param name="anyPart">Part of the query for which any word must exist in the result</param>
        /// <param name="nonePart">Part of the query for which no words may exist in the result</param>
        /// <param name="sizeOptions">Size options string</param>
        /// <param name="artwork">The reference object to store the resulting artwork in</param>
        /// <returns>The result status code for this service call</returns>
        ServiceResultStatus DownloadArtwork(string allPart, string exactPart, string anyPart, string nonePart, string[] options, out Image artwork) {
            artwork = null;

            // Get image URIs
            var imageURLs = GetImageURLs(allPart, exactPart, anyPart, nonePart, options, NumberOfImagesToCheck);

            // Could not get results, service offline
            if (imageURLs == null) return ServiceResultStatus.ServiceOffline;

            // If we have no URls, we have no results
            if (imageURLs.Length == 0) return ServiceResultStatus.NoResult;

            // Get each image until we have a working one
            WebClient client = new WebClient();
            foreach (var url in imageURLs) {
                artwork = Util.DownloadImage(url);
                if (artwork != null) break;
            }

            return artwork == null ? ServiceResultStatus.NoResult : ServiceResultStatus.Success;
        }

        /// <summary>
        /// Gets all image URLs for the given parameters from the Google Image Search website
        /// </summary>
        /// <param name="allPart">Part of the query for which all words must exist in the result</param>
        /// <param name="exactPart">Part of the query for which the exact phrase must exist in the result</param>
        /// <param name="anyPart">Part of the query for which any word must exist in the result</param>
        /// <param name="nonePart">Part of the query for which no words may exist in the result</param>
        /// <param name="sizeOptions">Size options string</param>
        /// <param name="max">The maximum number of image URIs to retrieve</param>
        /// <returns>A string array containing matching image URLs</returns>
        string[] GetImageURLs(string allPart, string exactPart, string anyPart, string nonePart, string[] options, int max) {

            // Build URI
            var url = _url
                .Replace("%all", HttpUtility.UrlEncode(allPart ?? ""))
                .Replace("%exact", HttpUtility.UrlEncode(exactPart ?? ""))
                .Replace("%any", HttpUtility.UrlEncode(anyPart ?? ""))
                .Replace("%none", HttpUtility.UrlEncode(nonePart ?? ""))
                .Replace("%options", String.Join(",", options ?? new string[0]));

            // Set valid max
            max = Math.Min(max, 1);

            // Get document
            HtmlDocument doc = Util.DownloadHTMLDocument(url);
            if (doc == null) return null;

            // Get the nodes containing the links to the full images
            HtmlNodeCollection linkNodes = doc.DocumentNode.SelectNodes("//*[@id='ires']//a");

            // Results array
            var imageURIs = new List<string>();

            // Loop through max max nodes if we have results
            if (linkNodes != null) {
                string href, querystring;
                for (int i = 0; i < Math.Min(max, linkNodes.Count); i++) {
                    // Get href attribute, which contains the image url
                    href = linkNodes[i].Attributes["href"].Value;
                    // Get the querystring part of the href
                    querystring = href.Replace("/imgres?", "");
                    imageURIs.Add(HttpUtility.ParseQueryString(querystring).Get("imgurl"));
                }
            }

            return imageURIs.ToArray();
        }

        #endregion



    }

}
