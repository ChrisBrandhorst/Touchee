define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/tiles_base'
], function($, _, Backbone, TilesBase) {
  
  var ChannelsList = TilesBase.extend({
    
    
    // Show the header
    header: true,
    
    
    // Constructor
    initialize: function(options) {
      var contents = options.contents;
      options.scrolllistOptions = {
        showID:         true,
        tileArtworkURL: options.contents.container.getArtworkUrl({size:'medium',resizeMode:'containAndFill'})
      };
      this.header = !!options.contents.filter.get('genre');
      TilesBase.prototype.initialize.apply(this, arguments);
    }
    
  });
  
  return ChannelsList;
});