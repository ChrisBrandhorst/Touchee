define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/split',
  './_genres_list'
], function($, _, Backbone, SplitView, GenresListView) {
  
  
  var GenresView = SplitView.extend({
    
    
    // SplitView options
    contentType:    'genres',
    
    
    // Constructor
    initialize: function(options) {
      this.left = new GenresListView(options);
      SplitView.prototype.initialize.apply(this, arguments);
    }
    
    
  });
  
  
  return GenresView;
  
});