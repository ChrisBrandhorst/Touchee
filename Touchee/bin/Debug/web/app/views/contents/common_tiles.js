define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/tiles'
], function($, _, Backbone, TilesView) {
  
  var CommonTilesView = TilesView.extend({
    
    
    // Tiles view properties
    artworkSize: 250,
    
    
    // Gets the model count
    getCount: function() {
      return this.model.length;
    },
    
    
    // Gets the set of models for the given range
    getItems: function(first, count) {
      return this.model.models.slice(first, first + count);
    }
    
    
  });
  
  return CommonTilesView;

});