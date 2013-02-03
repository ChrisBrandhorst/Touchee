define([
  'underscore',
  'Backbone',
  'Touchee'
], function(_, Backbone, Touchee){
  
  // Internal cache for all artwork objects
  var _cache = {};
  
  
  // Artwork object
  var Artwork = Backbone.Model.extend({
    
    initialize: function(attributes, options) {
      this.image = options.image;
      this.set({
        height: this.image.height,
        width:  this.image.width
      });
    }
    
  },{
    
    
    // 
    create: function(url, image) {
      return new Artwork({url:url}, {image:image});
    },
    
    
    // 
    fetch: function(url, options) {
      options = _.extend({
        remote:   true,
        toCache:  true
      }, options);
      
      // Get artwork from cache
      var artwork = this.fromCache(url);
      
      // If we have a cache, do success
      if (artwork === false || artwork instanceof Artwork) {
        if (options.success) options.success(artwork);
        return;
      }
      
      // No remote? Bail out
      if (!options.remote) {
        if (options.error) options.error();
        return;
      }
      
      // Get the image from remote. If it is cached by the browser, the onload will immediately run
      var img = new Image();
      img.onload = function() {
        artwork = Artwork.create(url, this);
        if (options.toCache) _cache[url] = artwork;
        if (options.success) options.success(artwork);
      };
      img.onerror = function(){
        if (options.toCache) _cache[url] = false;
        if (options.error) options.error();
      };
      img.onabort = options.error;
      img.src = url;
      
    },
    
    
    // Gets the artwork object for the given URL from the cache
    fromCache: function(url) {
      var artwork = _cache[url];
      return artwork instanceof Artwork ? artwork : (artwork === false ? false : null);
    },
    
    
    // // Returns whether the given URL is already cached
    // isCached: function(url) {
    //   return this.fromCache(url) !== null;
    // }
    
    
  });
  
  return Artwork;
  
});