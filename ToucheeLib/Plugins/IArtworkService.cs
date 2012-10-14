using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using Touchee.Artwork;

namespace Touchee.Service {

    public interface IAlbumArtworkService {
        ServiceResultStatus GetAlbumArtwork(ITrack track, out Image artwork);
        ServiceResultStatus GetAlbumArtwork(string artist, string title, out Image artwork);
    }

    public interface IArtistArtworkService {
        ServiceResultStatus GetArtistArtwork(ITrack track, out Image artwork);
        ServiceResultStatus GetArtistArtwork(string artist, out Image artwork);
    }

    public interface IVideoArtworkService {
        ServiceResultStatus GetVideoArtwork(string path, out Image artwork);
    }

    public interface IMovieArtworkService {
        ServiceResultStatus GetMovieArtwork(string title, out Image artwork);
        ServiceResultStatus GetMovieArtwork(string title, int year, out Image artwork);
    }

    public interface ISeriesArtworkService {
        ServiceResultStatus GetSeriesArtwork(string title, out Image artwork);
        ServiceResultStatus GetSeriesArtwork(string title, string season, out Image artwork);
    }

    public interface ICustomArtworkService {
        ServiceResultStatus GetCustomArtwork(string query, out Image artwork);
    }

}