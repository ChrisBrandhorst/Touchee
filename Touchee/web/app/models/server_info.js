define([
  'underscore',
  'Backbone',
  'Touchee'
], function(_, Backbone, Touchee){
  
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
      return plugin instanceof Touchee.Plugin ? plugin : null;
    }
    
    
  });
  
  return new ServerInfo;
  
});