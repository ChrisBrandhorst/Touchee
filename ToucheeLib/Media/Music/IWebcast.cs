using System.Collections.Generic;

namespace Touchee.Media.Music {


    /// <summary>
    /// Respresents a webcast
    /// </summary>
    public interface IWebcast : IAudioItem {

        /// <summary>
        /// The title of this stream
        /// </summary>
        string Title { get; }

        /// <summary>
        /// The sorted title of this stream
        /// </summary>
        string TitleSort { get; }

        /// <summary>
        /// The genre of this stream
        /// </summary>
        string Genre { get; }

        /// <summary>
        /// The streams from which this webcast can be streamed
        /// </summary>
        ISet<IWebcastStream> Streams { get; }

        /// <summary>
        /// The meta-text for this stream
        /// </summary>
        string Meta { get; }

    }

}
