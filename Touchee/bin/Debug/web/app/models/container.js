define([
  'jquery',
  'underscore',
  'Backbone',
  'models/contents'
], function($, _, Backbone, Contents){
  
  var Container = Backbone.Model.extend({
    
    
    // The model used for the contents object
    contentsModel:      Contents,
    
    
    // The model used for the items within the contents object
    contentsItemModel:  null,
    
    
    // The different views that are available for this container
    views: [],
    
    
    // Whether this container has one single set of contents, regardless of params
    singleContents:   true,
    
    
    // Constructor
    initialize: function(attributes, options) {
    },
    
    
    // Gets the URL for the container, optionally appended by params
    url: function(params) {
      return Touchee.getUrl(Backbone.Model.prototype.url.call(this), params);
    },
    
    
    // Builds an instance of the Contents for the given params
    buildContents: function(params) {
      var contents;
      
      if (this._contents)
        contents = this._contents;
      else {
        contents = new this.contentsModel(null, {
          container:  this,
          params:     params,
          model:      this.contentsItemModel
        });
        if (this.singleContents)
          this._contents = contents;
      }
      
      return contents;
    },


    // Called when the server notifies the client of the changing of the contents
    // VIRTUAL
    notifyContentsChanged: function() {
      this.trigger('notifyContentsChanged');
    }

    
  });
  
  return Container;
});