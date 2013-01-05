define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/tiles_base'
], function($, _, Backbone, TilesBase) {
  
  var AlbumsList = TilesBase.extend({
    
    
    // Albums are sorted by alpha num
    alphaNum: true,
    
    
    // Constructor
    initialize: function(options) {
      var contents    = options.contents,
          artworkURL  = options.contents.container.getArtworkUrl({size:'medium'});
      
      options.scrolllistOptions = {
        showID:         true,
        tileArtworkURL: artworkURL,
        tileLine1Key:   'album',
        tileLine2Key:   'artist'
      }
      
      TilesBase.prototype.initialize.apply(this, arguments);
    }
    
  });
  
  return AlbumsList;
});