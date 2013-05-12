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
      this.on('sync add', this.fetchContainers, this);
    },


    // Called after each sync
    fetchContainers: function(model) {

      // If the collection has been synced, call sync for each container
      // and trigger event when completed
      if (model == Media) {
        var containersFetched = 0;
        this.each(function(medium){
          medium.containers.fetch({
            success: function() {
              if (++containersFetched == Media.length)
                Media.trigger('sync:containers:all');
            }
          });
        });
      }
      
      // Else, sync the containers for the medium added
      else {
        model.containers.fetch();
      }

    },


    getLocal: function() {
      return this.find(function(medium){
        return medium.isLocal();
      });
    }
    
  }));
  
  return Media;
  
});