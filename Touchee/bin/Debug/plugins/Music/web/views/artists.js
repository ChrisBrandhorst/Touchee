define([
  'jquery',
  'underscore',
  'Backbone',
  './../models/view_models/artists',
  'views/contents/split',
  './_artists_list',
  './artist'
], function($, _, Backbone, Artists, SplitView, ArtistsListView, ArtistView) {
  
  
  var ArtistsView = SplitView.extend({
    
    
    // SplitView options
    contentType:  'artists',
    

    // Which model this view is supposed to show
    viewModel:    Artists,
    subView:      ArtistView,

    
    // Constructor
    initialize: function(options) {
      this.left = new ArtistsListView(options);
      SplitView.prototype.initialize.apply(this, arguments);
    }
    
    
  });
  
  
  return ArtistsView;
  
});