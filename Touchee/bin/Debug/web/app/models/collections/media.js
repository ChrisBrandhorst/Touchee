define([
  'underscore',
  'Backbone',
  'models/medium'
], function(_, Backbone, Medium){
  
  var Media = new (Backbone.Collection.extend({
    
    model:  Medium,
    url:    "media",
    

    // Constructor
    initialize: function() {
      this.toFetch = 0;
      this.on('add', this.fetchContainers, this);
    },


    // Called when a Medium is added to this collection.
    // Continues fetching the Containers for the new Media, until
    // all are fetched.
    fetchContainers: function(model) {
      this.toFetch++;
      model.containers.fetch({
        success: function() {
          if (--Media.toFetch == 0)
            Media.trigger('sync:containers:all');
        }
      });
    }

  }));
  
  return Media;
  
});