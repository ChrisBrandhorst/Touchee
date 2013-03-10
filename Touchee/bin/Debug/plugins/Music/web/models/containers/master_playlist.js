define([
  'underscore',
  'Backbone',
  'models/container',
  './../track',
  './../view_models/tracks',
  './../view_models/albums',
  './../view_models/artists',
  './../view_models/artist_tracks'
], function(_, Backbone, Container,
            Track,
            Tracks, Albums, Artists,
            ArtistTracks
  ){
  
  var MasterPlaylist = Container.extend({
    
    
    // The model used for the items within the contents object
    contentsItemModel: Track,
    
    
    // The different views that are available for this container, together
    // with the corresponding viewmodel class.
    views: {
      artist:    Artists,
      album:     Albums,
      track:     Tracks,
      genre:     {},
      playlist:  {}
    },
    
    
    // Gets the view model class for the given view description
    getViewModelClass: function(view) {
      if (view == 'artist_track') return ArtistTracks;
    }

    
  });

  return MasterPlaylist;
  
});