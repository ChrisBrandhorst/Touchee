define([
  'jquery',
  'underscore',
  'Backbone',
  'models/artwork',
  'views/contents/tiles'
], function($, _, Backbone, Artwork, TilesView) {
  
  var AlbumsView = TilesView.extend({
    
    // ScrollList properties
    contentType:    'albums',
    indexAttribute: 'albumArtistSort',
    
    
    // Tiles properties
    line1:    'album',
    line2:    'albumArtist',
    
    
    // Constructor
    initialize: function(options) {
      TilesView.prototype.initialize.apply(this, arguments);
      // this.filter = options.filter;
      this.model.on('reset add remove change', this.contentChanged, this);
    },
    
    
    // Gets the model count
    getCount: function() {
      return this.model.length;
    },
    
    
    // Gets the models
    getModels: function(items) {
      return this.model.models.slice(items.first, items.first + items.count);
    },
    
    
    // Gets the unknown value for the given attribute of the model
    getUnknownAttributeValue: function(model, attr) {
      var val = I18n.unknown;
      switch (attr) {
        case 'album':       val += " " + I18n.p.music.models.album.one;   break;
        case 'albumArtist': val += " " + I18n.p.music.models.artist.one;  break;
      }
      return val;
    },
    
    
    // Gets the artwork url for the given item
    getArtworkUrl: function(item) {
      return item.artworkUrl ? item.artworkUrl({size:this.calculated.size.inner.width}) : null;
    },
    
    
    // 
    clickedTile: function(ev) {
      var $el = $(ev.target).closest('li')
      this.zoomTile($el);
    },
    
    
    // 
    zoomTile: function($el, zoom) {
      zoom = zoom === false ? false : true;
      var el = $el[0];
      
      // Unzoom the element
      if (!zoom) {
        _.extend(el.style, el._artworkStyle);
        $el.removeClass('zoom');
      }
      
      // Zoom the element
      else {
        
        // Unzoom the last zoomed element, if any
        if (this._$lastZoomEl)
          this.zoomTile(this._$lastZoomEl, false);
        
        // Get some props
        var item        = this.model.get($el.attr('data-id')),
            artworkUrl  = this.getArtworkUrl(item),
            artwork     = Artwork.fromCache(artworkUrl);
        
        // If we have any artwork, set the style for the zoomed version
        if (artwork) {
          var zoomStyle = this.getArtworkStyle(artwork, {size:this.calculated.size.zoom});
          _.extend(el.style, zoomStyle);
        }
        
        // Add the class
        $el.addClass('zoom').siblings().removeClass('zoom');
        
        // Set the last zoom el
        this._$lastZoomEl = $el;
        
      }
      
    }
    
    
  });
  
  return AlbumsView;
  
});