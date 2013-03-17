define([
  'underscore',
  'models/module',
  './views/tracks',
  './views/albums',

  './views/artists',
  './views/genres',

  './views/artist',
  './views/genre',

  './models/containers/master_playlist'
], function(_, BaseModule, TracksView, AlbumsView, ArtistsView, GenresView, ArtistView, GenreView){
  
  var MusicModule = BaseModule.extend({
    
    
    // The different views that are available for this module, together
    // with the corresponding view class.
    views: {
      track:  TracksView,
      album:  AlbumsView,
      artist: ArtistsView,
      genre:  GenresView
    },
    
    
    // Get the container class for the given type
    getContainerClass: function(type) {
      // Can't use './models' here....
      return require('plugins/music/models/containers/' + type);
    },


    // Gets the view class for the given view description
    getViewClass: function(view) {
      switch(view) {
        case 'artist_track':  return ArtistView;
        case 'genre_track':   return GenreView;
      }
    },
    
    
    // 
    navigate: function(view, params) {

      // We have selected an artist from the artists list
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

      // We have selected an genre from the genres list
      else if (view instanceof GenresView) {
        
        // Get the genre
        var genre = params.genre;
        if (_.isUndefined(genre)) return;
        
        // Build the genre view
        var genreView = this.buildView( view.model.contents.container, _.extend(params, {view:'genre_track'}) );
        genreView.model.fetch();
        
        // Set view
        view.setRight(genreView);
      }

    }
    
    
  });
  
  return MusicModule;

});