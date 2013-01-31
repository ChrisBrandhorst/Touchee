define([
  'underscore',
  'Backbone'
], function(_, Backbone){
  
  
  // Parses an filter string (with escaped values) to an object
  var parse = function(str) {
    
    // // Because JS has no lookbehind, we need to fix escaped slashes
    // for (var i = parts.length - 2; i >= 0; i--) {
    //   if (parts[i].match(/\\$/))
    //     parts[i] = parts[i].slice(0, -1) + '/' + parts.splice(i + 1, 1)[0];
    // }
    
    var parts = str.split('/');
    if (parts.length % 2 != 0)
      throw("Invalid filter string: " + str);
    
    var parsed = {};
    for (var i = 0; i < parts.length - 1; i += 2)
      parsed[ parts[i] ] = parts[i + 1];

    return parsed;
  };
  
  
  // Filter object
  var Filter = Backbone.Model.extend({
    
    
    // Key counter
    count: 0,
    
    
    // Constructor
    initialize: function(attributes) {
    },
    
    
    // Extends the filter with the given filter
    set: function(key, value, options) {
      if (_.isString(key) && !_.isString(value))
        key = parse(key);
      Backbone.Model.prototype.set.call(this, key, value, options);
      this.count = _.keys(this.attributes).length;
      return this;
    },
    
    
    // Ouputs an ordered, escaped string representation of the filter
    // with optional filter additions
    toString: function(attributes) {
      if (_.isString(attributes))
        attributes = parse(attributes);
      
      var filter = _.extend({}, this.toJSON(), attributes);
      
      return _.map(
        _.keys(filter).sort(),
          function(key) {
            return key + "/" + encodeURIComponent(encodeURIComponent(filter[key].toString() || ""));
          }
        ).join('/');
    }
    
    
  });
  
  return Filter;
  
});