define([
  'underscore',
  'Backbone'
], function(_, Backbone){
  
  var ContentsPart = Backbone.Collection.extend({
    
    
    // Dummy comparator, so the default sort will always succeed
    comparator: "_",
    
    
    // Constructor
    initialize: function(models, options) {
      this.contents = options.contents;
      this.filter   = options.filter;
      
      this.contents.on('reset',   this._contentsReset,  this);
      this.contents.on('change',  this._contentsChange, this);
      this.contents.on('add',     this.add,             this);
      this.contents.on('remove',  this.remove,          this);
    },
    
    
    // Redirect fetch call to the underlying contents
    fetch: function() {
      this.contents.fetch.apply(this.contents, arguments);
    },
    
    
    // Filters the models from the contents collection.
    // Override this for custom filtering.
    sieve: function(models) {
      return models;
    },
    
    
    // Override the default collection sort method, so we can use custom 
    // sorting using linq by implementing a 'order' method.
    sort: function(options) {
      if (!this.order) {
        return Backbone.Collection.prototype.sort.apply(this, arguments);
      }
      
      var models = this.order(this.getEnum());
      if (models.ToArray) models = models.ToArray();
      else if (models.models) models = models.models;
      this.models = models;
      
      if (!options || !options.silent) this.trigger('sort', this, options);
      return this;
    },
    
    
    // Resets the collection with the models from the given colleciton,
    // filtered by the sieve method
    _contentsReset: function(collection, options) {
      var models = this.sieve(collection.models);
      this.reset(models, options);
    },
    
    
    // Resort the view model when an item changes
    _contentsChange: function(model, options) {
      this.sort();
    }
    
    
  });
  
  
  return ContentsPart;

});