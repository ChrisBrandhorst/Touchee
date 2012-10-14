using System.IO;
using System.Collections.Generic;
using Nancy;
using Nancy.IO;
using Nancy.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Touchee.Server.Http {

    public class JsonNetSerializer : ISerializer {

        DefaultJsonSerializer _defaultSerializer = new DefaultJsonSerializer();
        JsonSerializer _serializer;

        public JsonNetSerializer() {
            _serializer = JsonSerializer.Create(
                new JsonSerializerSettings {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }
            );
            _serializer.Formatting = Formatting.None;
        }

        /// <summary>
        /// Whether the serializer can serialize the content type
        /// </summary>
        /// <param name="contentType">Content type to serialise</param>
        /// <returns>True if supported, false otherwise</returns>
        public bool CanSerialize(string contentType) {
            return _defaultSerializer.CanSerialize(contentType);
        }

        /// <summary>
        /// Gets the list of extensions that the serializer can handle.
        /// </summary>
        /// <value>An <see cref="IEnumerable{T}"/> of extensions if any are available, otherwise an empty enumerable.</value>
        public IEnumerable<string> Extensions {
            get { return _defaultSerializer.Extensions; }
        }

        /// <summary>
        /// Serialize the given model with the given contentType
        /// </summary>
        /// <param name="contentType">Content type to serialize into</param>
        /// <param name="model">Model to serialize</param>
        /// <param name="outputStream">Output stream to serialize to</param>
        /// <returns>Serialised object</returns>
        public void Serialize<TModel>(string contentType, TModel model, Stream outputStream) {
            using (var writer = new JsonTextWriter(new StreamWriter(new UnclosableStreamWrapper(outputStream)))) {
                _serializer.Serialize(writer, model);
                writer.Flush();
            }
        }

    }

}
