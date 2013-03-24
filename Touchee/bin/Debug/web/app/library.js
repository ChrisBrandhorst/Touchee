define([
  'underscore',
  'Backbone',
  'models/collections/media'
], function(_, Backbone, Media) {
  
  var Library = {
    
    
    // Init
    initialize: function() {
    },
    
    
    // Called when the App is ready (again)
    load: function(connectedBefore) {
      
      // Get all media
      Media.fetch({
        update:   true,
        success:  _.bind(this.mediaLoaded, this)
      });
      
    },
    
    
    // 
    mediaLoaded: function() {

      var medium = Media.first();
      if (medium)
        medium.containers.on('sync', this.firstMediumLoaded, this);

    },


    //
    firstMediumLoaded: function(containers) {
      containers.off('sync', this.firstMediumLoaded);
      Backbone.history.navigate(containers.medium.url(), {trigger:true});
    }
    
    
  };
  
  return Library;

});