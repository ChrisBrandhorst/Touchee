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
      if (this.computed) {
        var comp = this.computed[attr];
        if (_.isFunction(comp))
          val = comp.call(this, val);
        else if (_.isString(comp))
          val = this.get(comp);
      }
      
      // If we have an array now, join it into a single string
      // or set to null if the array is empty
      if (_.isArray(val)) {
        val = val.length == 0 ? null : val.join(", ");
      }

      // If we have nothing yet, try the "unknown ..."
      if (!val) {
        val = I18n.models[ attr ];
        if (_.isObject(val))
          val = val.one;
        if (val)
          val = I18n.unknown + " " + val.toTitleCase();
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
    },


    // 
    artworkUrl: function(params) {
      return Touchee.getUrl(
        [this.collection.container.url(), "artwork/id", this.id].join('/'),
        params
      );
    },


    // 
    getDisplayLine1: function() {
    },
    getDisplayLine2: function() {
    },
    getDisplayLine: function(idx) {

    }
    
    
  });

  return Item;
  
});