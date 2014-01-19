define([
  'underscore',
  'Backbone'
], function(_, Backbone){
  
  var Item = Backbone.Model.extend({
    

    // Gets the container this item belongs to
    getContainer: function() {
      return this.collection && this.collection.container || this.container;
    },


    // Gets the plugin key this item belongs to
    getPluginKey: function() {
      var container = this.getContainer();
      return container && container.get('plugin');
    },


    //
    url: function() {
      return this.collection.url() + "/item/" + this.id;
    },


    // 
    artworkUrl: function(params) {
      var container = this.getContainer();
      return container && Touchee.buildUrl(
        container.url(),
        "artwork/id",
        this.id,
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

  return Item.including(Backbone.SmartGet);
  
});