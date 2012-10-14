using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Touchee {

    public interface IWebcast : IAudioItem {

        /// <summary>
        /// The title of this stream
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The sorted title of this stream
        /// </summary>
        string SortName { get; }

        /// <summary>
        /// The genre of this stream
        /// </summary>
        string Genre { get; }

        /// <summary>
        /// The streams from which this webcast can be streamed
        /// </summary>
        ISet<StreamInfo> Streams { get; }

        /// <summary>
        /// The meta-text for this stream
        /// </summary>
        string Meta { get; }

    }

}
