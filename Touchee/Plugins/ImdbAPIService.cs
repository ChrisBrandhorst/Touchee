using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;
using System.Web;
using System.Web.Script.Serialization;
using System.Net;

using Touchee.Meta;
using Touchee.Artwork;
using Touchee.Components;
using Touchee.Components.Services;

namespace Touchee.Plugins {

    /// <summary>
    /// Service that uses the IMDbAPI service for downloading movie data and artwork
    /// </summary>
    public class ImdbAPIService : Base, IPlugin, IMovieArtworkService, ISeriesArtworkService, IMovieInfoService {

        #region Properties

        /// <summary>
        /// The IMDb API url that is used
        /// </summary>
        static string _url;

        /// <summary>
        /// Serializer used for reading JSON results
        /// </summary>
        static JavaScriptSerializer _serializer = new JavaScriptSerializer();

        #endregion


        #region IPlugin implementation

        /// <summary>
        /// The name of this plugin
        /// </summary>
        public string Name { get { return "IMDb API Service"; } }
        

        /// <summary>
        /// The description of this plugin
        /// </summary>
        public string Description { get { return "Retrieves info and artwork for amovies from the IMDB API."; } }


        /// <summary>
        /// The version of this plugin
        /// </summary>
        public Version Version { get { return new Version(1, 0, 0, 0); } }


        /// <summary>
        /// Starts this plugin
        /// </summary>
        /// <param name="config">The configuration section for this plugin</param>
        /// <param name="context">The context for this plugin</param>
        /// <returns>False if no valid URL was given in the config, otherwise true</returns>
        public bool StartPlugin(dynamic config, IPluginContext context) {

            // No config, no dice...
            if (config == null || config.GetType() != typeof(ConfigObject)) {
                Log("This plugin requires a ImdbAPIService config section with a url");
                return false;
            }

            // Get the value from the config
            config.TryGetString("url", out _url);

            // OK if we now have a url
            var ok = _url != null;

            // Register components if ok
            if (ok)
                PluginManager.Register((IComponent)this);

            return ok;
        }

        /// <summary>
        /// Stops this plugin
        /// </summary>
        /// <returns>True</returns>
        public bool StopPlugin() {
            PluginManager.Unregister((IComponent)this);
            return true;
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
            artwork = null;

            // Get data
            IMovieInfo movieInfo;
            var status = GetMovieInfo(title, year, out movieInfo);

            // Check status
            if (status != ServiceResultStatus.Success) return status;

            // No URL? No result
            if (movieInfo.PosterURL == null) return ServiceResultStatus.NoResult;

            // Download artwork
            artwork = Util.DownloadImage(movieInfo.PosterURL);

            // Download the image and return it
            return artwork == null ? ServiceResultStatus.TemporaryError : ServiceResultStatus.Success;
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
            return GetMovieArtwork(title + " series", out artwork);
        }

        #endregion


        #region IMovieInfoService implementation

        /// <summary>
        /// Gets the MovieInfo object for the given movie title
        /// </summary>
        /// <param name="title">The title of the movie</param>
        /// <param name="year">The year of the movie</param>
        /// <param name="movieInfo">The reference object to store the resulting MovieInfo object in</param>
        /// <returns>The result status code for this service call</returns>
        public ServiceResultStatus GetMovieInfo(string title, out IMovieInfo movieInfo) {
            return GetMovieInfo(title, 0, out movieInfo);
        }

        /// <summary>
        /// Gets the MovieInfo object for the given movie title and year
        /// </summary>
        /// <param name="title">The title of the movie</param>
        /// <param name="year">The year of the movie</param>
        /// <param name="movieInfo">The reference object to store the resulting MovieInfo object in</param>
        /// <returns>The result status code for this service call</returns>
        public ServiceResultStatus GetMovieInfo(string title, int year, out IMovieInfo movieInfo) {
            movieInfo = null;

            // Build URL
            var url = _url
                .Replace("%title", HttpUtility.UrlEncode(title))
                .Replace("%year", year > 0 ? year.ToString() : "");

            // Get info
            MovieInfo mi;
            var status = LoadMovieData(url, out mi);

            // Check status
            if (status != ServiceResultStatus.Success) return status;

            // All ok!
            movieInfo = (IMovieInfo)mi;
            return status;
        }

        #endregion


        /// <summary>
        /// Downloads the JSON from the given URI and parses this into a dictionary
        /// </summary>
        /// <param name="url">The URI to download</param>
        /// <param name="movieInfo">The reference object to store the resulting MovieInfo object in</param>
        /// <returns>The result status code for this service call</returns>
        ServiceResultStatus LoadMovieData(string url, out MovieInfo movieInfo) {
            movieInfo = null;

            // Download JSON
            var client = new WebClient();
            string json;
            try {
                json = client.DownloadString(url);
            }
            catch (System.Net.WebException e) {
                Log("Could not load JSON: " + e.Message);
                return ServiceResultStatus.TemporaryError;
            }

            // Parse JSON and build object
            Dictionary<string, object> data;
            try {
                data = _serializer.DeserializeObject(json) as Dictionary<string, object>;
            }
            catch(Exception e) {
                Log("Could not parse JSON: " + e.Message);
                return ServiceResultStatus.InvalidResponse;
            }

            // Check response
            if (data["Response"].ToString().ToLower() == "false") {
                Log(data["Error"].ToString());
                return ServiceResultStatus.NoResult;
            }

            // Build object
            try {
                movieInfo = MovieInfo.Build(data);
                return ServiceResultStatus.Success;
            }
            catch (Exception) {
                Log("Could not build MovieInfo object");
                return ServiceResultStatus.InvalidResponse;
            }
        }

        /// <summary>
        /// Contains movie info
        /// </summary>
        public class MovieInfo : IMovieInfo {

            public string Actors { get; protected set; }
            public string Director { get; protected set; }
            public TimeSpan Duration { get; protected set; }
            public string Genre { get; protected set; }
            public string ImdbID { get; protected set; }
            public float ImdbRating { get; protected set; }
            public int ImdbVotes { get; protected set; }
            public string Plot { get; protected set; }
            public string PosterURL { get; protected set; }
            public string Rating { get; protected set; }
            public DateTime ReleaseDate { get; protected set; }
            public string Title { get; protected set; }
            public string Writer { get; protected set; }

            public static MovieInfo Build(Dictionary<string, object> data) {
                var culture = System.Globalization.CultureInfo.InvariantCulture;
                return new MovieInfo() {
                    Actors      = (string)data["Actors"],
                    Director    = (string)data["Director"],
                    Duration    = TimeSpan.ParseExact((string)data["Runtime"],  @"%h' h '%m' min'", culture),
                    Genre       = (string)data["Genre"],
                    ImdbID      = (string)data["imdbID"],
                    ImdbRating  = float.Parse((string)(data["imdbRating"] ?? "-1"), culture),
                    ImdbVotes   = int.Parse((string)data["imdbVotes"], System.Globalization.NumberStyles.AllowThousands, culture),
                    Plot        = (string)data["Plot"],
                    PosterURL   = (string)data["Poster"],
                    Rating      = (string)data["Rated"],
                    ReleaseDate = DateTime.ParseExact((string)data["Released"], @"dd MMM yyyy", culture),
                    Title       = (string)data["Title"],
                    Writer      = (string)data["Writer"]
                };
            }

        }

    }


}
