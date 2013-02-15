define([
  'underscore',
  'Backbone'
], function(_, Backbone){
  
  
  // Parses an params string (with escaped values) to an object
  var parse = function(str) {
    
    var parts = str.split('/');
    if (parts.length % 2 != 0)
      throw("Invalid filter string: " + str);
    
    var parsed = {};
    for (var i = 0; i < parts.length - 1; i += 2)
      parsed[ parts[i] ] = parts[i + 1];

    return parsed;
  };
  
  
  // Params object
  var Params = Backbone.Model.extend({
    
    
    // Key counter
    count: 0,
    
    
    // Constructor
    initialize: function(attributes) {
    },
    
    
    // Extends the params with the given params
    set: function(key, value, options) {
      if (_.isString(key) && !_.isString(value))
        key = parse(key);
      Backbone.Model.prototype.set.call(this, key, value, options);
      this.count = _.keys(this.attributes).length;
      return this;
    },
    
    
    // Ouputs an ordered, escaped string representation of the params
    // with optional filter additions
    toString: function(attributes) {
      if (_.isString(attributes))
        attributes = parse(attributes);
      
      var params = _.extend({}, this.toJSON(), attributes);
      
      return _.map(
        _.keys(params).sort(),
          function(key) {
            return key + "/" + encodeURIComponent(encodeURIComponent(params[key].toString() || ""));
          }
        ).join('/');
    }
    
    
  });
  
  return Params;
  
});