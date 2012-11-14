using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Touchee.Media.Music {

    /// <summary>
    /// Basic webcast stream implementation which sorts the streams
    /// according to the StreamEncoding enum order
    /// </summary>
    public class WebcastStream : IWebcastStream, IComparable {

        /// <summary>
        /// The URI of this stream
        /// </summary>
        public Uri Uri { get; protected set; }

        /// <summary>
        /// The encoding this stream uses
        /// </summary>
        public StreamEncoding Encoding { get; protected set; }

        /// <summary>
        /// Constructs a new WebcastStream object
        /// </summary>
        /// <param name="uri">The URI of the stream</param>
        /// <param name="encoding">The encoding of the stream</param>
        public WebcastStream(Uri uri, StreamEncoding encoding) {
            this.Uri = uri;
            this.Encoding = encoding;
        }

        /// <summary>
        /// Compares two WebcastStream objects with each other
        /// </summary>
        /// <param name="obj">The WebcastStream to compare to</param>
        /// <returns>-1 if this object is to be placed before the given object, 1 if after and 0 zero otherwise</returns>
        public int CompareTo(object obj) {
            var other = (WebcastStream)obj;
            return this.Encoding.CompareTo(other.Encoding);
        }
    }

}
