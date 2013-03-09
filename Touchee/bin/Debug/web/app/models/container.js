define([
  'jquery',
  'underscore',
  'Backbone',
  'Touchee',
  'models/contents'
], function($, _, Backbone, Touchee, Contents){
  
  var Container = Backbone.Model.extend({
    
    
    // The model used for the contents object
    contentsModel:      Contents,
    
    
    // The model used for the items within the contents object
    contentsItemModel:  null,
    
    
    // The different views that are available for this container, together
    // with the corresponding viewmodel class.
    views: {
      // viewID: ViewModelClass
    },
    
    
    // Whether this container has one single set of contents, regardless of params
    singleContents:   true,
    
    
    // Constructor
    initialize: function(attributes, options) {
    },
    
    
    // Gets the URL for the container, optionally appended by params
    url: function(params) {
      return Touchee.getUrl(Backbone.Model.prototype.url.call(this), params);
    },
    
    
    // Builds an instance of the ViewModel for the given params
    buildViewModel: function(params) {
      var viewModelClass  = this.views[params.view],
          contents        = this.buildContents(params);
      return new viewModelClass(null, {
        contents: contents,
        params:   params
      });
    },
    
    
    // Builds an instance of the Contents for the given params
    buildContents: function(params) {
      var contents;
      
      if (this._contents)
        contents = this._contents;
      else {
        var contentsClass = this.getContentsClass(params),
        contents = new contentsClass(null, {
          container:  this,
          params:     params,
          model:      this.contentsItemModel
        });
        if (this.singleContents)
          this._contents = contents;
      }
      
      return contents;
    },
    
    
    // Returns the Contents class for the given params
    getContentsClass: function(params) {
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
    //   return [this.url(), "/artwork?", options, (options.length ? "&" : ""), "item=", item ? encodeForParams(item) : ""].join('');
    // }
    
  });
  
  return Container;
});