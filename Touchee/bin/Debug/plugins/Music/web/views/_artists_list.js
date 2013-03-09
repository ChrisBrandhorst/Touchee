define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/table'
], function($, _, Backbone, TableView) {
  
  var ArtistsView = TableView.extend({
    
    // ScrollList properties
    contentType:    'artists',
    indexAttribute: 'artistSort',
    
    
    // Table properties
    columns: [
      'artist'
    ],
    
    
    // Constructor
    initialize: function(options) {
      TableView.prototype.initialize.apply(this, arguments);
      this.model.on('reset add remove change', this.contentChanged, this);
    },
    
    
    // Gets the model count
    getCount: function() {
      return this.model.length;
    },
    
    
    // Gets the models
    getModels: function(first, count) {
      return this.model.models.slice(first, first + count);
    },
    
    
    // Gets the index of the given item
    getIndex: function(item) {
      return this.model.models.indexOf(item);
    },
    
    
    // Gets the unknown value for the given attribute of the model
    getUnknownAttributeValue: function(model, attr) {
      return I18n.unknown + " " + I18n.p.music.models.artist.one;
    }
    
    
  });
  
  return ArtistsView;

});