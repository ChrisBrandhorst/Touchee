define([
  'underscore',
  'Backbone'
], function(_, Backbone){
  
  
  // Parses an escaped filter string to an object
  var parse = function(str) {
    var parsed = {};
    
    // Because JS has no lookbehind, we need to fix escaped comma's
    var parts = str.split(',');
    
    for (var i = parts.length - 2; i >= 0; i--) {
      if (parts[i].match(/\\$/))
        parts[i] = parts[i].slice(0, -1) + ',' + parts.splice(i + 1, 1)[0];
    }
    
    // Split key value pairs
    _.each(parts, function(f){
      var kv  = f.split(':'),
          v   = kv.slice(1).join(':');
      if (v)
        parsed[kv[0]] = v;
    });
    
    return parsed;
  };
  
  
  // Filter object
  var Filter = Backbone.Model.extend({
    
    
    // Constructor
    initialize: function(filter) {
      this.set(filter || {});
    },
    
    
    // Extends the filter with the given filter
    set: function(key, value, options) {
      if (_.isString(key) && key.indexOf(':') > -1)
        key = parse(key);
      return Backbone.Model.prototype.set.call(this, key, value, options);
    },
    
    
    // Ouputs an ordered, escaped string representation of the filter
    // with optional filter additions
    toString: function(attributes) {
      if (typeof attributes == 'string')
        attributes = parse(attributes);
      
      var filter = _.extend({}, this.toJSON(), attributes || {});
      
      var f =
        _.map(
          _.keys(filter).sort(),
          function(key) {
            return [key, (filter[key] || "").toString().replace(',', "\\,")].join(':');
            // return [key, encodeURIComponent(filter[key] || "")].join(':');
          }
        ).join(',');
      
      return f;
    }
    
    
  });
  
  return Filter;
  
});