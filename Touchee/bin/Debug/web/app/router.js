define([
  'jquery',
  'underscore',
  'Backbone',
  
  'models/contents_module',
  
  'models/server_info',
  
  'models/collections/media',
  'models/collections/contents_containers',
  
  'models/config_container',
    
  'views/browser/index',
  'views/media/popup',
  'views/config/index'
], function($, _, Backbone,
            BaseContentsModule,
            ServerInfo,
            Media, ContentsContainers,
            ConfigContainer,
            BrowserView, MediaPopupView, ConfigView
){
  
  var AppRouter = Backbone.Router.extend({
    
    
    routes: {
      "media/:mid":                                 "medium",
      "media/:mid/containers/:cid":                 "container",
      "media/:mid/containers/:cid/*params":         "container",
      "config":                                     "config",
      "config/:id":                                 "config"
    },
    
    
    initialize: function() {
      
      // Create base module instance
      this.baseContentsModule = new BaseContentsModule;
      
      // Always go to root
      window.location.hash = "";
    },
    
    
    // A medium was selected from the nav list
    medium: function(mediumID) {
      var medium = this.getMedium(mediumID);
      if (!medium) return;
      
      var masters = medium.containers.masters();
      if (masters.length > 1)
        MediaPopupView.showMedium(medium);
      Backbone.history.navigate(masters.first().url(), {trigger:true});
    },
    
    
    // Show the contents of a single container 
    container: function(mediumID, containerID, paramsStr) {
      paramsStr || (paramsStr = "");

      // Get the container
      var container = this.getContainer(mediumID, containerID);
      if (!container) return;
      
      // Build params object
      params = Touchee.Params.parse(paramsStr);
      
      // If we were not given any specific type to show in the params, we get the first viewType
      var view = params.view || container.get('views')[0];
      
      // Find the base and full fragments
      delete params.view;
      var viewFragment  = Backbone.history.fragment.match(/media\/\d+\/containers\/[^\/]+/)[0] + "/view/" + view,
          fragment      = viewFragment + "/" + Touchee.Params.compose(params);
      params.view = view;
      Backbone.history.navigate(fragment, {replace:true});
      
      // Get the module for this container
      var plugin = ServerInfo.getPlugin(container.get('plugin')),
          module = (plugin && plugin.module) || this.baseContentsModule;
      
      // Build the view
      module.showContents(viewFragment, params, container);
    },


    // Show config
    config: function(id) {
      BrowserView.setView(ConfigView, ConfigContainer);
      if (id) {
        var section = Touchee.Config.sections.get(id);
        var view = _.isFunction(section.view) ? new section.view : section.view;
        view.section = section;
        ConfigView.setRight(view);
      }
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