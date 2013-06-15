using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

using Touchee.Components;
using Touchee.Components.Services;
using Touchee.Media.Music;

namespace Touchee.Artwork {

    /// <remarks>
    /// Artwork methods
    /// </remarks>
    public static class ArtworkHelper {


        /// <summary>
        /// Gets the unique key for the given item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string GetUniqueKey(IItem item) {
            string identifier = null;

            // Track
            if (item is ITrack) {
                var track = (ITrack)item;
                identifier = track.AlbumArtist + "=" + track.Album;
            }
            else
                identifier = item.UniqueKey;
            
            return identifier == null ? null : identifier.ToLower();
        }


        /// <summary>
        /// Gets the image from the cache corresponding to the given hash input string.
        /// </summary>
        /// <param name="uniqueKey">The unique key string of the image</param>
        /// <returns>An Image object if successfull, otherwise null</returns>
        public static ArtworkResult GetFromCache(string uniqueKey) {
            var result = new ArtworkResult();

            // Handle null value
            if (uniqueKey == null) return result;

            // Get artwork path
            var path = GetArtworkCachePath(uniqueKey);

            // If file exists, try to load image
            if (File.Exists(path)) {
                try {
                    result.Artwork = Image.FromFile(path);
                    result.Status = ArtworkStatus.Cached;
                    result.DateTime = File.GetCreationTime(path);
                }
                catch(Exception e) {
                    result.Artwork = null;
                    result.Status = ArtworkStatus.Error;
                    Logger.Log(String.Format("Could not load image from {0}: {1}", path, e.Message), Logger.LogLevel.Error);
                }
            }
            else
                result.Status = ArtworkStatus.Unavailable;

            // Return the image
            return result;
        }


        /// <summary>
        /// Stores the given image with the given hash input in the cache
        /// </summary>
        /// <param name="artwork">The image to save</param>
        /// <param name="uniqueKey">Unique key of the artwork</param>
        public static void SaveToCache(Image artwork, string uniqueKey) {

            // Get artwork path
            var path = GetArtworkCachePath(uniqueKey);
            
            // Create folder
            new FileInfo(path).Directory.Create();

            // Save to cache
            try {
                artwork.Save(path, System.Drawing.Imaging.ImageFormat.Png);
            }
            // Cannot save, probaly some other thread already saving to this path
            catch (System.Runtime.InteropServices.ExternalException) { }
        }


        /// <summary>
        /// Gets the full artwork cache path for the artwork with the given unqiue key
        /// </summary>
        /// <param name="uniqueKey"></param>
        /// <returns>A path string</returns>
        public static string GetArtworkCachePath(string uniqueKey) {
            var hash = uniqueKey.GetInt64HashCode();
            return Path.Combine(
                @"D:\Artwork",
                Convert.ToInt32(hash[15].ToString(), 16).ToString("D2"),
                Convert.ToInt32(hash[14].ToString(), 16).ToString("D2"),
                Convert.ToInt32(hash[13].ToString(), 16).ToString("D2"),
                String.Format("{0}.art", hash)
            );
        }


        /// <summary>
        /// Gets the default artwork for the given item
        /// </summary>
        /// <param name="item">The item for which the artwork should be retrieved</param>
        /// <returns>A filled ArtworkResult object</returns>
        public static ArtworkResult GetFromArtworkService(IItem item) {
            var result = new ArtworkResult();

            // Track: get album artwork from services until one is found
            if (item is ITrack)
                result = GetFromComponents<IAlbumArtworkService>(ArtworkType.Album, new object[] { (ITrack)item });

            // Set result object
            if (result.Type == ArtworkType.Unknown)
                result.Type = ArtworkHelper.GetDefaultArtworkType(item);
            return result;
        }


