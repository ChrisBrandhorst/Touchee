namespace Touchee.Media.Music {

    public static class Extensions {

        public static bool IsByArtist(this ITrack track, string artist) {
            return track.Artist == null ? artist == null : track.Artist.ToLower() == artist;
        }
        public static bool IsOfGenre(this ITrack track, string genre) {
            return track.Genre == null ? genre == null : track.Genre.ToLower() == genre;
        }

    }
}
