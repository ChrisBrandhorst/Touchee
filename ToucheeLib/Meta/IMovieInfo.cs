using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Touchee.Meta {

    /// <summary>
    /// Represents information about a movie
    /// </summary>
    public interface IMovieInfo {
        
        string Actors { get; }
        string Director { get; }
        TimeSpan Duration { get; }
        string Genre { get; }
        string ImdbID { get; }
        float ImdbRating { get; }
        int ImdbVotes { get; }
        string Plot { get; }
        string PosterURL { get; }
        //System.Drawing.Image Poster { get; }
        string Rating { get; }
        DateTime ReleaseDate { get; }
        string Title { get; }
        string Writer { get; }

    }

}