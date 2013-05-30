define([
  'underscore',
  'models/module',
  
  './views/tracks',
  './views/albums',
  './views/artists',
  './views/genres',

  './models/containers/master_playlist'
], function(_, BaseModule, TracksView, AlbumsView, ArtistsView, GenresView, MasterPlaylist){
  
  var SpotifyModule = BaseModule.extend({


    // The different views that are available for this module, together
    // with the corresponding view class.
    views: {
      track:  TracksView,
      artist: ArtistsView,
      album:  AlbumsView,
      genre:  GenresView
    },


    // The default container model
    containerModel: MasterPlaylist
    
    
  });
  
  return SpotifyModule;

});