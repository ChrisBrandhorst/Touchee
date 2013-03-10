define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/split',
  './_artists_list'
], function($, _, Backbone, SplitView, ArtistsListView) {
  
  
  var ArtistsView = SplitView.extend({
    
    
    // SplitView options
    contentType:    'artists',
    
    
    // Constructor
    initialize: function(options) {
      this.left = new ArtistsListView(options);
      SplitView.prototype.initialize.apply(this, arguments);
    }
    
    
  });
  
  
  return ArtistsView;
  
});