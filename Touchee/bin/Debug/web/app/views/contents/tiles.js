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
    showIndex:    false,
    extraRows:    10,
    
    
    // Tiles view properties
    line1:        'id',
    line2:        'id',
    artwork:      true,
    // Override the size of the artwork to be loaded for each tile
    artworkSize:  null,
    
    
    // Backbone view properties
    events: {
      'click .scrollable > ul > li': 'clickedTile'
    },
    
    
    
    
    // ScrollList overrides
    // --------------------
    
    // Additional calculation for the zoom state of the tiles
    calculateSizes: function() {
      var $dummy = ScrollListView.prototype.calculateSizes.apply(this, arguments);
      
      // Generate a dummy
      $dummy.addClass('zoom')
        .css({
          position:   'absolute',
          visibility: 'hidden'
        }).appendTo(this.$inner);
      
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
      
      return $dummy;
    },
    
    
    // Renders each item of the list
    // VIRTUAL
    renderItem: function(item, i) {
      var zoomed    = this.data.zoomed == item,
          style     = this.getStyle(item, {string:true, zoomed:zoomed, afterDetails:this.details && i > this.details.afterIdx}),
          klass     = zoomed ? "zoom" : null;
      
      var rendered = '<li' + (style ? ' style="'+style+'"' : '') + (klass ? ' class="'+klass+'"' : '') + ">";
      
      if (this.line1)
        rendered += "<span>" + this.getAttribute(item, this.line1) + "</span>";
      if (this.line2)
        rendered += "<span>" + this.getAttribute(item, this.line2) + "</span>";
        
      rendered += "</li>";
      
      return rendered;
    },
    
    
    // Called when the render is complete
    // Starts the iterative getting of artwork
    // VIRTUAL
    afterRender: function(items) {
      
      // Do nothing if we have no items in view or no artwork provisioning
      if (!items.count || this.artwork === false) return;
      
      // Start setting artwork on the first element in view
      this._setArtworkIter(
        items.firstInView,
        items.firstInView - (items.firstInView - items.first),
        items.firstInView + (items.full ? items.count : items.countInView) - 1,
        items.timestamp
      );
    },
    
    
    // Called after a resize.
    // Clears the details view and resets any zooms
    // VIRTUAL
    onResize: function() {
      var $children;
      
      if (this.details) {
        // Remove the details
        this.details.$el.remove();
        // Set noanim for faster switching
        var $inner = this.$inner.addClass('noanim');
        // Reset position
        $children = this.$inner.children().css('-webkit-transform', "");
        // Reset bottom padding
        this.$inner.css('padding-bottom', "");
        // Disable noaim
        _.defer(function(){ $inner.removeClass('noanim'); });
        // Remove storage
        delete this.details;
      }
      
      if (this.data.zoomed) {
        // Get children if we have not yet
        if (!$children) $children = this.$inner.children();
        // Remove zoom from the class
        $children.eq( this.getElementIndex(this.data.zoomed) ).removeClass('zoom');
        // Remove storage
        delete this.data.zoomed;
      }
      
      // Call original onResize
      ScrollListView.prototype.onResize.apply(this, arguments);
    },
    
    
    
    
    // Attribute value getting
    // -----------------------
    
    // Gets the value for the given attribute for the given model
    // VIRTUAL
    getAttribute: function(model, attr) {
      var val = attr.call ? attr.call(model, model) : model.get(attr);
      return val || "";
    },
    
    
    
    
    // Rendering helpers
    // -----------------
    
    // Gets the style used for displaying the tile
    // VIRTUAL
    getStyle: function(item, options) {
      options || (options = {});
      var artwork = Artwork.fromCache(item),
          style   = {};
      
      // If we have any artwork
      if (artwork && artwork.exists()) {
        
        // Get data
        var tileSize    = options.zoomed ? this.calculated.size.zoom : this.calculated.size,
            artworkSize = options.size || this.artworkSize || this.calculated.size.zoom.inner.width;

        // The image
        style['background-image'] = "url(" + (options.url || artwork.url({size:artworkSize})) + ")";
        
        // Square artwork: nothing special
        if (artwork.isSquare()) { }
        
        // Portrait artwork
        else if (artwork.isPortrait()) {
          var imgWidth          = Math.ceil(tileSize.inner.height * artwork.get('ratio'));
          style['width']        = imgWidth + 'px';
          style['margin-right'] = tileSize.margin.right + (tileSize.inner.width - imgWidth) + 'px';
        }
        
        // Landscape artwork
        else {
          var imgHeight       = Math.ceil(tileSize.inner.width / artwork.get('ratio'));
          style['height']     = style['padding-top'] = imgHeight + 'px';
          style['margin-top'] = tileSize.margin.top + (tileSize.inner.height - imgHeight) + 'px';
        }
      }
      
      // If there is a details view and this item is below the details, move it down
      if (options.afterDetails) {
        style['-webkit-transform'] = "translate3d(0," + this.details.height + "px,0)";
      }
      
      // Convert to string representation if requested
      return options.string ? _.asCssString(style) : style;
    },
    
    
    // Sets the artwork for the tiles iteratively
    _setArtworkIter: function(itemIdx, elOffset, lastItemIdx, timestamp) {
      
      // Remember that we are busy with the current index
      this.data.lastRender.artworkIdx = itemIdx;
      
      // Function for doing the next item
      var view = this, doNext = function() {
        // Bail out if we have a different timestamp (new render) or if all tiles in view are done
        if (view.data.lastRender.timestamp != timestamp || itemIdx + 1 >= lastItemIdx) return;
        view._setArtworkIter(
          itemIdx + 1,
          elOffset,
          lastItemIdx,
          timestamp
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
        var item  = this.model.models[itemIdx];
        
        // Get the artwork
        Artwork.fetch(item, {
          size:     this.artworkSize || this.calculated.size.zoom.inner.width,
          colors:   true,
          success:  function(artwork, url, img) {
            // If we have artwork, set it
            if (artwork.exists() === true) {
              var artworkStyle  = view.getStyle(item, {url:url, zoomed:view.data.zoomed == item}),
                  $el           = $(el).addClass('noanim');
              _.extend(el.style, artworkStyle);
              _.defer(function(){
                $el.removeClass('noanim');
              });
            }
            // Do the next item
            doNext();
          },
          none:   doNext,
          error:  doNext
        });
        
      }
    },
    
    
    
    
    // Details view
    // ------------
    
    // (un)Zoomes the given tile
    zoomTile: function($el, zoom) {
      var el          = $el[0],
          item        = this.getItem($el),
          artwork     = Artwork.fromCache(item),
          hasArtwork  = artwork && artwork.exists() === true;
      
      // Unzoom the element
      if (zoom === false) {
        if (hasArtwork)
          _.extend(el.style, this.getStyle(item));
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
        if (hasArtwork)
          _.extend(el.style, this.getStyle(item, {zoomed:true}));
        
        // Add the class
        $el.addClass('zoom');
        
        // Remember the item that was zoomed
        this.data.zoomed = item;
        
      }
      
      // Toggle zoom
      else {
        this.zoomTile($el, item != this.data.zoomed);
      }
      
      return $el.hasClass('zoom');
    },
    
    
    // (un)Shows the detail view for the given tile
    showDetails: function($el) {
      var existing  = this.details,
          remove    = $el === false,
          props     = {},
          view      = this,
          $moved;
      
      // Do nothing if we have no details view function or there is nothing to remove
      if (!this.getDetailsView || (remove && !existing)) return;
      
      // The item that is detailed
      props.item      = remove ? existing.item : this.getItem($el);
      // The index of the item
      props.itemIdx   = this.getModelIndex(props.item);
      // The index of the item after which the other items should make room for the details
      props.afterIdx  = props.itemIdx + (this.calculated.capacity.hori - props.itemIdx % this.calculated.capacity.hori) - 1;
      // The DOM elements which should make room
      var afterElIdx  = props.afterIdx + 1 < this.data.lastRender.first ? 0 : Math.min(props.afterIdx - this.data.lastRender.first, this.getCount()-1);
      $moved          = this.$inner.children().eq(afterElIdx).nextAll();
      
      // Remove the detail view if asked
      if (remove) {
        _.defer(function(){
          var $details = existing.$el
          $details
            .css('-webkit-transform', "translate3d(0," + (existing.top + (existing.newHeight || 0)) + "px,0)")
            .children('.cover')
            .on('webkitTransitionEnd', function(){ $details.remove(); })
            .removeClass('open')
            .css('-webkit-transform', "")
          $moved.css('-webkit-transform', "");
          view.$inner.css('padding-bottom', "");
        });
        delete this.details;
        return;
      }
      
      // Build the details element
      var $details    = $(tilesDetailsTemplate({item:props.item})).addClass('dummy').insertBefore(this.$inner),
          $content    = $details.children('.content'),
          contentView = this.getDetailsView(props.item, $content),
          onSameRow   = existing && existing.afterIdx == props.afterIdx,
          elTop       = $el[0].offsetTop;
      
      // Save more props
      _.extend(props, {
        // The details element
        $el:              $details,
        // The content element
        $content:         $content,
        // Whether the details is opened on the same row as the current details (if exists)
        onSameRow:        onSameRow,
        // Whether the details is opened above the current details (if exists)
        newAboveCurrent:  existing && !onSameRow ? props.afterIdx < existing.afterIdx : (void 0),
        // Calculate arrow position
        arrowLeft:        $el.position().left + $el.outerWidth(true) / 2,
        // Calculate the height of the details
        height:           $details.outerHeight(),
        // Calculate the top of the new details
        top:              onSameRow ? existing.top : elTop + $el.outerHeight(true) - $el.css('margin-top').numberValue()
      });
      
      // If the new details is on the same row, we should replace the content
      if (props.onSameRow) {
        
        // Fix the height to the max of the new and existing height
        existing.$el.css('height', Math.max(props.height, existing.height));
        
        // Get the old and new content elements
        var $oldContent = existing.$el.children('.content').addClass('outgoing');
        $content.addClass('incoming').insertAfter($oldContent);
        
        // Remove the old content after the transition
        $oldContent.on('webkitTransitionEnd', function(){
          $oldContent.remove();
          existing.$el.css('height', "");
        });
        
        // Remove the incoming clas son the new content
        $content.removeClass('incoming');
        
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
      
      // Check if details fit in view
      if (props.top + props.height > this.scroller.scrollTop + this.scroller.clientHeight - 10)
        props.scrollTop = Math.min(elTop - this.calculated.size.zoom.margin.top, props.top + props.height - this.scroller.clientHeight + 10);
      else if (elTop - this.calculated.size.zoom.margin.top < this.scroller.scrollTop)
        props.scrollTop = elTop - this.calculated.size.zoom.margin.top;
      
      // First part animation
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
      
      // Set scroll top
      if (_.isNumber(props.scrollTop)) {
        this.disableRendering();
        this.$scroller.animate(
          { scrollTop:props.scrollTop },
          {
            duration: 400,
            complete: function(){ view.enableRendering(); }
          }
        );
      }
      
      // The rest
      _.defer(_.bind(function(){
        
        // Set padding to make room for the details
        this.$inner.css('padding-bottom', props.height + "px");
        
        // Move the tiles after the details down
        $moved.css('-webkit-transform', "translate3d(0," + props.height + "px,0)");
        
        // Slide the details open
        $details
          .removeClass('dummy')
          .css('-webkit-transform', "translate3d(0," + props.top + "px,0)")
          .children('.cover')
          .addClass('open')
          .css('-webkit-transform', "translate3d(0," + (props.height-1) + "px,0)");
        
      }, this));
      
      // 
      return this.details = props;
    }
    
    
    
  });
  
  return TilesView;
  
});