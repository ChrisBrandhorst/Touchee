define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/tiles_base'
], function($, _, Backbone, TilesBase) {
  
  var AlbumsList = TilesBase.extend({
    
    // Constructor
    initialize: function(options) {
      var contents = options.contents;
      
      var artworkURL = options.contents.container.getArtworkUrl({size:'medium'});
      
      options.scrolllistOptions = {
        showID:         true,
        tileArtworkURL: artworkURL,
        tileLine1Key:   'album',
        tileLine2Key:   'artist'
      };
      TilesBase.prototype.initialize.apply(this, arguments);
    }
    
  });
  
  return AlbumsList;
});