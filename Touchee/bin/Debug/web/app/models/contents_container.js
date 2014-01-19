define([
  'underscore',
  'Backbone',
  'models/container',
  'models/contents',
  'models/item'
], function(_, Backbone, Container, Contents, Item){
  
  var ContentsContainer = Container.extend({
    
    
    // The model used for the contents object
    contentsModel:      Contents,
    
    
    // The model used for the items within the contents object
    contentsItemModel:  Item,
    
    
    // Whether this contents container has one single set of contents, regardless of params
    singleContents:     true,
    
    
    // Gets the URL for the contents container, optionally appended by params
    url: function(params) {
      return Touchee.buildUrl(
        Backbone.Model.prototype.url.call(this),
        params
      );
    },
    
    
    // Builds a view model for the given parameters
    buildViewModel: function(params, viewClass) {
      var viewModelClass = viewClass && viewClass.prototype.viewModel;

      if (!viewModelClass)
        return this.Log.error("No valid view model class specified for module " + (this.get('plugin') || 'base') + " and view " + params.view);

      return new viewModelClass(null, {
        contents: this.buildContents(params),
        params:   params
      });
    },


    // Builds an instance of the Contents for the given params
    buildContents: function(params) {
      var contents;
      
      if (this.singleContents && this._contents)
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
    },


    // Whether this contents container is a master contents container
    isMaster: function() {
      return !_.isNumber(this.get('masterID'));
    },


    // Gets the master contents container for this contents container
    getMaster: function() {
      return this.collection.get(this.get('masterID'));
    }

    
  });
  
  return ContentsContainer;
});