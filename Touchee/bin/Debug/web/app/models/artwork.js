define([
  'jquery',
  'underscore',
  'Backbone'
], function($, _, Backbone){
  
  // Internal cache for all artwork objects
  var _cache = {};
  
  
  // Artwork object
  var Artwork = Backbone.Model.extend({
    
    defaults: {
      queried: false
    },
    
    // Constructor
    initialize: function(attributes, options) {
      this.item = options.item;
      this.sizes = {};
    },
    
    
    // Returns whether the artwork is available
    exists: function() {
      return this.get('exists') === true;
    },
    
    
    // 
    isQueried: function() {
      return this.get('queried');
    },
    
    
    // 
    hasSize: function(size) {
      return !_.isUndefined(this.sizes[size || null]);
    },
    
    
    // 
    getSize: function(size) {
      return this.sizes[size];
    },
    
    
    // 
    setSize: function(size, width, height, url) {
      this.sizes[size || null] = {
        width:  width,
        height: height,
        url:    url
      };
      if (!this.has('ratio'))
        this.set('ratio', width / height);
      return this;
    },
    
    
    // 
    url: function(options) {
      options || (options = {});
      var url;
      
      // If we are asked for the largest already known image, get the URL
      if (options.largest) {
        var keys = _.keys(this.sizes).sort(function(a,b){return b == null || a < b;});
        if (keys[0]) url = this.sizes[keys[0]].url;
        delete options.largest;
      }
      
      // Get the base URL
      url = url || this.item.artworkUrl();
      
      // Build the full URL
      var query = $.param(options);
      if (query != "") url += (url.indexOf('?') == -1 ? "?" : "&") + query;
      return url;
    },
    
    
    //
    isSquare: function() { return this.get('ratio') == 1; },
    isLandscape: function() { return this.get('ratio') > 1; },
    isPortrait: function() { return this.get('ratio') < 1; },
    
    
    // 
    getColors: function(options) {
      options || (options = {});
      var artwork = this;
      var xhr = $.ajax({
        url:      this.url(),
        data:     {colors:true},
        success:  function(data, textStatus, jqXHR) {
          artwork.colors = data;
          if (options.success) options.success(artwork, artwork.colors);
          artwork.item.trigger('colors', artwork.colors, artwork.item);
        },
        error: options.error
      });
    }
    
    
  },{
    
    
    // 
    fetch: function(item, options) {
      options = _.extend({
        remote:   true,
        toCache:  true
      }, options);
      
      // Check for artwork URL
      if (!_.isFunction(item.artworkUrl)) {
        if (options.error) options.error("Item has no artworkUrl");
        return;
      }

      // Get artwork from cache
      var artwork = this.fromCache(item);

      // Build URL
      var query = {}, url;
      if (options.size) query.size = options.size;

      // If we have a cache
      if (artwork) {
        var exists = artwork.exists();
        // If the artwork does not exist
        if (exists === false) {
          if (options.none) options.none(artwork);
          return;
        }
        // If the artwork exists and the correct size is present
        else if (exists === true && artwork.hasSize(options.size)) {
          if (options.success) options.success(artwork, artwork.url(query));
          return;
        }
      }
      
      // Else, build new object
      else {
        artwork = new Artwork({},{item:item});
      }
      
      // No remote? Bail out
      if (!options.remote) {
        if (options.none) options.none();
        return;
      }
      
      // Get the image from remote. If it is cached by the browser, the onload will immediately run
      url = artwork.url(query);
      var img = new Image();
      img.onload = function() {
        artwork.set({
          exists:   true,
          queried:  true
        });
        artwork.setSize(options.size || null, img.width, img.height, url);
        if (options.toCache)
          _cache[item.url()] = artwork;
        if (options.success)
          options.success(artwork, url, img);
        if (options.colors && !artwork.colors)
          artwork.getColors();
        item.trigger('artwork', artwork, item);
      };
      img.onerror = function(){
        artwork.set({
          exists:   false,
          queried:  true
        });
        if (options.toCache) _cache[item.url()] = artwork;
        if (options.error) options.error();
      };
      img.onabort = options.error;
      
      // Load the image
      img.src = artwork.url(query);
      
    },
    
    
    // Gets the artwork object for the given item from the cache
    fromCache: function(item) {
      return _cache[item.url()];
    }
    
    
    
  });
  
  return Artwork;
  
});