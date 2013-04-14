define([
  'underscore',
  'Backbone'
], function(_, Backbone){
  
  var Item = Backbone.Model.extend({
    
    // Override get method for getting custom values for attributes (e.g. "Unknown ...").
    // If the attribute name is suffixed with an $, the display value is retrieved.
    // Implement get$ for extra custom behaviour.
    get: function(attr) {

      // See if a custom value is required
      var custom = attr.charAt(attr.length - 1) == '$';
      if (custom) attr = attr.slice(0, -1);
      
      // First, get the original value and return it if not custom
      var val = Backbone.Model.prototype.get.call(this, attr);
      if (!custom) return val;
      
      // Then go for a custom display if present
      if (this.computed && _.isFunction(this.computed[attr]))
        val = this.computed[attr].call(this, val);
      
      // If we have nothing yet, try the "unknown ..."
      if (!val) {
        var container = this.getContainer();
        if (container) {
          var plugin = container.get('plugin');
          if (plugin) {
            var pluginLocale = I18n.p[plugin];
            if (pluginLocale && pluginLocale.models)
              val = pluginLocale.models[ attr ];
              if (_.isObject(val))
                val = val.one;
              if (val)
                val = I18n.unknown + " " + val.toTitleCase();
          }
        }
      }

      return val;
    },


    // Gets the container this item belongs to
    getContainer: function() {
      return this.collection.container;
    },


    //
    url: function() {
      return this.collection.url() + "/item/" + this.id;
    }
    
    
  });

  return Item;
  
});