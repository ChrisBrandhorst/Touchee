define([
  'underscore',
  'Backbone',
  'models/device'
], function(_, Backbone, Device){
  
  var Devices = Backbone.Collection.extend({
    
    model:  Device,
    url:    "devices",
    
    getMaster: function() {
      return this.find(function(device){
        return device.isMaster();
      });
    }
    
  });
  
  return Touchee.Devices = new Devices;
  
});