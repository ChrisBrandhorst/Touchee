define([
  'underscore',
  'Backbone',
  'models/filter'
], function(_, Backbone, Filter){
  
  var Contents = Backbone.Collection.extend({
    
    
    // Backbone collection options
    model:  Backbone.Model,
    url:    function() { return this.container.url() + '/contents'; },
    
    
    // Constructor
    initialize: function(attributes, options) {
      options || (options = {});
      this.container = options.container;
      this.filter = options.filter;
    },
    
    
    // Modified fetch method for sending the filter param with the request
    fetch: function(options) {
      options || (options = {});
      (options.data || (options.data = {})).filter = this.filter.toString();
      Backbone.Collection.prototype.fetch.call(this, options);
      return this;
    },
    
    
    // Parse the response in order to fill the items collection
    parse: function(response) {
      return response.contents;
    }
    
    
  });
  
  return Contents;
});