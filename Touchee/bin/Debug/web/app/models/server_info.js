define([
  'underscore',
  'Backbone',
  'Touchee'
], function(_, Backbone, T){
  
  // ServerInfo object
  var ServerInfo = Backbone.Model.extend({
    
    
    // Backbone model options
    url:      "server_info",
    
    
    // Contains all plugins
    plugins: {},
    
    
    // Constructor
    initialize: function(attributes, options) {
    },
    
    
    // Fetches the server info
    // After the fetch, loads the plugins
    fetch: function(options) {
      options || (options = {});
      var success = options.success, serverInfo = this;
      
      options.success = function(model, response, options) {
        if (_.isFunction(success))
          success = _.bind(success, this, model, response, options);
        serverInfo.loadPlugins(success);
        serverInfo.setName();
      };
      
      Backbone.Model.prototype.fetch.call(this, options);
    },
    
    
    // Loads all plugins
    loadPlugins: function(success) {
      var plugins     = this.plugins = {},
          keys        = this.get('plugins'),
          pluginCount = keys.length,
          loadedCount = 0;
      
      // Load plugins
      _.each(keys, function(key){
        
        // If plugin is already loaded, skip it
        if (plugins[key]) return;
        
        // Require plugin
        require(['plugins/' + key + '/plugin'], function(plugin){
          T.Log.info("ServerInfo:: Plugin loaded: " + key);
          plugins[key] = plugin;
          plugin.id = key;
          
          loadedCount++;
          if (loadedCount == pluginCount && _.isFunction(success))
            success();
        });
        
      });
    },
    
    
    // Gets the plugin with the given key
    getPlugin: function(key) {
      var plugin = this.plugins[key];
      return plugin instanceof T.Plugin ? plugin : null;
    },
    
    
    // Stores the currently available name so we can later retrieve the name
    // when we only have the host
    setName: function() {
      var names = JSON.parse(localStorage['names'] || "[]"),
          name  = this.get('name');
      
      // No name? No store...
      if (!name) return;
      
      var existing = _.find(names, function(n){ return n.host == window.location.host; });
      if (existing)
        existing.name = name;
      else
        names.push({
          host: window.location.host,
          name: name
        });
      localStorage['names'] = JSON.stringify(names);
    },
    
    
    // 
    getName: function() {
      var name = this.get('name');
      if (name) return name;
      
      var existing = _.find(JSON.parse(localStorage['names'] || "[]"), function(n){ return n.host == window.location.host; });
      if (existing)
        return existing.name;
      else
        return window.location.hostname;
    }
    
    
  });
  
  return new ServerInfo;
  
});