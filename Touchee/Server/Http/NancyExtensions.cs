using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Touchee.Server.Http {

    public static class NancyExtensions {

        /// <summary>
        /// Gets the value from the dictionary for the given key as type T.
        /// If the key is not present in the dictionary, returns the default for the type.
        /// </summary>
        /// <typeparam name="T">The expected value type</typeparam>
        /// <param name="dict">The dictionary</param>
        /// <param name="name">The name of the key</param>
        /// <returns>The value for the given key.</returns>
        public static T Get<T>(this Nancy.DynamicDictionary dict, string name) {
            return dict.Get<T>(name, default(T));
        }


        /// <summary>
        /// Gets the value from the dictionary for the given key as type T.
        /// If the key is not present in the dictionary, returns the default for the type.
        /// </summary>
        /// <typeparam name="T">The expected value type</typeparam>
        /// <param name="dict">The dictionary</param>
        /// <param name="name">The name of the key</param>
        /// <param name="defaultValue">The default value to return</param>
        /// <returns>The value for the given key.</returns>
        public static T Get<T>(this Nancy.DynamicDictionary dict, string name, T defaultValue) {
            T ret = defaultValue;

            if (dict[name].HasValue) {
                try {
                    ret = dict[name];
                }
                catch (FormatException) { }
            }

            return ret;
        }


    }

}
