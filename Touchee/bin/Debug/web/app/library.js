define([
  'underscore',
  'Backbone',
  'models/collections/media'
], function(_, Backbone, Media) {
  
  var Library = {
    
    
    // Init
    initialize: function() {
      this.listenTo(Media, 'sync', this.mediaLoaded);
    },
    
    
    // Called when the App is ready (again)
    load: function(connectedBefore) {
      
      // Get all media
      Media.fetch({ update: true });
      
    },
    
    
    // Check for loading of all containers
    mediaLoaded: function() {

      var containersLoaded = 0;

      var count = function(){
        containersLoaded++;
        if (containersLoaded == Media.length) {
          // Media.off('sync:containers', count);
          Library.trigger('loaded:containers');
        }
      };
debugger;
      Media.on('sync:containers', count);

    },


    //
    firstMediumLoaded: function(containers) {
      Backbone.history.navigate(containers.medium.url(), {trigger:true});
    }
    
    
  };
  
  _.extend(Library, Backbone.Events);

  return Library;

});