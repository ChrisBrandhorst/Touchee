define([
  'underscore',
  'Backbone',
  'Touchee',
  'models/container',
  'models/contents',
  'views/contents/table_base'
], function(_, Backbone, Touchee, Container, Contents, TableBaseView) {
  
  
  // Touchee.Module
  // -----------------
  
  // Modules receive requests for content pages from the user interface and process them
  // specificly for the module type
  var Module = function() {
    this.initialize.apply(this, arguments);
  };
  Module.extend = Backbone.Model.extend;
  
  
  // Set up all inheritable **Touchee.Module** properties and methods.
  _.extend(Module.prototype, Backbone.Events, {
    
    
    // Logger
    Log: Touchee.Log,
    
    
    // Initialize is an empty function by default. Override it with your own
    // initialization logic.
    initialize: function(){},
    
    
    showContent: function(container, filter, containerView, fragment) {
      // Build the contents object for the given container and filter
      var contents = this.buildContents(container, filter);
      // Show the contents in the active containerView
      this.buildContentsView(contents, containerView, fragment);
      // Retrieve the actual contents
      this.fetchContents(contents);
    },
    
    
    
    // Build the container object for the given container attribetus
    buildContainer: function(attrs, options) {
      var containerClass = this.getContainerClass(attrs.type);
      return new containerClass(attrs, options);
    },
    
    
    // Get the container class for the given type
    getContainerClass: function(type) {
      return Container;
    },
    
    
    // Build the contents object for the given container and filter
    buildContents: function(container, filter) {
      var type          = filter.get('type'),
          contentsClass = this.getContentsClass(type);
      return contents = new contentsClass({
        type: type
      },{
        container:  container,
        filter:     filter
      });
    },
    
    
    // Get the contents class for the given type
    getContentsClass: function(type) {
      return Contents;
    },
    
    
    // Show the contents in the given containerView
    buildContentsView: function(contents, containerView, fragment) {
      var contentsViewClass = this.getContentsViewClass(contents.getViewType(), contents);
      var contentsView = new contentsViewClass({
        contents: contents,
        back:     containerView.isEmpty() ? false : containerView.activePage.contents.getTitle(),
        fragment: fragment
      });
      this.showContentsView(containerView, contentsView);
    },
    
    
    // Get the view class for the given
    getContentsViewClass: function(type, contents) {
      return TableBaseView;
    },
    
    
    // Default setContentsView
    showContentsView: function(containerView, itemView) {
      containerView.storePage(itemView.fragment, itemView);
      containerView.activatePage(itemView);
      itemView.render();
    },
    
    
    // Fetch the actual contents
    fetchContents: function(contents, options) {
      contents.fetch(options);
    }
    
    
  });
  
  
  return Module;
  
  
});