define([
  'underscore',
  'Backbone'
], function(_, Backbone){
  
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
      this.listenTo(this.container, 'notifyContentsChanged', this.containerContentsChanged);
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
    },


    // Called when the server notifies the client of the changing of the contents of the container
    // VIRTUAL
    containerContentsChanged: function() {
      this.fetch();
      // this.trigger('notifyContentsChanged');
    }
    
    
  });
  
  return Contents;
});