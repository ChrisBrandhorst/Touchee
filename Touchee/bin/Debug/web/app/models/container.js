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
      var viewModelClass  = this._getViewModelClass(params.view),
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
        var contentsClass = this._getContentsClass(params),
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


    // Gets the view model class for the given view description
    // Implement the getViewModelClass method for custom behaviour
    // PRIVATE
    _getViewModelClass: function(view) {
      return (_.isFunction(this.getViewModelClass) && this.getViewModelClass.apply(this, arguments)) || this.views[view];
    },


    // Returns the Contents class for the given params
    // Implement the getContentsClass method for custom behaviour
    // PRIVATE
    _getContentsClass: function(view) {
      return (_.isFunction(this.getContentsClass) && this.getContentsClass.apply(this, arguments)) || Contents;
    }
    
    
  });
  
  return Container;
});