        /// <summary>
        /// Gets the default artwork for the given filter
        /// </summary>
        /// <param name="filter">The filter for which the artwork should be retrieved</param>
        /// <returns>A filled ArtworkResult object</returns>
        public static ArtworkResult GetFromArtworkService(Options filter) {
            var result = new ArtworkResult();

            // Artist: get artist or album
            if (filter.ContainsKey("artist")) {
                var artist = ((string)filter["artist"]).ToLower();

                // Also album: get album
                if (filter.ContainsKey("album"))
                    result = GetFromComponents<IAlbumArtworkService>(ArtworkType.Album, new object[] { artist, ((string)filter["album"]).ToLower() });

                // Get artist
                else
                    result = GetFromComponents<IArtistArtworkService>(ArtworkType.Artist, new object[] { artist });
            }

            // Set result object
            if (result.Type == ArtworkType.Unknown)
                result.Type = ArtworkHelper.GetDefaultArtworkType(filter);
            return result;
        }



        /// <summary>
        /// Gets artwork from the components of the given type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        static ArtworkResult GetFromComponents<T>(string type, object[] parameters) where T : IArtworkService {

            // Set vars
            var components = PluginManager.GetComponent<T>();
            var statuses = new List<ServiceResultStatus>();
            var args = new object[parameters.Length + 1];
            Array.Copy(parameters, args, parameters.Length);
            Image artwork = null;
            var imageType = typeof(Image).AssemblyQualifiedName.Replace("System.Drawing.Image", "System.Drawing.Image&");
            var types = args.Select(t => t == null ? Type.GetType(imageType) : t.GetType()).ToArray();

            // Loop through plugins
            foreach (var component in components) {

                // Get image from plugin
                var method = component.GetType().GetMethod("Get" + type.ToCamelCase() + "Artwork", types);
                if (method == null) continue;
                var status = (ServiceResultStatus)method.Invoke(component, args);
                
                // Check for corruption
                artwork = (Image)args[parameters.Length];
                if (artwork != null) {
                    using (var stream = new MemoryStream()) {
                        try {
                            artwork.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                        }
                        catch (Exception) {
                            artwork.Dispose();
                            args[parameters.Length] = artwork = null;
                            status = ServiceResultStatus.InvalidResponse;
                        }
                    }
                }
                
                // Add status
                statuses.Add(status);

                // Stop if we have an image
                if (artwork != null) break;
            }

            // Build and return result
            var result = new ArtworkResult();
            result.Artwork = artwork;
            result.DateTime = DateTime.Now;
            if (result.Artwork != null)
                result.Status = ArtworkStatus.Retrieved;
            else if (statuses.All(s => s == ServiceResultStatus.NoResult))
                result.Status = ArtworkStatus.Unavailable;
            else
                result.Status = ArtworkStatus.Error;
            return result;
        }



        public static string GetDefaultArtworkType(IItem item) {
            var type = ArtworkType.Unknown;

            if (item is ITrack)
                type = ArtworkType.Album;
            else if (item is IWebcast)
                type = ArtworkType.Webcast;

            return type;
        }
        



        public static string GetDefaultArtworkType(Options filter) {
            var type = ArtworkType.Unknown;

            if (filter.ContainsKey("artist"))
                type = filter.ContainsKey("album") ? ArtworkType.Album : ArtworkType.Artist;

            return type;
        }


    }



    public class ArtworkType {
        public const string Unknown = "unknown";
        public const string Album = "album";
        public const string Artist = "artist";
        public const string Webcast = "webcast";
    }

    public enum ArtworkStatus {
        Unknown,
        Error,
        Cached,
        Retrieved,
        Pending,
        Unavailable
    }


    public class ArtworkResult {

        public Image Artwork { get; set; }
        public string Type { get; set; }
        public ArtworkStatus Status { get; set; }
        public DateTime DateTime { get; set; }

        public ArtworkResult() {
            Type = ArtworkType.Unknown;
            Status = ArtworkStatus.Unknown;
            DateTime = DateTime.Now;
        }

    }

}
