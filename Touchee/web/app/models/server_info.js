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
    initialize: function() {
      this.on('change:plugins', this.loadPlugins, this);
    },
    
    
    // Loads all plugins
    loadPlugins: function() {
      var plugins = this.plugins = {};
      
      _.each(this.get('plugins') || [], function(p){
        T.Log.info("ServerInfo:: Loading plugin: " + p);
        require(['plugins/' + p + '/web/plugin.js'], function(plugin){
          // T.Log.info("ServerInfo:: Plugin loaded: " + p);
          plugins[p] = plugin;
          plugin.id = p;
        });
      });
    },
    
    
    //
    getPlugin: function(plugin) {
      var p = this.plugins[plugin];
      return p instanceof Touchee.Plugin ? p : null;
    }
    
    
  });
  
  return new ServerInfo;
  
});