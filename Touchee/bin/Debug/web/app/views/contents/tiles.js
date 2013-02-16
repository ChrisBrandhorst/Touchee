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
    extraRows:    2,
    
    // Tiles view properties
    line1:        'id',
    line2:        'id',
    
    // Override the size of the artwork to be loaded for each tile
    artworkSize:  null,
    
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
    renderItem: function(item, i, options) {
      var zoomed    = this.data.zoomed == item,
          style     = this.getStyle(item, {string:true, zoomed:zoomed, afterDetails:this.details && i > this.details.afterIdx}),
          klass     = zoomed ? "zoom" : null;
      
      var rendered = '<li' + (style ? ' style="'+style+'"' : '') + (klass ? ' class="'+klass+'"' : '') + ">";
      
      if (this.line1)
        rendered += "<span>" + this.getAttributeValue(item, this.line1) + "</span>";
      if (this.line2)
        rendered += "<span>" + this.getAttributeValue(item, this.line2) + "</span>";
        
      rendered += "</li>";
      
      return rendered;
    },
    
    
    // Gets the style used for displaying the tile
    getStyle: function(item, options) {
      options || (options = {});
      var artwork = Artwork.fromCache(item),
          style   = {};
      
      // If we have any artwork
      if (artwork && artwork.exists() === true) {
        
        // Get data
        var tileSize    = options.zoomed ? this.calculated.size.zoom : this.calculated.size,
            artworkSize = this.artworkSize || this.calculated.size.zoom.inner.width,
            style       = {};
        
        // The image
        style['background-image'] = "url(" + artwork.url({size:artworkSize}) + ")";
        
        // Square artwork: nothing special
        if (artwork.isSquare()) { }
        
        // Portrait artwork
        else if (artwork.isPortrait()) {
          var imgWidth          = Math.ceil(tileSize.inner.height / artwork.get('ratio'));
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
            view  = this;
        
        // Get the artwork
        Artwork.fetch(item, {
          size:     this.artworkSize || this.calculated.size.zoom.inner.width,
          colors:   true,
          success:  function(artwork) {
            // If we have artwork, set it
            if (artwork.exists() === true) {
              var artworkStyle  = view.getStyle(item, {zoomed: view.data.zoomed == item}),
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
        this.zoomTile($el, !$el.hasClass('zoom'));
      }
      
      return $el.hasClass('zoom');
    },
    
    
    // (un)Shows the detail view for the given tile
    showDetails: function($el) {
      var existing  = this.details,
          remove    = $el === false,
          props     = {};
      
      // The item that is detailed
      props.item      = remove ? existing.item : this.getItem($el);
      // The index of the item
      props.itemIdx   = this.getIndex(props.item);
      // The index of the item after which the other items should make room for the details
      props.afterIdx  = props.itemIdx + (this.calculated.capacity.hori - props.itemIdx % this.calculated.capacity.hori) - 1;
      // The DOM elements which should make room
      $moved          = this.$inner.children().eq(Math.min(props.afterIdx - this._lastRender.first, this.getCount()-1)).nextAll();
      
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
        });
        delete this.details;
        return;
      }
      
      // Build the details element
      var $details    = $(tilesDetailsTemplate()).addClass('dummy').insertBefore(this.$inner),
          $content    = $details.children('.content'),
          contentView = this.getDetailsView(props.item, $content),
          onSameRow   = existing && existing.afterIdx == props.afterIdx;
      
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
        top:              onSameRow ? existing.top : $el[0].offsetTop + $el.outerHeight(true) - $el.css('margin-top').numberValue()
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
        $moved.css('-webkit-transform', "translate3d(0," + props.height + "px,0)");
        
        // Slide the details open
        $details
          .removeClass('dummy')
          .css('-webkit-transform', "translate3d(0," + props.top + "px,0)")
          .children('.cover')
          .addClass('open')
          .css('-webkit-transform', "translate3d(0," + (props.height-1) + "px,0)");
      });
      
      // 
      return this.details = props;
    }
    
    
    
  });
  
  return TilesView;
  
});