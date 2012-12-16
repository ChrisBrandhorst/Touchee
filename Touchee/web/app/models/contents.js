define([
  'underscore',
  'Backbone',
  'models/filter'
], function(_, Backbone, Filter){
  
  var Contents = Backbone.Model.extend({
    
    
    // URL
    url:    function() { return this.container.url() + '/contents'; },
    
    
    // Items collection
    items:  new Backbone.Collection([], { model: Backbone.Model }),
    
    
    // Constructor
    initialize: function(attributes, options) {
      options || (options = {});
      this.container = options.container;
      this.filter = options.filter instanceof Filter ? options.filter : new Filter(options.filter);
    },
    
    
    // Parse the response in order to fill the items collection
    parse: function(response) {
      this.items.reset(response.contents);
      return {};
    },
    
    
    shuffable: false,
    
    // // Modified fetch method for including type and filter parameters
    // fetch: function(options) {
    //   options = _.extend({}, options, { data: {
    //     filter: this.filter.toString({type:this.get('type')})
    //   }});
    //   Backbone.Model.prototype.fetch.call(this, options);
    // },
    // 
    // 
    // // Modified parse method
    // parse: function(response) {
    //   
    //   // Get meta data
    //   if (response.meta) {
    //     this.meta = response.meta;
    //     delete response.meta;
    //   }
    //   
    //   // Set the keys
    //   if (response.keys) {
    //     this.keys = {};
    //     _.each(response.keys, function(key, i) {
    //       this.keys[key] = i;
    //     }, this);
    //     delete response.keys;
    //   }
    //   
    //   // Delete unnecessary containerID
    //   delete response.containerID;
    //   return response;
    // },
    // 
    // 
    // // Build the URL for the given item
    // getUrl: function(id) {
    //   var filter = {};
    //   filter[this.idAttribute] = id;
    //   return this.url().replace(/contents$/, "play/" + this.filter.toString(filter));
    // },
    // 
    // 
    // Get the title for this content object
    getTitle: function() {
      // return T.T.viewTypes[ this.get('type') ].toTitleCase();
      return "TITLE";
    },
    
    
    // Get the view type for this contents
    getViewType: function() {
      return this.get('type');
    }
    
    
  });
  
  return Contents;
});