define([
  'underscore',
  'Backbone'
], function(_, Backbone){
  
  // Status object
  var Status = Backbone.Model.extend({
    
    
    // Backbone model options
    url:      "status",
    
    
    // Constructor
    initialize: function(attributes, options) {
    }
    
    
  });
  
  return new Status;
  
});