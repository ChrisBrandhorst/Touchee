define([
  'underscore',
  'Backbone',
  'Touchee'
], function(_, Backbone, Touchee){
  
  var Contents = Backbone.Collection.extend({
    
    
    // Backbone collection options
    url: function(params) {
      return this.container.url(_.extend({}, this.params, params));
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