define([
  'underscore',
  'models/module',
  './views/tracks',
  './views/albums',
  './views/artists',
  './views/artist',
  './models/containers/master_playlist'
], function(_, BaseModule, TracksView, AlbumsView, ArtistsView, ArtistView){
  
  var MusicModule = BaseModule.extend({
    
    
    // The different views that are available for this module, together
    // with the corresponding view class.
    views: {
      track:  TracksView,
      album:  AlbumsView,
      artist: ArtistsView
    },
    
    
    // Get the container class for the given type
    getContainerClass: function(type) {
      // Can't use './models' here....
      return require('plugins/music/models/containers/' + type);
    },


    // Gets the view class for the given view description
    getViewClass: function(view) {
      if (view == 'artist_track') return ArtistView;
    },
    
    
    // 
    navigate: function(view, params) {
      
      // We have selected an artist from the artist list
      if (view instanceof ArtistsView) {
        
        // Get the artist
        var artist = params.artist;
        if (_.isUndefined(artist)) return;
        
        // Build the artist view
        var artistView = this.buildView( view.model.contents.container, _.extend(params, {view:'artist_track'}) );
        artistView.model.fetch();
        
        // Set view
        view.setRight(artistView);
      }
      
    }
    
    
  });
  
  return MusicModule;

});