define([
  'jquery',
  'underscore',
  'Backbone',
  'Touchee', 
  
  'modules/base/module',
  
  'models/server_info',
  
  'models/collections/media',
  'models/collections/containers',
  
  'models/contents',
  'models/filter',
  'models/control_request',
    
  'views/browser',
  'views/media/popup'
], function($, _, Backbone, Touchee,
            BaseModule,
            ServerInfo,
            Media, Containers,
            Contents, Filter,
            ControlRequest,
            BrowserView, MediaPopupView
){
  
  var AppRouter = Backbone.Router.extend({
    
    
    routes: {
      "media/:mid":                                 "medium",
      "media/:mid/containers/:cid":                 "container",
      "media/:mid/containers/:cid/*filter":         "container",
      // "media/:mid/containers/:cid/play/*filter":      "play",
      // "queue/:qid/:command":                          "control"
    },
    
    
    initialize: function() {
      
      // Create base module instance
      this.baseModule = new BaseModule;
      
      // Always go to root
      window.location.hash = "";
    },
    
    
    // A medium was selected from the nav list
    medium: function(mediumID) {
      var medium = this.getMedium(mediumID);
      if (!medium) return;
      
      if (medium.containers.length > 1)
        MediaPopupView.showMedium(medium);
      else
        Backbone.history.navigate(medium.containers.first().url(), {trigger:true});
    },
    
    
    // Show the contents of a single container 
    container: function(mediumID, containerID, filter) {
      
      // Get the container
      var container = this.getContainer(mediumID, containerID);
      if (!container) return;
      
      // Build filter object
      filter = new Filter(decodeURIComponent(filter || ""));
      
      // If we were not given any specific type to show in the filter, we get the first viewType
      var view      = filter.get('view') || _.keys(container.views)[0],
          viewModel = container.views[view];
      if (!viewModel)
        return this.Log.error("No valid viewmodel class specified for container " + containerID) + " (" + view + ")";
      
      // Explicitly set the view in the filter and update the fragment
      filter.set('view', view);
      var fragment = [Backbone.history.fragment.match(/media\/\d+\/containers\/\d+/)[0], "/", filter.toString()].join("");
      Backbone.history.navigate(fragment, {replace:true});
      
      // Get the module for this container
      var plugin = ServerInfo.getPlugin(container.get('plugin')),
          module = (plugin && plugin.module) || this.baseModule;
      
      // Set selection of container in browser
      BrowserView.setSelectedContainer(container, view);
      
      // Build the view
      module.showContents(container, filter, fragment, BrowserView);
      
      
      // TODO: necessary?
      // Check if we navigated from within a container view or from the media list or view type buttons
      // Unset 'view' in filter, count attributes, re-set 'view'
    },
    
    
    // Send a play command to the server
    play: function(mediumID, containerID, filter) {
      
      // Get the container
      var container = this.getContainer(mediumID, containerID);
      if (!container) return;
      
      new ControlRequest({
        command:      'play',
        containerID:  containerID,
        filter:       new Filter(filter || "")
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
        this.navigate("", {replace:true,trigger:true});
        this.Log.error("Medium with id " + mediumID + " cannot be found. Removed? Going back to root");
        return null;
      }
      return medium;
    },
    
    
    // Safely get the container with the given ID from the medium with the given ID
    getContainer: function(mediumID, containerID) {
      var medium = this.getMedium(mediumID);
      if (!medium) return medium;
      var container = medium.containers.get(containerID);
      if (!container) {
        this.navigate("", {replace:true,trigger:true});
        this.Log.error("Container with id " + containerID + " cannot be found. Removed? Going back to root");
        return null;
      }
      return container;
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