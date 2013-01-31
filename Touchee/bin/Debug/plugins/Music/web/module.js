define([
  'underscore',
  'models/module',
  './views/songs',
  './views/albums',
  './models/containers/master_playlist'
], function(_, BaseModule, SongsView, AlbumsView){
  
  var MusicModule = BaseModule.extend({
    
    
    // The different views that are available for this module, together
    // with the corresponding view class.
    views: {
      song:   SongsView,
      album:  AlbumsView
    },
    
    
    // Get the container class for the given type
    getContainerClass: function(type) {
      // Can't use './models' here....
      return require('plugins/music/models/containers/' + type);
    }
    
    
  });
  
  return MusicModule;

});