define([
  'underscore',
  'Backbone',
  'models/medium'
], function(_, Backbone, Medium){
  
  var Media = Backbone.Collection.extend({
    
    model:  Medium,
    url:    "media",
    // sync:   Backbone.readOnlySync,
    
    initialize: function() {
      this.on('add', this.mediumAdded, this);
    },

    parse:  function(response) {
      return response.items;
    },
    
    getLocal: function() {
      return this.find(function(medium){
        return medium.isLocal();
      });
    },

    // When a medium is added, auto-load the containers
    mediumAdded: function(model, collection, options) {
      model.containers.fetch();
    }
    
  });
  
  return new Media;
  
});