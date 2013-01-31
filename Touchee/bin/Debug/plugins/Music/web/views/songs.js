define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/table'
], function($, _, Backbone, TableView) {
  
  var SongsView = TableView.extend({
    
    // ScrollList properties
    contentType:    'songs',
    indexAttribute: 'titleSort',
    
    
    // Table properties
    columns: [
      'title',
      'artist',
      'album',
      function(song){ return String.duration(song.get('duration')); }
    ],
    
    
    // Constructor
    initialize: function(options) {
      TableView.prototype.initialize.apply(this, arguments);
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
    }
    
  });
  
  return SongsView;
  
});