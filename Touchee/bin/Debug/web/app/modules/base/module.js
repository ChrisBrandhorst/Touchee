define([
  'underscore',
  'Backbone',
  'Touchee'
], function(_, Backbone, Touchee) {
  
  
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
    // - Build a view for the given container and filter;
    // - Sets the view in the browser view;
    // - Fetch the contents of the view model.
    showContents: function(container, filter, fragment, browserView) {
      var view = this.buildView(container, filter, fragment);
      this.setView(view, browserView);
      this.fetchViewContents(view);
    },
    
    
    // Build the view object for the given container and filter.
    buildView: function(container, filter, fragment) {
      var viewClass = this.getViewClass(filter);
      if (!viewClass)
        return this.Log.error("No valid viewmodel class specified for module " + container.get('plugin') || 'base') + " (" + view + ")";
      
      var view = new viewClass({
        model:  container.buildViewModel(filter),
        filter: filter
      });
      view.fragment = fragment;
      return view;
    },
    
    
    // Gets the view class for the given filter
    getViewClass: function(filter) {
      return this.views[filter.get('view')];
    },
    
    
    // Sets the given view in the browser view
    setView: function(view, browserView) {
      browserView.setView(view);
      view.render();
    },
    
    // 
    fetchViewContents: function(view) {
      view.model.fetch();
    }
    
    
  });
  
  
  return Module;
  
  
});