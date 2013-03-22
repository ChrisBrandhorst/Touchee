define([
  'underscore',
  'Backbone',
  'Touchee',
  'models/container',
  'views/browser',
  'views/contents/split'
], function(_, Backbone, Touchee, Container, BrowserView, SplitView) {
  
  
  // Touchee.Module
  // -----------------
  
  // Modules receive requests for content pages from the user interface and process them
  // specificly for the module type
  var Module = Touchee.Module = function() {
    this.initialize.apply(this, arguments);
  };
  Module.extend = Backbone.Model.extend;
  

  // Set up all inheritable **Touchee.Module** properties and methods.
  _.extend(Module.prototype, Backbone.Events, {
    
    
    // Logger
    Log: Touchee.Log,
    
    
    // The different views that are available for this module, together
    // with the corresponding view class.
    views: {
      // viewID: ViewClass
    },


    // The default container model
    containerModel: Container,

    
    // Initialize is an empty function by default. Override it with your own
    // initialization logic.
    initialize: function(){},
    
    
    // Build the container object for the given container attributes
    // VIRTUAL
    buildContainer: function(attrs, options) {
      return new this.containerModel(attrs, options);
    },
    
    
    // Shows the contents for the given parameters. Default implementation is as follows:
    // - Build a view for the given container and params;
    // - Sets the view in the browser view;
    // - Fetch the contents of the view model;
    // - Start the inner navigation of the view.
    showContents: function(container, params, fragment) {
      var existingView = this.getView(fragment);
      var view = existingView || this.buildView(container, params, fragment);
      this.setView(view);
      if (existingView)
        this.navigate(view, params, fragment);
      else
        this.fetchViewContents(view);
    },
    
    
    // Gets the view for the given fragment (or null if it does not exist)
    getView: function(fragment) {
      return BrowserView.getView(fragment);
    },
    
    
    // Build the view object for the given container and params.
    buildView: function(container, params, fragment, viewClass) {
      var view            = params.view,
          viewClass       = viewClass || this.views[view],
          viewModelClass  = viewClass.prototype.viewModel;

      if (!viewClass)
        return this.Log.error("No valid view class specified for module " + (container.get('plugin') || 'base') + " (" + params.view + ")");
      if (!viewModelClass)
        return this.Log.error("No valid view model class specified for module " + (container.get('plugin') || 'base') + " (" + params.view + ")");

      var viewModel = new viewModelClass(null, {
        contents: container.buildContents(params),
        params:   params
      });

      var viewInstance = new viewClass({
        model:  viewModel
      });
      viewInstance.fragment = fragment;
      
      return viewInstance;
    },
    
    
    // Sets the given view in the browser view
    setView: function(view) {
      BrowserView.setView(view);
    },
    
    
    // Fetch the contents of a view
    fetchViewContents: function(view) {
      view.model.fetch();
    },
    
    
    // Navigate further into a view
    navigate: function(view, params, fragment) {
      // If the view has its own custom navigate, do that
      if (_.isFunction(view.navigate))
        view.navigate(params, fragment, this);
    }
    
    
  });
  
  
  return Module;
  
  
});