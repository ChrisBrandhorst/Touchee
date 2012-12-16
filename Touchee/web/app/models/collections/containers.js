define([
  'underscore',
  'Backbone',
  'models/container',
  'models/server_info'
], function(_, Backbone, Container, ServerInfo){
  
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
    
    
    // Parse method
    parse:  function(response) {
      return response.items;
    },
    
    
    // Group all containers by their content type
    groupByContentType: function() {
      var groups = [], group;
      
      this.each(function(c){
        if (c.isNew()) return;
        
        var ct = c.get('contentType');
        if (!group || group.key != ct) {
          group = {key:ct, members:[c]};
          groups.push(group);
        }
        else {
          group.members.push(c);
        }
        
      });
      
      return groups;
    },
    
    
    // Get all containers for the given contentType
    getByContentType: function(contentType) {
      return this.filter(function(c) {
        return c.get('contentType') == contentType;
      });
    }
    
    
  });
  
  return Containers;
  
});