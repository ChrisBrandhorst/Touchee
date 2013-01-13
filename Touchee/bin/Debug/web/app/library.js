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
        success:  this.mediaLoaded
      });
      
    },
    
    
    // 
    mediaLoaded: function() {
      _.each(Media.models, function(medium, i){
        medium.containers.fetch({
          success: i > 0 ? null : function(){
            Backbone.history.navigate(medium.url(), {trigger:true});
          }
        });
      });
    }
    
    
  };
  
  return Library;

});