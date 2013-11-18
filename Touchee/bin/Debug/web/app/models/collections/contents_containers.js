define([
  'underscore',
  'Backbone',
  'models/contents_container',
  'models/server_info'
], function(_, Backbone, ContentsContainer, ServerInfo){
  
  var Containers = Backbone.Collection.extend({
    
    
    // The URL for retrieving the collection of containers
    url:    function() { return this.medium.url() + '/containers'; },
    
    
    // Get the model based on the attributes
    model:  function(attrs, options) {
      return ServerInfo.getPlugin(attrs.plugin).module.buildContainer(attrs, options);
    },
    
    
    // Constructor
    initialize: function(models, options) {
      this.medium = options.medium;
    },

    //
    masters: function() {
      return new Backbone.Collection(
        this.select(function(c){
          return c.isMaster();
        })
      );
    }
    
    
  });
  
  return Containers;
  
});