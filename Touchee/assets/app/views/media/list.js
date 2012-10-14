define([
  'jquery',
  'Underscore',
  'Backbone',
  'views/paged_view',
  'models/collections/media',
  'views/media/show'
], function($, _, Backbone, PagedView, Media, MediumShowView) {
  
  var MediaList = PagedView.extend({
    
    el:     $('#navigation'),
    
    // Constructor
    initialize: function() {
      // Render list if entire collection is reset
      this.collection.on('reset', this.render, this);
      PagedView.prototype.initialize.apply(this, arguments);
    },
    
    // Releases events
    onDispose: function() {
      this.collection.off('reset', this.render);
    }
    
  });
  
  return MediaList;
  
});