using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Touchee {

    /// <summary>
    /// Options object
    /// </summary>
    public class Options : Dictionary<string, string> {


        /// <summary>
        /// Builds a options object from the given string
        /// </summary>
        /// <param name="optionsString">The options string to build the filter object from</param>
        /// <returns>A options object</returns>
        public static Options Build(string optionsString) {
            var options = new Options();

            if (String.IsNullOrWhiteSpace(optionsString))
                return options;

            try {
                options = new Options(
                    Regex
                        .Split(optionsString, "(?<!\\\\),")
                        .Select(o =>
                            o.Split(new char[]{':'}, 2)
                         )
                        .ToDictionary(o =>
                            o[0].ToLower(), o => Regex.Unescape(o[1])
                        )
                );
            }
            catch (Exception) { }

            return options;
        }

        Options() : base() { }
        public Options(Dictionary<string, string> options) : base(options) { }


        public int TryGetInt(string key, out int value) {
            value = 0;
            if (this.ContainsKey(key))
                Int32.TryParse(this[key], out value);
            return value;
        }

        public int GetInt(string key) {
            var value = 0;
            this.TryGetInt(key, out value);
            return value;
        }


    }

}

