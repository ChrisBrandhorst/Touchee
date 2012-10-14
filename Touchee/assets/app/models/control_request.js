define([
  'Underscore',
  'Backbone'
], function(_, Backbone){
  
  
  // Filter object
  var ControlRequest = Backbone.Model.extend({
    
    
    url: function() { return "control/" + this.get('command'); },
    
    
    // Constructor
    initialize: function(params) {
      this.filter = params.filter;
      this.id = 0;
    },
    
    
    // 
    save: function(attributes, options) {
      var data = this.toJSON();
      if (this.filter)
        data.filter = this.filter.toString();
      
      options = _.extend({}, options, {
        data:         data,
        processData:  true
      });
      Backbone.Model.prototype.save.apply(this, [attributes, options]);
    }
    
    
  });
  
  return ControlRequest;
  
});