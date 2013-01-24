using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

using HtmlAgilityPack;

namespace Touchee {

    /// <remarks>
    /// Utility methods
    /// </remarks>
    public static class Util {


        /// <summary>
        /// Creates an instance of each available implementation of the specified interface type
        /// </summary>
        /// <typeparam name="T">The interface type</typeparam>
        /// <returns>A list of instances</returns>
        public static List<T> InstantiateAllImplementations<T>() {
            var iType = typeof(T);
            var types = AppDomain.CurrentDomain.GetAssemblies().ToList()
                .SelectMany(s => s.GetTypes())
                .Where(p => iType.IsAssignableFrom(p) && p != iType);
            List<T> instances = new List<T>();
            foreach (var type in types) {
                var instance = (T)Activator.CreateInstance(type);
                instances.Add(instance);
            }
            return instances;
        }


        /// <summary>
        /// Returns whether the first character of the given string is an alpha character (a-z)
        /// </summary>
        /// <param name="input">The string to test</param>
        /// <returns>True if the first character of the string is an alpha character</returns>
        public static bool FirstIsAlpha(string input) {
            if (String.IsNullOrWhiteSpace(input))
                return false;
            else
                return input.FirstIsAlpha();
        }


        /// <summary>
        /// Checkes whether the two strings have an equal value
        /// </summary>
        /// <param name="value1">The first string to compare</param>
        /// <param name="value2">The second string to compare</param>
        /// <param name="matchCase">Whether the comparison should be done case sensitive</param>
        /// <returns>True if the two strings are equal, otherwise false</returns>
        public static bool Equals(string value1, string value2, bool matchCase = false) {
            if (!matchCase) {
                if (value1 != null) value1 = value1.ToLower();
                if (value2 != null) value2 = value2.ToLower();
            }
            return String.Equals(value1, value2);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string GetIndex(string input) {
            return Util.FirstIsAlpha(input) ? input.First().ToString().ToUpper() : "#";
        }



        public const string DecimalSortValue = "{";
        public const string NullSortValue = "|";


        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToSortName(string input) {

            if (String.IsNullOrWhiteSpace(input))
                return NullSortValue;

            else {
                var sortName = input.ToLower().StripPrefixes().StripDiacritics();
                sortName = Regex.Replace(sortName, @"[^a-z0-9]", "");
                if (Regex.IsMatch(sortName, @"^\d"))
                    sortName = DecimalSortValue + sortName;
                    //sortName = ((char)(Char.MaxValue - 1)).ToString() + sortName;
                sortName = Regex.Replace(sortName, @"(\d+)", m =>
                    (
                        (char)(
                            Math.Min(
                                Int32.Parse(m.Value),
                                Char.MaxValue - 1
                            )
                        )
                    ).ToString()
                );
                return sortName;
            }
        }



        /// <summary>
        /// Downloads an image from the given URL
        /// </summary>
        /// <param name="url">The URL to download the image from</param>
        /// <returns>An Image object, or null if no valid image could be downloaded</returns>
        public static Image DownloadImage(string uri) {
            Image image = null;

            // Get the image
            WebClient client = new WebClient();
            try {
                var stream = client.OpenRead(uri);
                image = Image.FromStream(stream);
            }
            catch (Exception) { }

            return image;
        }


        /// <summary>
        /// Downloads the HTML document from the given uri
        /// </summary>
        /// <param name="uri">The uri to download</param>
        /// <returns>The HTMLDocument</returns>
        public static HtmlDocument DownloadHTMLDocument(string uri) {
            HtmlDocument doc = null;
            try {
                doc = new HtmlWeb().Load(uri);
            }
            catch (Exception e) {
                Logger.Log("Could not load HtmlDocument: " + e.Message, Logger.LogLevel.Error);
            }
            return doc;
        }

        
    }
}
