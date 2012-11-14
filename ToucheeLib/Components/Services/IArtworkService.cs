using System.Drawing;
using Touchee.Artwork;
using Touchee.Media.Music;

namespace Touchee.Components.Services {

    public interface IArtworkService : IComponent {
    }

    public interface IAlbumArtworkService : IArtworkService {
        ServiceResultStatus GetAlbumArtwork(ITrack track, out Image artwork);
        ServiceResultStatus GetAlbumArtwork(string artist, string title, out Image artwork);
    }

    public interface IArtistArtworkService : IArtworkService {
        ServiceResultStatus GetArtistArtwork(ITrack track, out Image artwork);
        ServiceResultStatus GetArtistArtwork(string artist, out Image artwork);
    }

    public interface IVideoArtworkService : IArtworkService {
        ServiceResultStatus GetVideoArtwork(string path, out Image artwork);
    }

    public interface IMovieArtworkService : IArtworkService {
        ServiceResultStatus GetMovieArtwork(string title, out Image artwork);
        ServiceResultStatus GetMovieArtwork(string title, int year, out Image artwork);
    }

    public interface ISeriesArtworkService : IArtworkService {
        ServiceResultStatus GetSeriesArtwork(string title, out Image artwork);
        ServiceResultStatus GetSeriesArtwork(string title, string season, out Image artwork);
    }

    public interface ICustomArtworkService : IArtworkService {
        ServiceResultStatus GetCustomArtwork(string query, out Image artwork);
    }

}