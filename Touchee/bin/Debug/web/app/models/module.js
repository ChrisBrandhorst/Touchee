define([
  'underscore',
  'Backbone',
  'Touchee',
  'views/browser'
], function(_, Backbone, Touchee, BrowserView) {
  
  
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
    
    
    // Initialize is an empty function by default. Override it with your own
    // initialization logic.
    initialize: function(){},
    
    
    // Build the container object for the given container attribetus
    buildContainer: function(attrs, options) {
      var containerClass = this.getContainerClass(attrs.type);
      return new containerClass(attrs, options);
    },
    
    
    // Get the container class for the given type
    getContainerClass: function(type) {
      return Container;
    },
    
    
    // Shows the contents for the given parameters. Default implementation is as follows:
    // - Build a view for the given container and params;
    // - Sets the view in the browser view;
    // - Fetch the contents of the view model;
    // - Start the inner navigation of the view.
    showContents: function(container, params, fragment) {
      var view = this.getView(fragment) || this.buildView(container, params, fragment);
      this.setView(view);
      this.fetchViewContents(view);
      this.navigate(view, params);
    },
    
    
    // Gets the view for the given fragment (or null if it does not exist)
    getView: function(fragment) {
      return BrowserView.getView(fragment);
    },
    
    
    // Build the view object for the given container and params.
    buildView: function(container, params, fragment) {
      var view      = params.view,
          viewClass = this.views[view];
      if (!viewClass)
        return this.Log.error("No valid view class specified for module " + (container.get('plugin') || 'base') + " (" + params.view + ")");
      
      var viewInstance = new viewClass({
        model:  container.buildViewModel(params)
      });
      viewInstance.fragment = fragment;
      
      return viewInstance;
    },
    
    
    // Sets the given view in the browser view
    setView: function(view) {
      BrowserView.setView(view);
    },
    
    
    // 
    fetchViewContents: function(view) {
      view.model.fetch();
    },
    
    
    //
    navigate: function(view, params) {
      // view.navigate(params);
    }
    
    
  });
  
  
  return Module;
  
  
});