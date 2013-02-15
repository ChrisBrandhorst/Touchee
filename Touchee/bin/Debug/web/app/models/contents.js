define([
  'underscore',
  'Backbone',
  'models/params'
], function(_, Backbone, Params){
  
  var Contents = Backbone.Collection.extend({
    
    
    // Backbone collection options
    url:      function() {
      var url = this.container.url() + "/contents";
      if (this.params && this.params.count) url += "/" + this.params.toString();
      return url;
    },
    
    
    // Custom model properties
    fetched:  false,
    
    
    // Constructor
    initialize: function(attributes, options) {
      options || (options = {});
      this.container = options.container;
      this.params = options.params;
    },
    
    
    // Modified fetch method for setting the fetched property
    fetch: function(options) {
      options || (options = {});
      var success = options.success, contents = this;
      options.success = function(){
        contents.fetched = true;
        if (_.isFunction(success))
          success.apply(this, arguments);
      };
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