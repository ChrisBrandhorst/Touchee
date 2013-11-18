define([
  'underscore',
  'Backbone',
  'models/contents_container',
  'views/browser/index',
  'views/contents/split',
  'views/config/index'
], function(_, Backbone, ContentsContainer, BrowserView, SplitView, ConfigView) {
  
  
  // Touchee.ContentsModule
  // -----------------
  
  // Modules receive requests for content pages from the user interface and process them
  // specificly for the module type
  var ContentsModule = Touchee.ContentsModule = function() {
    this.initialize.apply(this, arguments);
  };
  ContentsModule.extend = Backbone.Model.extend;
  

  // Set up all inheritable **Touchee.ContentsModule** properties and methods.
  _.extend(ContentsModule.prototype, Backbone.Events, {
    
    
    // Logger
    Log: Touchee.Log,
    
    
    // The different views that are available for this module, together
    // with the corresponding view class.
    views: {
      // viewID: ViewClass
      config: ConfigView
    },


    // The default contents container model
    contentscontentsContainerModel: ContentsContainer,

    
    // Initialize is an empty function by default. Override it with your own
    // initialization logic.
    initialize: function(){},
    
    
    // Build the container object for the given container attributes
    // VIRTUAL
    buildContainer: function(attrs, options) {
      return new this.contentsContainerModel(attrs, options);
    },
    
    
    // Shows the contents for the given parameters. Default implementation is as follows:
    // - Build a view for the given container and params;
    // - Sets the view in the browser view;
    // - Fetch the contents of the view model;
    // - Start the inner navigation of the view.
    showContents: function(fragment, params, container) {
      var existingView = this.getView(fragment);
      var view = existingView || this.buildView(container, params, fragment);
      if (!view) return;
      this.setView(view, params);
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
      var viewClass = viewClass || this.views[params.view];

      if (!viewClass)
        return this.Log.error("No valid view class specified for module " + (container.get('plugin') || 'base') + " and view " + params.view);

      var options = {};
      if (_.isFunction(container.buildViewModel))
        options.model = container.buildViewModel(params, viewClass);

      var viewInstance = viewClass.prototype ? new viewClass(options) : viewClass;
      viewInstance.fragment = fragment;
      
      return viewInstance;
    },
    
    
    // Sets the given view in the browser view
    setView: function(view, params) {
      BrowserView.setView(view, view.model.contents.container, params.view);
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
  
  
  return ContentsModule;
  
  
});