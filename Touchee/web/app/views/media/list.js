define([
  'jquery',
  'underscore',
  'Backbone',
  'models/collections/media',
  'views/paged_view',
  'views/media/show'
], function($, _, Backbone, Media, PagedView, MediumShowView) {
  
  
  var MediaListView = PagedView.extend({
    
    
    // Backbone View options
    
    
    
    // Constructor
    initialize: function() {
      
      // Talkin' 'bout Media
      this.collection = Media;
      
      // Render list if collection is reset
      // this.collection.on('reset', this.render, this);
      this.collection.on('reset update', this.afterInitialReset, this);
      PagedView.prototype.initialize.apply(this, arguments);
    },
    
    
    // Releases events
    onDispose: function() {
      this.collection.off('reset update', this.render);
      this.collection.off('reset update', this.afterInitialReset);
    },
    
    
    // Shows the first medium after the initial reset of the Media collection
    afterInitialReset: function() {
      
      var localMedium = Media.getLocal();
      if (localMedium) {
        this.collection.off('reset update', this.afterInitialReset);
        this.navigate(localMedium);
      }
      
    },
    
    
    // Navigate to the given medium and group
    navigate: function(medium, group) {
      
      // Generate key and try to get the corresponding view
      var key   = [medium.id, group].join("_"),
          view  = this.getPage(key);
      
      // No view? Build it!
      if (!view) {
        view = new MediumShowView({model:medium,contentType:group});
        this.storePage(key, view);
      }
      this.activatePage(view);
      
    }
    
    
    
  });
  
  return MediaListView;
  
});