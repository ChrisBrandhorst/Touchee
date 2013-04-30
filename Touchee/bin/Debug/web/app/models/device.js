define([
  'underscore',
  'Backbone'
], function(_, Backbone){
  
  var Device = Backbone.Model.extend({
    
    isMaster: function() {
      return this.get('type') == 'master';
    }
    
  });
  
  return Device;
  
});