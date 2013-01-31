define([
  'jquery',
  'underscore',
  'Backbone',
  'models/contents',
  'models/filter'
], function($, _, Backbone, Contents, Filter){
  
  var Container = Backbone.Model.extend({
    
    
    // The model used for the contents object
    contentsModel:    Contents,
    
    
    // The different views that are available for this container, together
    // with the corresponding viewmodel class.
    views: {
      // viewID: ViewModelClass
    },
    
    
    // Whether this container has one single set of contents, regardless of filter
    singleContents:   true,
    
    
    // Constructor
    initialize: function(attributes, options) {
      options || (options = {});
    },
    
    
    // Gets the URL for the container, optionally appended by a filter
    url: function(filter) {
      var url = Backbone.Model.prototype.url.call(this);
      if (filter) {
        filter = new Filter(filter);
        if (filter.count) url += "/" + filter.toString();
      }
      return url;
    },
    
    
    
    // Builds an instance of the ViewModel for the given filter
    buildViewModel: function(filter) {
      var viewModelClass  = this.views[filter.get('view')],
          contents        = this.buildContents(filter);
      return new viewModelClass(null, {
        contents: contents,
        filter:   filter
      });
    },
    
    
    // Builds an instance of the Contents for the given filter
    buildContents: function(filter) {
      var contents;
      
      if (this._contents)
        contents = this._contents;
      else {
        var contentsClass = this.getContentsClass(filter),
        contents = new contentsClass(null, {
          container:  this,
          filter:     filter
        });
        if (this.singleContents)
          this._contents = contents;
      }
      
      return contents;
    },
    
    
    // Returns the Contents class for the given filter
    getContentsClass: function(filter) {
      return Contents;
    }
    
    
    
    
    
    
    
    
    
    
    // getArtworkUrl: function(options) {
    //   options = _.extend({
    //     ratio: window.devicePixelRatio
    //   }, options || {});
    //   
    //   var item = options.item;
    //   if (item) delete options.item;
    //   
    //   options = $.param(options);
    //   return [this.url(), "/artwork?", options, (options.length ? "&" : ""), "item=", item ? encodeForFilter(item) : ""].join('');
    // }
    
  });
  
  return Container;
});