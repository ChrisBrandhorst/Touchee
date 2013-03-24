define([
  'underscore',
  'Backbone',
  'Touchee'
], function(_, Backbone, Touchee){
  
  var ContentsPart = Backbone.Collection.extend({
    
    
    // Dummy comparator, so the default sort will always succeed
    comparator: "_",
    
    
    // Gets the URL for this contents part
    url: function(params) {
      return this.contents.url(_.extend({}, this.params, params));
    },
    
    
    // Constructor
    initialize: function(models, options) {
      this.contents = options.contents;
      this.params   = options.params;
      this.listenTo(this.contents, 'reset change add remove', this._contentsReset);
      this._contentsReset(this.contents);
    },
    
    
    // Redirect fetch call to the underlying contents
    fetch: function() {
      if (!this.contents.fetched)
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
    
    
    // Resets the collection with the models from the given collection,
    // filtered by the sieve method
    _contentsReset: function(collection, options) {
      var models = this.sieve(this.contents.models);
      if (models != this.contents.models || this.length == 0 && models.length != 0)
        this.reset(models, options);
    },
    
    
    // Override this for getting the URL for the given item
    getUrl: function(item) {
      throw("NotImplementedException");
    }
    
    
  });
  
  
  return ContentsPart;

});