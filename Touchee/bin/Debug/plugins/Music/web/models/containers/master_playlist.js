define([
  'underscore',
  'Backbone',
  'models/container',
  './../track',
  './../view_models/tracks',
  './../view_models/albums',
  './../view_models/artists',
  './../view_models/genres',
  './../view_models/artist_tracks',
  './../view_models/genre_tracks'
], function(_, Backbone, Container,
            Track,
            Tracks, Albums, Artists, Genres,
            ArtistTracks, GenreTracks
  ){
  
  var MasterPlaylist = Container.extend({
    
    
    // The model used for the items within the contents object
    contentsItemModel: Track,
    
    
    // The different views that are available for this container, together
    // with the corresponding viewmodel class.
    views: {
      track:    Tracks,
      album:    Albums,
      artist:   Artists,
      genre:    Genres,
      playlist: {}
    },
    
    
    // Gets the view model class for the given view description
    getViewModelClass: function(view) {
      switch(view) {
        case 'artist_track':  return ArtistTracks;
        case 'genre_track':   return GenreTracks;
      }
    }

    
  });

  return MasterPlaylist;
  
});