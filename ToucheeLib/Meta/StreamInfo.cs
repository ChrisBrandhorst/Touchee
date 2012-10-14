using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Touchee {

    public enum StreamEncoding {
        HEAAC,
        MP3,
        WindowsMedia
    }

    public class StreamInfo : IComparable {
        public Uri Uri { get; protected set; }
        public StreamEncoding Encoding { get; protected set; }
        public StreamInfo(Uri uri, StreamEncoding encoding) {
            this.Uri = uri;
            this.Encoding = encoding;
        }
        public int CompareTo(object obj) {
            var other = (StreamInfo)obj;
            return this.Encoding.CompareTo(other.Encoding);
        }
    }

}
