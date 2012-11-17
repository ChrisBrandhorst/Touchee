//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Runtime.Serialization;

namespace Touchee {

    /// <summary>
    /// Subclasses of this class can be serialized 
    /// </summary>
    [DataContract]
    public abstract class CacheObject<T> where T : CacheObject<T>, new() {


        /// <summary>
        /// Creates a DataContractSerializer for the current type
        /// </summary>
        /// <returns>A new DataContractSerializer for the current type.</returns>
        static DataContractSerializer GetSerializer() {
            return new DataContractSerializer(typeof(T));
        }


        /// <summary>
        /// Serializes the cacheobject to the given stream.
        /// </summary>
        /// <param name="stream">The stream to serialize to</param>
        public static void Serialize(Stream stream) {

            // Create an instance of the type
            var cacheObject = new T();
            ((CacheObject<T>)cacheObject).BeforeSerialize();

            // Setup the serializer
            var serializer = GetSerializer();
            var settings = new XmlWriterSettings();
            settings.Indent = true;
            
            // Write the object
            using(var writer = XmlWriter.Create(stream, settings))
                serializer.WriteObject(writer, cacheObject);
        }


        /// <summary>
        /// Serializes the cacheobject to the given file.
        /// </summary>
        /// <param name="path">The file to serialize to</param>
        public static void Serialize(string path) {
            using (var file = File.Open(path, FileMode.Create, FileAccess.Write))
                Serialize(file);
        }


        /// <summary>
        /// Deserialize the cacheobject from the given stream.
        /// </summary>
        /// <param name="stream">The stream to serialize from</param>
        public static void Deserialize(Stream stream) {
            var resultObject = GetSerializer().ReadObject(stream);
            ((CacheObject<T>)resultObject).AfterDeserialize();
        }


        /// <summary>
        /// Deserialize the cacheobject from the given file.
        /// </summary>
        /// <param name="path">The file to serialize from</param>
        public static void Deserialize(string path) {
            using (var file = File.Open(path, FileMode.Open, FileAccess.Read))
                Deserialize(file);
        }


        /// <summary>
        /// Occurs before serialization: the subclass should fill all entries of the object
        /// which need to be serialized.
        /// </summary>
        public abstract void BeforeSerialize();


        /// <summary>
        /// Occurs after deserialization: the subclass should process all entries which
        /// were loaded.
        /// </summary>
        public abstract void AfterDeserialize();



    }

}
