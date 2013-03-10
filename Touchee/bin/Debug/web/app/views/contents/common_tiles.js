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
    getModels: function(first, count) {
      return this.model.models.slice(first, first + count);
    },
    
    
    // Gets the index of the given item
    getModelIndex: function(item) {
      return this.model.models.indexOf(item);
    }
    
    
  });
  
  return CommonTilesView;

});