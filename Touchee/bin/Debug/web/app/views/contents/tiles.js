define([
  'jquery',
  'underscore',
  'Backbone',
  'models/artwork',
  'views/contents/scroll_list',
  'text!views/contents/tiles_details.html'
], function($, _, Backbone, Artwork, ScrollListView, tilesDetailsTemplate) {
  tilesDetailsTemplate = _.template(tilesDetailsTemplate);
  
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
    
    
    // Renders each item of the list
    renderItem: function(item, options) {
      var zoom      = this.data.zoomed == item,
          artwork   = this._getArtwork(item),
          style     = artwork ? this.getArtworkStyle(artwork, {string:true,zoom:zoom}) : null,
          klass     = zoom ? "zoom" : null;
      
      var rendered = '<li' + (style ? ' style="'+style+'"' : '') + (klass ? ' class="'+klass+'"' : '') + ">";
      
      if (this.line1)
        rendered += "<span>" + this.getAttributeValue(item, this.line1) + "</span>";
      if (this.line2)
        rendered += "<span>" + this.getAttributeValue(item, this.line2) + "</span>";
        
      rendered += "</li>";
      
      return rendered;
    },
    
    
    // Gets the artwork object for the given item or element
    _getArtwork: function(itemOrEl) {
      if (!this.getArtworkUrl) return null;
      var item = itemOrEl instanceof Backbone.Model ? itemOrEl : this.getItem(itemOrEl);
      return Artwork.fromCache(this.getArtworkUrl(item));
    },
    
    
    // Gets the style used for displaying the artwork of the tile
    getArtworkStyle: function(artwork, options) {
      options || (options = {});
      
      // Get data
      var width   = artwork.get('width'),
          height  = artwork.get('height'),
          size    = options.zoom ? this.calculated.size.zoom : this.calculated.size,
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
              
              var artworkStyle  = view.getArtworkStyle(artwork, {zoom: view.data.zoomed == item}),
                  $el           = $(el).addClass('noanim');
              _.extend(el.style, artworkStyle);
              _.defer(function(){
                $el.removeClass('noanim');
              });
            }
            // Do the next item
            doNext();
          },
          error: doNext
        });
        
      }
      
    },
    
    
    // (un)Zoomes the given tile
    zoomTile: function($el, zoom) {
      var el = $el[0];
      
      // Unzoom the element
      if (zoom === false) {
        var artwork = this._getArtwork($el);
        if (artwork)
          _.extend(el.style, this.getArtworkStyle(artwork));
        $el.removeClass('zoom');
        delete this.data.zoomed;
      }
      
      // Zoom the element
      else if (zoom === true) {
        
        // Unzoom the last zoomed element, if any
        var $zoomed = $el.siblings('.zoom');
        if ($zoomed.length)
          this.zoomTile($zoomed, false);
        
        // If we have any artwork, set the style for the zoomed version
        var item    = this.getItem($el),
            artwork = this._getArtwork(item);
        if (artwork) {
          var zoomStyle = this.getArtworkStyle(artwork, {zoom:true});
          _.extend(el.style, zoomStyle);
        }
        
        // Add the class
        $el.addClass('zoom');
        
        // Remember the item that was zoomed
        this.data.zoomed = item;
        
      }
      
      // Toggle zoom
      else {
        this.zoomTile($el, !$el.hasClass('zoom'));
      }
      
      return $el.hasClass('zoom');
    },
    
    
    // 
    showDetails: function($el, remove) {
      var existing = this.details,
          $details;
      
      
      // Remove the detail view if asked
      if ($el === false || remove) {
        if (!existing) return;
        _.defer(function(){
          ($details = existing.$el)
            .css('-webkit-transform', "translate3d(0," + (existing.top + (existing.newHeight || 0)) + "px,0)")
            .children('.cover')
            .on('webkitTransitionEnd', function(){ $details.remove(); })
            .removeClass('open')
            .css('-webkit-transform', "")
          existing.$moved.css('-webkit-transform', "");
        });
        delete this.details;
        return;
      }
      
      
      // Calculate after which tile the details should be shown
      var elIdx     = $el.prevAll().length,
          afterIdx  = elIdx + (this.calculated.capacity.hori - elIdx % this.calculated.capacity.hori) - 1,
          $after    = $el.parent().children().eq(Math.min(afterIdx, this.getCount()-1)),
          onSameRow = existing && existing.$after[0] == $after[0];
      
      // Build the details element
      var item      = this.getItem($el),
          content   = this.getDetailsContent(item),
          $details  = $( tilesDetailsTemplate({content:content}) ).addClass('dummy').insertBefore(this.$inner);
      
      
      // Start the props object
      var props = {
        // The data item that was clicked
        item:             item,
        // The details element
        $el:              $details,
        // The index after which the details view was injected
        afterIdx:         afterIdx,
        // The element after which the details view was injected
        $after:           $after,
        // The items that should be moved
        $moved:           $after.nextAll(),
        // Whether the details is opened on the same row as the current details (if exists)
        onSameRow:        onSameRow,
        // Whether the details is opened above the current details (if exists)
        newAboveCurrent:  existing && !onSameRow ? afterIdx < existing.afterIdx : (void 0),
        // Calculate arrow position
        arrowLeft:        $el.position().left + $el.outerWidth(true) / 2,
        // Calculate the height of the details
        height:           $details.outerHeight(),
        // Calculate the top of the new details
        top:              onSameRow ? existing.top : $after[0].offsetTop + $after.outerHeight(true) - $after.css('margin-top').numberValue()
      };
      
      
      // If the new details is on the same row, we should replace the content
      if (props.onSameRow) {
        
        // Fix the height to the max of the new and existing height
        existing.$el.css('height', Math.max(props.height, existing.height));
        
        // Get the old and new content elements
        var $oldContent = existing.$el.find('.content').addClass('outgoing'),
            $content    = $details.find('.content').addClass('incoming').insertAfter($oldContent);
        
        // Remove the old content after the transition
        $oldContent.on('webkitTransitionEnd', function(){
          $oldContent.remove();
          existing.$el.css('height', "");
        });
        
        // Defer the removal of the hide class on the new content
        _.defer(function(){ $content.removeClass('incoming'); });
        
        // Remove the dummy details, and redirect the var to the existing one
        $details.remove();
        $details = props.$el = existing.$el;
      }
      
      
      // If the new details is to be shown above the current one, move the new details earlier in the DOM
      if (props.newAboveCurrent === true) $details.prependTo(this.$scroller);
      
      // 
      var startTop = props.top;
      if (props.newAboveCurrent === false) startTop += props.height;
      
      // Remove the existing details if a new details is not on the same row as the existing
      if (props.onSameRow === false) {
        if (props.newAboveCurrent === true)
          existing.newHeight = props.height;
        this.showDetails(false);
      }
      
      
      $details
        // Set initial top position
        .css('-webkit-transform', "translate3d(0," + startTop + "px,0)")
        // Set arrow position
        .children('svg')
        .css('-webkit-transform', "translate3d(" + (-1000 + props.arrowLeft) + "px,0,0)");
      // Set other arrow position
      $details
        .find('> .cover > .arrow')
        .css('left', props.arrowLeft + 'px');
      
      // 
      _.defer(function(){
        
        // Move the tiles after the details down
        props.$moved.css('-webkit-transform', "translate3d(0," + props.height + "px,0)");
        
        // Slide the details open
        $details
          .removeClass('dummy')
          .css('-webkit-transform', "translate3d(0," + props.top + "px,0)")
          .children('.cover')
          .addClass('open')
          .css('-webkit-transform', "translate3d(0," + (props.height-1) + "px,0)");
      });
      
      
      // Store props
      this.details = props;
      
      // Set styling
      this.setDetailsStyle(this.details.$el, item);
      
      return this.details.$el;
    },
    
    
    //
    setDetailsStyle: function($details, item) {
      
    }
    
    
    
  });
  
  return TilesView;
  
});