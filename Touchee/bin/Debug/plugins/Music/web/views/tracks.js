define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/table'
], function($, _, Backbone, TableView) {
  
  var TracksView = TableView.extend({
    
    // ScrollList properties
    contentType:    'tracks',
    indexAttribute: 'titleSort',
    
    
    // Table properties
    columns: [
      'title',
      'artist',
      'album',
      function(track){ return String.duration(track.get('duration')); }
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
    }
    
    
  });
  
  return TracksView;
  
});