using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;
using System.Xml.Linq;
using System.Net;
using System.Xml.XPath;

namespace Touchee.Service {

    /// <summary>
    /// Service using Last.FM API for retrieving album and artist artwork
    /// </summary>
    public class LastFMService : Base, IPlugin, IAlbumArtworkService, IArtistArtworkService {


        #region Properties

        /// <summary>
        /// The Last.FM API url that is used
        /// </summary>
        static string _url;

        /// <summary>
        /// Last.FM API key
        /// </summary>
        static string _key;

        /// <summary>
        /// Last.FM API secret
        /// </summary>
        static string _secret;

        #endregion


        #region IPlugin implementation

        /// <summary>
        /// The name of this plugin
        /// </summary>
        public string Name { get { return "Last.FM API Artwork Service"; } }

        /// <summary>
        /// Starts this plugin
        /// </summary>
        /// <param name="config">The configuration section for this plugin</param>
        /// <returns>False if no valid URL, key or secret was given in the config, otherwise true</returns>
        public bool Start(dynamic config) {

            // No config, no dice...
            if (config == null || config.GetType() != typeof(ConfigObject)) {
                Log("This plugin requires a LastFMService config section with a url, key and secret setting");
                return false;
            }

            // Get the values from the config
            config.TryGetString("url", out _url);
            config.TryGetString("key", out _key);
            config.TryGetString("secret", out _secret);

            // Collect errors
            var errors = new List<string>();
            if (String.IsNullOrWhiteSpace(_url))
                errors.Add("No url given");
            if (String.IsNullOrWhiteSpace(_key))
                errors.Add("No key given");
            if (String.IsNullOrWhiteSpace(_secret))
                errors.Add("No secret given");
            
            // Return
            if (errors.Count() > 0) {
                Log("Wrong LastFMService config: " + String.Join(", ", errors));
                return false;
            }
            else
                return true;
        }

        /// <summary>
        /// Stops this plugin
        /// </summary>
        /// <returns>True</returns>
        public bool Shutdown() {
            return true;
        }

        #endregion


        #region IAlbumArtworkService implementation

        /// <summary>
        /// Gets an image for the album of the given track
        /// </summary>
        /// <param name="track">A track of the requested album</param>
        /// <param name="artwork">The reference object to store the resulting artwork in</param>
        /// <returns>The result status code for this service call</returns>
        public ServiceResultStatus GetAlbumArtwork(ITrack track, out Image artwork) {
            return GetAlbumArtwork(track.Artist, track.Album, out artwork);
        }

        /// <summary>
        /// Gets an image for the given album
        /// </summary>
        /// <param name="artist">The artist of the album</param>
        /// <param name="title">The title of the album</param>
        /// <param name="artwork">The reference object to store the resulting artwork in</param>
        /// <returns>The result status code for this service call</returns>
        public ServiceResultStatus GetAlbumArtwork(string artist, string title, out Image artwork) {
            artwork = null;
            var queryParameters = new Dictionary<string, string>();
            queryParameters["artist"] = artist;
            queryParameters["album"] = title;

            // Get XML
            XPathNavigator xml;
            var status = Get("album.getinfo", queryParameters, out xml);

            // No success? Bail out
            if (status != ServiceResultStatus.Success) return status;

            // Check if we have an error and process it
            var error = GetErrorInfo(xml);
            if (error != null) {
                if (error.Item2 != null && error.Item2.ToLower() == "album not found")
                    return ServiceResultStatus.NoResult;
                else
                    return TranslateErrorCode(error.Item1);
            }

            // Get the image
            var imageNode = xml.SelectSingleNode("//image[@size='mega']/text()");
            if (imageNode == null) return ServiceResultStatus.NoResult;
            artwork = Util.DownloadImage(imageNode.Value);

            // Return the artwork
            return artwork == null ? ServiceResultStatus.TemporaryError : ServiceResultStatus.Success;
        }

        #endregion


        #region IArtistArtworkService implementation

        /// Gets an image for the artist of the given track
        /// </summary>
        /// <param name="track">A track of the requested artist</param>
        /// <returns>An Image object containing the artwork, or null if none found</returns>
        public ServiceResultStatus GetArtistArtwork(ITrack track, out Image artwork) {
            return GetArtistArtwork(track.Artist, out artwork);
        }

        /// <summary>
        /// Gets an image for the given artist
        /// </summary>
        /// <param name="artist">The artist</param>
        /// <returns>An Image object containing the artist picture, or null if none found</returns>
        public ServiceResultStatus GetArtistArtwork(string artist, out Image artwork) {
            artwork = null;

            // Set parameters
            var queryParameters = new Dictionary<string, string>();
            queryParameters["artist"] = artist;
            queryParameters["limit"] = "1";
            queryParameters["order"] = "popularity";

            // Get XML
            XPathNavigator xml;
            var status = Get("artist.getimages", queryParameters, out xml);

            // No success? Bail out
            if (status != ServiceResultStatus.Success) return status;

            // Check if we have an error and process it
            var error = GetErrorInfo(xml);
            if (error != null) {
                if (error.Item2 != null && error.Item2.ToLower() == "the artist you supplied could not be found")
                    return ServiceResultStatus.NoResult;
                else
                    return TranslateErrorCode(error.Item1);
            }

            // Get the image
            var imageNode = xml.SelectSingleNode("//image/sizes/size[@name='largesquare']/text()");
            if (imageNode == null) return ServiceResultStatus.NoResult;
            artwork = Util.DownloadImage(imageNode.Value);

            // Return the artwork
            return artwork == null ? ServiceResultStatus.TemporaryError : ServiceResultStatus.Success;
        }

