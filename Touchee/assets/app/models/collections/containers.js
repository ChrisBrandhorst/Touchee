define([
  'Underscore',
  'Backbone',
  'models/container'
], function(_, Backbone, Container){
  
  var Containers = Backbone.Collection.extend({
    
    url:    function() { return this.medium.url() + '/containers'; },
    model:  Container,
    
    initialize: function(params) {
      this.medium = params.medium;
    },
    
    parse:  function(response) {
      return response.items;
    },
    
    getLocal: function() {
      return this.find(function(medium){
        return medium.isLocal();
      });
    },
    
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
    
    getByContentType: function(contentType) {
      return this.filter(function(c) {
        return c.get('contentType') == contentType;
      });
    }
    
  });
  
  return Containers;
  
});