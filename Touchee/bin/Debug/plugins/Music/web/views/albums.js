define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/tiles'
], function($, _, Backbone, TilesView) {
  
  var AlbumsView = TilesView.extend({
    
    // ScrollList properties
    contentType:    'albums',
    indexAttribute: 'albumArtistSort',
    
    
    // Tiles properties
    line1:    'album',
    line2:    'albumArtist',
    
    
    // Constructor
    initialize: function(options) {
      TilesView.prototype.initialize.apply(this, arguments);
      // this.filter = options.filter;
      this.model.on('reset add remove change', this.contentChanged, this);
    },
    
    
    // Gets the model count
    getCount: function() {
      return this.model.length;
    },
    
    
    // Gets the models
    getModels: function(items) {
      return this.model.models.slice(items.first, items.first + items.count);
    },
    
    
    // Gets the unknown value for the given attribute of the model
    getUnknownAttributeValue: function(model, attr) {
      var val = I18n.unknown;
      switch (attr) {
        case 'album':       val += " " + I18n.p.music.models.album.one;   break;
        case 'albumArtist': val += " " + I18n.p.music.models.artist.one;  break;
      }
      return val;
    }
    
    
  });
  
  return AlbumsView;
  
});