        #endregion


        #region Download helpers

        /// <summary>
        /// Gets an XML fragment from the Last.FM API for the given parameters
        /// </summary>
        /// <param name="method">The API method to call</param>
        /// <param name="queryParameters">Parameters given to the API</param>
        /// <param name="navigator">Reference parameter which is filled with the XML document when no error has occured</param>
        /// <returns>The resulting artwork status status code</returns>
        ServiceResultStatus Get(string method, Dictionary<string, string> queryParameters, out XPathNavigator navigator) {
            navigator = null;

            // Set some query parameters
            queryParameters["method"] = method;
            queryParameters["api_key"] = _key;

            // Build querystring and url
            string[] query = new string[queryParameters.Count];
            int i = 0;
            foreach (KeyValuePair<string, string> kv in queryParameters) {
                query[i] = kv.Key + "=" + System.Web.HttpUtility.UrlEncode(kv.Value);
                i++;
            }
            string url = _url + "?" + String.Join("&", query);

            // Setup vars
            XPathDocument lastFMDoc = null;
            var req = WebRequest.Create(url);
            //req.Timeout = 5000;
            WebResponse resp = null;

            // Do request
            try { resp = req.GetResponse(); }
            catch (WebException e) {
                if (e.Response == null)
                    return ServiceResultStatus.TemporaryError;
                else
                    resp = e.Response;
            }

            // If no XML, bail out
            if (!resp.ContentType.ToLower().Contains("text/xml")) {
                Log("Last.FM API Response is not XML");
                return ServiceResultStatus.InvalidResponse;
            }

            // Parse XML
            try {
                lastFMDoc = new XPathDocument(resp.GetResponseStream());
                navigator = lastFMDoc.CreateNavigator();
                return ServiceResultStatus.Success;
            }
            catch (System.Xml.XmlException e) {
                Log("Could not parse an XML response from Last.FM API: " + e.Message);
                return ServiceResultStatus.InvalidResponse;
            }
        }


        /// <summary>
        /// Returns the error code and message in the given xml fragment, if available
        /// </summary>
        /// <param name="xml">The XML navigator to search in</param>
        /// <returns>The error code and message tuple if they exists, otherwise null</returns>
        Tuple<ErrorCode, string> GetErrorInfo(XPathNavigator navigator) {

            // Check for error
            var errorNode = navigator.SelectSingleNode("//error");
            if (errorNode == null) return null;

            Tuple<ErrorCode, string> error = null;

            try {
                error = new Tuple<ErrorCode, string>(
                    (ErrorCode)Enum.ToObject(typeof(ErrorCode), errorNode.SelectSingleNode("@code").ValueAsInt),
                    errorNode.Value
                );
            }
            catch (Exception) {
                Log("Invalid error response from Last.FM: " + errorNode.OuterXml);
            }

            return error;
        }


        /// <summary>
        /// Translates the Last.FM error code to a ArtworkServiceResultStatus
        /// </summary>
        /// <param name="errorCode">The error code to translate</param>
        /// <returns>The resulting ArtworkServiceResultStatus</returns>
        ServiceResultStatus TranslateErrorCode(ErrorCode errorCode) {
            var status = ServiceResultStatus.UnknownError;
            switch (errorCode) {
                case ErrorCode.None: status = ServiceResultStatus.Success; break;
                case ErrorCode.InvalidService: status = ServiceResultStatus.ClientError; break;
                case ErrorCode.InvalidMethod: status = ServiceResultStatus.ClientError; break;
                case ErrorCode.AuthenticationFailed: status = ServiceResultStatus.AuthenticationFailed; break;
                case ErrorCode.InvalidFormat: status = ServiceResultStatus.ClientError; break;
                case ErrorCode.InvalidParameters: status = ServiceResultStatus.ClientError; break;
                case ErrorCode.InvalidResourceSpecified: status = ServiceResultStatus.ClientError; break;
                case ErrorCode.InvalidSessionKey: status = ServiceResultStatus.AuthenticationFailed; break;
                case ErrorCode.InvalidAPIKey: status = ServiceResultStatus.AuthenticationFailed; break;
                case ErrorCode.ServiceOffline: status = ServiceResultStatus.ServiceOffline; break;
                case ErrorCode.InvalidMethodSignature: status = ServiceResultStatus.ClientError; break;
                case ErrorCode.TemporaryError: status = ServiceResultStatus.TemporaryError; break;
                case ErrorCode.SuspendedAPIKey: status = ServiceResultStatus.AuthenticationFailed; break;
                case ErrorCode.RateLimitExceeded: status = ServiceResultStatus.Throttled; break;
            }
            return status;
        }

        #endregion


        #region LastFM error codes

        /// <remarks>
        /// Last.FM error codes
        /// </remarks>
        public enum ErrorCode {
            None = 1,
            InvalidService = 2,
            InvalidMethod = 3,
            AuthenticationFailed = 4,
            InvalidFormat = 5,
            InvalidParameters = 6,
            InvalidResourceSpecified = 7,
            OperationFailed = 8,
            InvalidSessionKey = 9,
            InvalidAPIKey = 10,
            ServiceOffline = 11,
            InvalidMethodSignature = 13,
            TemporaryError = 16,
            SuspendedAPIKey = 26,
            RateLimitExceeded = 27
        }

        #endregion

    }

}
