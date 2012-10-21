define([
  'jquery',
  'underscore',
  'Backbone',
  
  'models/collections/media',
  'models/collections/containers',
  
  'models/contents',
  'models/filter',
  'models/control_request',
  
  'views/browser'
], function($, _, Backbone,
            Media, Containers,
            Contents, Filter, ControlRequest,
            BrowserView
){
  
  var AppRouter = Backbone.Router.extend({
    
    
    routes: {
      "":                                             "root",
      "media/:mid/containers":                        "containers",
      "media/:mid/groups/:group/containers":          "containers",
      "media/:mid/containers/:cid/contents":          "container",
      "media/:mid/containers/:cid/contents/*filter":  "container",
      "play/container/:cid/*filter":                  "play",
      "queue/:qid/:command":                          "control"
    },
    
    
    initialize: function() {
      
      // Bind global navigate event
      // _.extend(window, Backbone.Events);
      // window.on('navigate', function(url){
      //   if (typeof url == 'string')
      //     this.navigate(url, {trigger:true});
      // }, this);
      
      // Always go to root
      window.location.hash = "";
    },
    
    
    // Root method
    root: function() {
      if (!BrowserView.mediaListView.isEmpty())
        BrowserView.mediaListView.activatePage('first');
    },
    
    
    // A medium was selected from the nav list
    containers: function(mediumID, group) {
      var medium = this.getMedium(mediumID);
      if (!medium) return;
      BrowserView.navigate(medium, group);
    },
    
    
    // Show the contents of a single container 
    container: function(mediumID, containerID, filter) {
      
      // Get the medium
      var medium = this.getMedium(mediumID);
      if (!medium) return;
      
      // Get the container
      var container = medium.containers.get(containerID);
      if (!container)
        return this.Log.error("Container with id " + containerID + " cannot be found. Removed?");
      
      // Build filter object
      filter = new Filter(decodeURIComponent(filter || ""));
      
      // If we were not given any specific type to show in the filter, we get the first viewType
      var type = filter.get('type') || container.get('viewTypes')[0];
      if (!type)
        return this.Log.error("No type specified for container " + containerID);
      
      
      
      
      
      
      
      
      // Check if any attributes other then type was in the filter
      filter.unset('type');
      var filterBesidesType = _.keys(filter.attributes).length > 0;
      filter.set('type', type);
      
      // Store container view
      var containerView;
      
      // If this is the first view we open for this container
      if (!filterBesidesType) {
        // Get or create the view which contains the contents pages for this container / type combo
        containerView = BrowserView.getOrCreateContainerView(container, type);
        // Activate it
        BrowserView.activateContainerView(containerView);
      }
      
      // Else, get the currently active view
      else
        containerView = BrowserView.activeContainerView;
      
      // If we already have a view for the given filter, activate that page
      var existingPage;
      if (existingPage = containerView.getPage( filter.toString() ))
        return containerView.activatePage(existingPage);
      
      // Check which module we must load
      var module = container.get('module');
      if (!module) {
        var contentType = container.get('contentType');
        if (_.include(Touchee.knownContentTypes, contentType))
          module = contentType
      }
      var modulePath = module ? 'modules/' + module + '/module' : 'lib/touchee.module';
      
      // Get the processing module
      require([modulePath], function(Module){
        Module.name = module;
        Module.setContentPage(containerView, type, filter);
      });
      
    },
    
    
    // Send a play command to the server
    play: function(containerID, filter) {
      new ControlRequest({
        command:    'play',
        container:  containerID,
        filter:     new Filter(filter || "")
      }).save();
    },
    
    
    // Send a control command to the server
    control: function(queueID, command) {
      new ControlRequest({
        command:  command,
        queue:    queueID
      }).save();
    },
    
    
    // Safely get the medium with the given ID
    getMedium: function(mediumID) {
      var medium = Media.get(mediumID);
      if (!medium) {
        this.navigate("", {trigger:true});
        this.Log.error("Medium with id " + mediumID + " cannot be found. Removed? Going back to root");
        return false;
      }
      return medium;
    }
    
    
  });
  
  
  var initialize = function(baseURL) {
    var app_router = new AppRouter();
    Backbone.history.start({root:baseURL});
  };
  return {
    initialize: initialize
  };
  
});