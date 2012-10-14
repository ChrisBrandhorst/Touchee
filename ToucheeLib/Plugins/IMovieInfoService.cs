using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Touchee.Service {

    public interface IMovieInfoService {
        ServiceResultStatus GetMovieInfo(string title, out IMovieInfo movieInfo);
        ServiceResultStatus GetMovieInfo(string title, int year, out IMovieInfo movieInfo);
    }

}
