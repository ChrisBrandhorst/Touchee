using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Touchee.Media.Music {


    /// <summary>
    /// Represents a webcast stream
    /// </summary>
    public interface IWebcastStream {


        /// <summary>
        /// The URI of this stream
        /// </summary>
        Uri Uri { get; }

        /// <summary>
        /// The encoding this stream uses
        /// </summary>
        StreamEncoding Encoding { get; }


    }

}
