using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Touchee {

    /// <summary>
    /// Options object
    /// </summary>
    public class Options : Dictionary<string, OptionValue> {


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
                var parts = optionsString.Split('/');
                if (parts.Length % 2 == 0) {
                    for (var i = 0; i < parts.Length - 1; i += 2)
                        options[parts[i]] = new OptionValue( HttpUtility.UrlDecode(parts[i + 1]) );
                }

            }
            catch (Exception) {
                options.Clear();
            }

            return options;
        }

    }



    public class OptionValue {

        string _value;

        public OptionValue(string value) {
            this._value = value;
        }

        public override string ToString() {
            return this._value;
        }

        public static implicit operator int(OptionValue optionValue) {
            return int.Parse(optionValue.ToString());
        }

        public static implicit operator string(OptionValue optionValue) {
            return optionValue.ToString();
        }

    }



}

