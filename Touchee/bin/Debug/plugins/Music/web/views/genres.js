define([
  'jquery',
  'underscore',
  'Backbone',
  './../models/view_models/genres',
  'views/contents/split',
  './_genres_list',
  './genre'
], function($, _, Backbone, Genres, SplitView, GenresListView, GenreView) {
  
  
  var GenresView = SplitView.extend({
    
    
    // SplitView options
    contentType:  'genres',
    

    // Which model this view is supposed to show
    viewModel:    Genres,
    subView:      GenreView,
    
    
    // Constructor
    initialize: function(options) {
      this.left = new GenresListView(options);
      SplitView.prototype.initialize.apply(this, arguments);
    }
    
    
  });
  
  
  return GenresView;
  
});