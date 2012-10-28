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
      Media.fetch({update:true});
      
    }
    
    
  };
  
  return Library;

});