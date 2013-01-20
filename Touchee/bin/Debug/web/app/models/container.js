define([
  'jquery',
  'underscore',
  'Backbone',
  'models/contents'
], function($, _, Backbone, Contents){
  
  var Container = Backbone.Model.extend({
    
    
    // The model used for the contents object
    contentsModel:      Contents,
    
    // The different views that are available for this container, together
    // with the corresponding viewmodel class.
    views: {
      // viewID: ViewModelClass
    },
    
    
    // Constructor
    initialize: function(attributes, options) {
      options || (options = {});
    },
    
    
    // Builds an instance of the ViewModel for the given filter
    buildViewModel: function(filter) {
      var viewModelClass  = this.views[filter.get('view')],
          isCollection    = !!viewModelClass.prototype.model,
          contents        = this.buildContents(filter);
      return new viewModelClass(isCollection ? [] : {}, {
        contents: contents,
        filter:   filter
      });
    },
    
    
    // Builds an instance of the Contents for the given filter
    buildContents: function(filter) {
      var contentsClass = this.getContentsClass(filter),
          isCollection  = !!contentsClass.prototype.model;
      return new contentsClass(isCollection ? [] : {}, {
        container:  this,
        filter:     filter
      });
    },
    
    
    // Returns the Contents class for the given filter
    getContentsClass: function(filter) {
      return Contents;
    },
    
    
    
    
    
    
    
    
    
    
    getArtworkUrl: function(options) {
      options = _.extend({
        ratio: window.devicePixelRatio
      }, options || {});
      
      var item = options.item;
      if (item) delete options.item;
      
      options = $.param(options);
      return [this.url(), "/artwork?", options, (options.length ? "&" : ""), "item=", item ? encodeForFilter(item) : ""].join('');
    }
    
  });
  
  return Container;
});