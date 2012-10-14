define([
  'jquery',
  'underscore',
  'Backbone'
], function($, _, Backbone){
  
  var Container = Backbone.Model.extend({
    
    getArtworkUrl: function(options) {
      options = _.extend({
        ratio: window.devicePixelRatio
      }, options || {});
      
      var item = options.item;
      if (item) delete options.item;
      
      options = $.param(options);
      return [this.url(), "/artwork?", options, (options.length ? "&" : ""), "item=", item ? encodeForFilter(item) : ""].join("");
    }
    
  });
  
  return Container;
});