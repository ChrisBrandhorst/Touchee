using Touchee.Meta;
namespace Touchee.Components.Services {

    public interface IMovieInfoService : IComponent {
        ServiceResultStatus GetMovieInfo(string title, out IMovieInfo movieInfo);
        ServiceResultStatus GetMovieInfo(string title, int year, out IMovieInfo movieInfo);
    }

}
