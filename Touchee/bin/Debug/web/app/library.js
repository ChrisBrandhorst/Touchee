define([
  'underscore',
  'Backbone',
  'models/collections/media'
], function(_, Backbone, Media) {
  
  var Library = {
    
    
    // Init
    initialize: function() {
      Media.on('sync:containers:all', this.allContainersLoaded, this);
    },
    
    
    // Called when the App is ready (again)
    load: function(firstTime) {
      this.firstTime = firstTime;
      Media.fetch();
    },
    
    
    // 
    allContainersLoaded: function() {
      Library.trigger('loaded:containers');
      if (this.firstTime && Media.length)
        Backbone.history.navigate(Media.first().url(), {trigger:true});
    }
    
  };
  
  _.extend(Library, Backbone.Events);

  return Library;

});