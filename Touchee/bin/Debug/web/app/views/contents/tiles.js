define([
  'jquery',
  'underscore',
  'Backbone',
  'models/artwork',
  'views/contents/scroll_list'
], function($, _, Backbone, Artwork, ScrollListView) {
  
  var TilesView = ScrollListView.extend({
    
    // ScrollList properties
    dummy:        '<li>&nbsp;<span>&nbsp;</span></li>',
    listType:     'tiles',
    innerTagName: 'ul',
    indicesShow:  false,
    
    // Tiles view properties
    line1:        'id',
    line2:        'id',
    
    
    // Backbone view properties
    events: {
      'click .scrollable > ul > li': 'clickedTile'
    },
    
    
    // Additional calculation for the zoom state of the tiles
    calculateSizes: function() {
      ScrollListView.prototype.calculateSizes.apply(this, arguments);
      
      // Generate a dummy
      var $dummy = $( _.result(this, 'dummy') )
        .addClass('zoom')
        .css({
          position:   'absolute',
          visibility: 'hidden'
        })
        .appendTo(this.$inner);
      
      // Get the metrics
      var zoomMargins = _.map($dummy.css('margin').replace(/px/g, "").split(" "), function(m){return +m;}),
          size        = this.calculated.size;
      size.zoom = {
        margin: {
          top:    zoomMargins[0],
          right:  zoomMargins[1],
          bottom: zoomMargins[2],
          left:   zoomMargins[3]
        },
        inner: {
          height: $dummy[0].clientHeight,
          width:  $dummy[0].clientWidth
        }
      };
      
    },
    
    
    // Renders each item of the list
    renderItem: function(item, options) {
      var style = this.getTileStyle(item);
      
      var rendered = '<li data-id="' + item.id + '"' + (style ? ' style="'+style+'"' : '') + ">";
      
      if (this.line1)
        rendered += "<span>" + this.getAttributeValue(item, this.line1) + "</span>";
      if (this.line2)
        rendered += "<span>" + this.getAttributeValue(item, this.line2) + "</span>";
        
      rendered += "</li>";
      return rendered;
    },
    
    
    // Gets the value for the given attribute for the given model
    getAttributeValue: function(model, attr) {
      var val = attr.call ? attr.call(model, model) : model.get(attr);
      if ((!val || val == "") && _.isString(attr))
        val = this.getUnknownAttributeValue(model, attr);
      return val || "";
    },
    
    
    // Gets the unknown value for the given attribute of the model
    getUnknownAttributeValue: function(model, attr) {
      return "";
    },
    
    
    // Gets the style for the tile of the given item
    getTileStyle: function(item) {
      var artworkStyle;
      
      // Get artwork style if we have an artwork url for the item and the artwork is already cached
      if (this.getArtworkUrl) {
        var artwork = Artwork.fromCache( this.getArtworkUrl(item) );
        if (artwork) artworkStyle = this.getArtworkStyle(artwork, {string:true});
      }
      
      return artworkStyle;
    },
    
    
    // Gets the style used for displaying the artwork of the tile
    getArtworkStyle: function(artwork, options) {
      options || (options = {});
      
      // Get data
      var width   = artwork.get('width'),
          height  = artwork.get('height'),
          size    = options.size || this.calculated.size,
          style   = {};
      
      // The image
      style['background-image'] = "url(" + artwork.get('url') + ")";
          
      // Square artwork: nothing special
      if (height == width) { }
      
      // Portrait artwork
      else if (height > width) {
        var imgWidth          = Math.floor(width * (size.inner.height / height));
        style['width']        = imgWidth + 'px';
        style['margin-right'] = size.margin.right + (size.inner.width - imgWidth) + 'px';
      }
      
      // Landscape artwork
      else {
        var imgHeight       = Math.floor(height * (size.inner.width / width));
        style['height']     = style['padding-top'] = imgHeight + 'px';
        style['margin-top'] = size.margin.top + (size.inner.height - imgHeight) + 'px';
      }
      
      // Convert to string representation
      if (options.string) {
        style = _.map(_.keys(style), function(k) {
          return k + ":" + style[k];
        }).join(';');
      }
      
      return style;
    },
    
    
    // Called when the render is complete
    afterRender: function(items) {
      
      // Do nothing if we have no items in view or no artwork provisioning
      if (!items.count || !this.getArtworkUrl) return;
      
      // Store the last render items
      this._lastRender = items;
      
      // Start setting artwork on the first element in view
      this._setArtworkIter(
        items.firstInView,
        items.firstInView - (items.firstInView - items.first),
        items.firstInView + items.countInView - 1
      );
    },
    
    
    // Sets the artwork for the tiles iteratively
    _setArtworkIter: function(itemIdx, elOffset, lastItemIdx) {
      
      // Remember that we are busy with the current index
      this._lastRender.artworkIdx = itemIdx;
      
      // Function for doing the next item
      var view = this, doNext = function() {
        // Bail out if we do not have an artworkIdx anymore (new render) or if all tiles in view are done
        if (!_.isNumber(view._lastRender.artworkIdx) || view._lastRender.artworkIdx == lastItemIdx) return;
        view._setArtworkIter(
          itemIdx + 1,
          elOffset,
          lastItemIdx
        );
      };
      
      // Get the element
      var el = this.$inner[0].childNodes[itemIdx - elOffset];
      
      // See if we already have an image
      if (el.style.backgroundImage) {
        doNext();
      }
      
      // If not, load an image
      else {
        
        // Get some params
        var item  = this.model.models[itemIdx],
            url   = this.getArtworkUrl(item),
            view  = this;
        
        // Get the artwork
        Artwork.fetch(url, {
          success: function(artwork) {
            // If we have artwork, set it
            if (artwork instanceof Artwork) {
              var artworkStyle = view.getArtworkStyle(artwork);
              _.extend(el.style, artworkStyle);
              el._artworkStyle = artworkStyle;
              el._artwork = artwork;
            }
            // Do the next item
            doNext();
          },
          error: doNext
        });
        
      }
      
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
  
  return TilesView;
  
});