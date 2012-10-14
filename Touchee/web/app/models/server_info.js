define([
  'underscore',
  'Backbone'
], function(_, Backbone){
  
  // ServerInfo object
  var ServerInfo = Backbone.Model.extend({
    
    url: "server_info"
    
  });
  
  return new ServerInfo;
  
});