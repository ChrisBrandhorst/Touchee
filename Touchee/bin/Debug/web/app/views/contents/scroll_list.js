define([
  'jquery',
  'underscore',
  'Backbone',
  'Touchee',
  'views/contents/base'
], function($, _, Backbone, Touchee, BaseView) {
  
  
  var selectionDefaults = {
    // The delay before the selection kicks in
    delay:      0,
    // The pixel distance to scroll before the selection is cancelled
    distance:   5,
    // The class to apply to selected items
    klass:      'selected',
    // Whether to keep the selection after the touch / click has ended
    keep:       false
  };
  
  
  
  
  var ScrollList = BaseView.extend({
    
    
    
    // Backbone View options
    // ---------------------
    tagName:      'section',
    className:    'scroll_list',
    
    
    
    
    // ScrollList View options
    // -----------------------
    
    // Type of scrolllist
    listType:       '',
    // Type of content
    contentType:    '',
    // Element used as floating index
    floatingIndex:  '<div/>',
    // jQuery object for the inner element. If this is not given, a new element is made
    // using the innerTagName value
    $inner:         null,
    // The tag for the inner element
    innerTagName:   'div',
    // Whether to show a scroller
    scroller:       false,
    // Whether to show indices in the list
    indicesShow:    false,
    // The attribute to use as index value. If set to falsy, the scroller will not be indexed
    indexAttribute: null,
    // From which number of items the list is rendered in one piece
    min:            0,
    // The number of extra rows to render outside the visible portion
    extraRows:      80,
    // Sets the quickscroll: false for none, true for default and 'alpha' for alpha version
    quickscroll:    true,
    // The selector for the selectable items. False if nothing can be selected
    selectable:     false,
    // Override for item selection options
    selection:      {},
    
    
    
    
    // Privates
    // -----------------------
    
    // Rendering is enabled
    _renderingEnabled: true,
    
    
    
    
    // Initialization
    // --------------
    
    // Constructor
    initialize: function() {
      this.$el.addClass(this.listType);
      this.$el.addClass(this.contentType);
      
      this.calculated = {};
      this.data = {};
      
      if (this.selectable)
        this.selection = _.extend({}, selectionDefaults, this.selection);
    },
    
    
    // Render
    render: function() {
      if (this._rendered) return;
      this._rendered = true;
      
      // Check if we are visible
      if (!this.$el.is(':visible'))
        return Touchee.Log.error("Cannot render ScrollList if it is not visible yet!");
      
      // Create contents container
      this.$scroller = $('<div class="scrollable"/>').prependTo(this.$el);
      this.scroller = this.$scroller[0];
      
      // Create inner element
      if (!this.$inner)
        this.$inner = $('<' + (this.innerTagName || 'div') + '/>').appendTo(this.$scroller);
      
      // Build the floating index if required
      if (this.indicesShow && this.indexAttribute)
        this._renderFloatingIndex();
        
      // Set quickscroll
      if (this.quickscroll && this.indexAttribute)
        this._renderQuickscroll();
      
      // Calculate size of elements and capacity
      this.calculateSizes();
      this._calculateCapacity();
      
      // Bind events
      this._bind();
      
      // Do first render
      this.contentChanged();
      
      // Show index if requested
      if (this.indicesShow && this.indexAttribute)
        this._positionFloatingIndex();
    },
    
    
    // Bind all event handlers for the list
    // PRIVATE
    _bind: function() {
      var touching    = false,
          scrolling   = false,
          scrollList  = this,
          scrollTimeout;
      
      // If we have touches, do some advanced events for efficiency
      if ('ontouchstart' in document.documentElement) {
        // Start touch
        this.$scroller.bind(Touchee.START_EVENT+'.scroll_list', function(ev){
          touching = true;
          scrolling = false;
        });
        // Touch end: if we are scrolling, do a render after a small delay
        this.$scroller.bind(Touchee.END_EVENT+'.scroll_list', function(){
          touching = false;
          if (scrolling)
            scrollTimeout = setTimeout(function(){
              scrollList.renderInView();
              scrollTimeout = null;
            }, 100);
          scrolling = false;
        });
        // If we are scrolling, position the floating index.
        // We only render if we are scrolling without touching
        this.$scroller.bind('scroll.scroll_list', function(){
          scrolling = touching;
          if (scrollList.indicesShow)
            scrollList._positionFloatingIndex();
          if (!touching && !scrollTimeout)
            scrollList.renderInView();
        });
      }
      
      // Else, just use regular debounce on scroll
      else {
        this.$scroller.bind('scroll.scroll_list', _.debounce(_.bind(this.renderInView, this), 100));
        if (scrollList.indicesShow)
          this.$scroller.bind('scroll.scroll_list', _.bind(this._positionFloatingIndex, this));
      }
      
      // Recalculate capacity when the view is resized
      $(window).bind('resize.scroll_list-' + this.id, _.debounce(
        _.bind(this.onResize, this), 100
      ));
      
      // Bind stuff for quickscroll
      if (this.quickscroll) {
        this._qs.$el.bind(Touchee.START_EVENT, _.bind(this._qsStart, this));
        this._qs.$el.bind(Touchee.MOVE_EVENT, _.bind(this._qsScroll, this));
        this._qs.$el.bind(Touchee.END_EVENT, _.bind(this._qsEnd, this));
      }
      
      // Touch scroll selection
      if (this.selectable)
        this.$scroller.on(Touchee.START_EVENT+'.scroll_list', this.selectable, _.bind(this._selectionStart, this));
      
    },
    
    
    // Unbind all event handlers for the list
    // PRIVATE
    _unbind: function() {
      $(window).unbind('resize.scroll_list-' + this.id);
      this.$scroller.unbind('.scroll_list');
      if (this.quickscroll)
        this._qs.$el.unbind();
    },
    
    
    // Called when the view is disposed
    // VIRTUAL
    onDispose: function() {
      this._unbind();
    },
    
    
    
    
    // (En/Dis)able rendering
    // ----------------------
    
    // Enable rendering
    enableRendering:  function() {
      this._renderingEnabled = true;
    },
    
    
    // Disable rendering
    disableRendering: function() {
      this._renderingEnabled = false;
    },
    
    
    
    
    // Model querying
    // --------------
    
    // Gets the model count
    // ABSTRACT
    getCount: function() {
      throw('NotImplementedException');
    },
    
    
    // Gets the set of models for the given range
    // ABSTRACT
    getModels: function(first, count) {
      throw('NotImplementedException');
    },
    
    
    // Gets the model for the given model index
    getModel: function(idx) {
      var models = this.getModels(idx, 1);
      return models && models.length ? models[0] : null;
    },
    
    
    // Gets the index of the given item
    // ABSTRACT
    getIndex: function(item) {
      throw('NotImplementedException');
    },
    
    
    // Gets the index of the element representing the given item
    getElementIndex: function(item) {
      return this.getIndex(item) + this.data.lastRender.first;
    },
    
    
    // Gets the item for the given rendered element
    getItem: function(el) {
      return !el || !el.length
        ? null
        : this.getModel( this.data.lastRender.first + $(el).prevAll(':not(.index)').length );
    },
    
    
    // Should be called when the content of the model which is viewed is changed
    contentChanged: function() {
      if (this.indexAttribute)
        this.indices = this.getIndices();
      this.renderInView(true);
    },
    
    
    
    
    // Calculation of content
    // ----------------------
    
    // Calculates the size of shown elements]
    // VIRTUAL
    calculateSizes: function() {
      var $children = this.$inner.children(':not(.index)'),
          $dummy, $item;
      
      // Get the rows if we have a tbody
      if ($children.is('tbody')) $children = $children.children(':not(.index)');
      
      // Get a reference to a child item. If there are no children, create a dummy
      if (!$children.length)
        $item = $dummy = $( _.result(this, 'dummy') ).appendTo(this.$inner);
      else
        $item = $children.first();
      
      // Get both the height and width including padding, border and margin
      var size = {
        height: $item.outerHeight(true),
        width:  $item.outerWidth(true)
      };
      
      // Set margins and 'inner' size
      var margins = _.map($item.css('margin').replace(/px/g, "").split(" "), function(m){return +m;});
      
      size.margin = {
        top:    margins[0],
        right:  margins[1],
        bottom: margins[2],
        left:   margins[3]
      };
      size.inner = {
        height: size.height - size.margin.top - size.margin.bottom,
        width:  size.width - size.margin.left - size.margin.right
      };
      
      // Remove dummy
      if ($dummy)
        $dummy.remove();
      
      // Calculate size of indices
      size.indexHeight = this.indicesShow ? this.$index.outerHeight() : 0;
      
      // Set data in model
      this.calculated.size = size;
    },
    
    
    // Calculates the capacity of the view
    // PRIVATE
    _calculateCapacity: function() {
      var scroller  = this.$scroller[0],
          capacity  = {};
      
      // Calculates the capacity of items within the viewport
      capacity = {
        vert: Math.ceil(scroller.clientHeight / this.calculated.size.height),
        hori: Math.floor(scroller.clientWidth / this.calculated.size.width)
      };
      
      // Up the vertical capacity if necessary
      if (scroller.clientHeight % this.calculated.size.height > 1) capacity.vert++;
      
      capacity.total = capacity.vert * capacity.hori;
      
      // Set data in model
      this.calculated.capacity = capacity;
    },
    
    
    // Default indices getter
    // VIRTUAL
    getIndices: function() {
      var models  = this.getModels(0, this.getCount()),
          data    = {indices:[],count:[],items:[],cumulCountMap:{},posMap:{}},
          attr    = this.indexAttribute,
          prevIdx;
      
      _.each(models, function(model, i){
        var idx = model.get(attr)[0].toUpperCase();
        
        if (idx > "Z") idx = Touchee.nonAlphaSortValue;
        
        if (idx != prevIdx) {
          data.indices[data.indices.length] = idx;
          data.count[data.indices.length - 1] = 1;
          data.posMap[idx] = data.indices.length - 1;
          prevIdx = idx;
        }
        else {
          data.count[data.indices.length - 1]++;
        }
        
        data.items[i] = idx;
        data.cumulCountMap[idx] = i + 1;
        
      });
      
      return data;
    },
    
    
    
    
    // Rendering
    // ---------
    
    // Renders the visible items
    renderInView: function(force) {
      if (!this._renderingEnabled) return;
      
      var scroller  = this.scroller,
          size      = this.calculated.size,
          capacity  = this.calculated.capacity,
          data      = this.data,
          scrollTop = scroller.scrollTop;
      
      // Set scrolling method :-(
      // if (scrollTop > Math.pow(2,17) - 1000)
      //   scroller.style.webkitOverflowScrolling = "auto";
      // else
      //   scroller.style.webkitOverflowScrolling = "touch";
      
      // If we are already fully rendered, but we're not forced, bail out
      if (data.fullRender && !force) return;
      else data.fullRender = false;
      
      // Get the total number of items
      // Check if we need to do a full render
      // Check if we scrolled up or down
      var total         = this.getCount(),
          fullRender    = total < this.min,
          visibleHeight = scroller.clientHeight,
          floatingIndex = null,
          items         = {
            first:    0,
            count:    total,
            indices:  {above:0,below:0}
          },
          lastRender    = {
            first:        0,
            firstInView:  0,
            count:        total,
            countInView:  total
          };
      
      // Calculate which item we should show if we are not fully rendering the list
      if (!fullRender) {
        
        // Calculate the extra rows to be added to the top and bottom
        var scrolledUp      = scrollTop < data.lastScrollTop,
            extraCount      = this.extraRows * capacity.hori,
            extraRowsAbove  = Math.round(this.extraRows * (scrolledUp ? .75 : .25)),
            extraAbove      = extraRowsAbove * capacity.hori,
            first;
        
        // If we show indices, do fancy calculation
        if (this.indicesShow) {
          
          // Calculate the first item in view
          var blockInfo = this._getBlockInfo(),
              inBlock   = Math.floor(Math.max(0, scrollTop - blockInfo.height - size.indexHeight) / size.height),
              idx       = this.indices.indices[blockInfo.idxIdx - 1];
          first = (idx ? this.indices.cumulCountMap[idx] : 0) + inBlock;
          
          // Correct for extra rows
          items.first = Math.max(0, first - extraAbove);
          items.count = Math.min(total - items.first, capacity.vert * capacity.hori + extraCount);
          
          // Set indices above / below
          var firstIdx = this.indices.items[items.first];
          items.indices.above = this.indices.posMap[firstIdx];
          var lastIdx = this.indices.items[items.first + items.count - 1];
          items.indices.below = this.indices.indices.length - this.indices.posMap[lastIdx] - 1;
          
        }
        
        // Else, simply use the viewport to calculate which items to show
        else {
          // Calculate the first and last in view based on the viewport
          first = Math.floor(scrollTop / size.height) * capacity.hori;
          items.count = capacity.vert * capacity.hori;
          
          // Make sure all is within bounds, including the extra rows
          items.first = Math.max(0, first - extraAbove);
          items.count = Math.min(total - items.first, items.count + extraCount);
        }
        
      }
      
      // Set after render props
      lastRender.first       = items.first;
      lastRender.firstInView = first;
      lastRender.count       = items.count;
      lastRender.countInView = Math.min(total - first, capacity.vert * capacity.hori);
      
      // Get the models. The items object may be changed!
      var models = this.getModels(items.first, items.count);
      
      // Set the HTML
      this.$inner[0].innerHTML = this.renderItems(models, items)
      
      // Calculate margins
      var marginTop     = (items.first / capacity.hori) * size.height,
          marginBottom  = Math.ceil((total - items.count - items.first) / capacity.hori) * size.height;
      // Correct for indices
      if (this.indicesShow) {
        marginTop     += items.indices.above * size.indexHeight;
        marginBottom  += items.indices.below * size.indexHeight;
      }
      // Set margins
      this.$inner[0].style.marginTop    = marginTop + 'px';
      this.$inner[0].style.marginBottom = marginBottom + 'px';
      
      // Position index on force render
      if (force && this.indicesShow)
        this._positionFloatingIndex();
        
      // Store stuff for next time
      data.lastScrollTop = scrollTop;
      data.fullRender = fullRender;
      data.lastRender = lastRender;
      data.lastRender.timestamp = new Date().getTime();
      
      // After render
      this.afterRender(lastRender);
    },
    
    
    // Renders a set of item
    // VIRTUAL
    renderItems: function(models, options) {
      var html    = "",
          odd     = options.first % 2 == 0,
          prevIdx;
      
      // Go through all models
      _.each(models, _.bind(function(model, i){
        
        // Draw index if requested
        if (this.indicesShow) {
          var idx = this.indices.items[options.first + i];
          if (idx != prevIdx) {
            html += this.renderIndex( this._processIndex(idx) );
            prevIdx = idx;
          }
        }
        
        // Render the item
        html += this.renderItem(model, options.first + i);
        odd = !odd;
      }, this));
      return html;
    },
    
    
    // Renders a single item
    // ABSTRACT
    renderItem: function(item, i) {
      throw('NotImplementedException');
    },
    
    
    // Renders an index item
    // ABSTRACT
    renderIndex: function(index) {
      throw('NotImplementedException');
    },
    
    
    // Renders the floating index
    _renderFloatingIndex: function(index) {
      this.$index = $(this.floatingIndex)
        .addClass('scroll_list-' + this.listType + '-index index')
        .prependTo(this.$el).hide();
    },
    
    
    // Called when a render has completed
    // VIRTUAL
    afterRender: function(items) {
    },
    
    
    // Called after a resize
    // VIRTUAL
    onResize: function() {
      this.calculateSizes();
      this._calculateCapacity();
      this.renderInView();
    },
    
    
    
    
    // Rendering helpers
    // -----------------
    
    // Get information about the index who's block is intersected by the top line of the view
    // PRIVATE
    _getBlockInfo: function() {
      var size        = this.calculated.size,
          height      = 0,
          blockHeight = 0,
          idxIdx      = 0;
      
      do {
        height += blockHeight;
        blockHeight = size.indexHeight + Math.ceil(this.indices.count[idxIdx]) * size.height;
      } while ((height + blockHeight < this.scroller.scrollTop) && ++idxIdx);
      
      return {
        height:       height,
        blockHeight:  blockHeight,
        idxIdx:       idxIdx
      };
      
    },
    
    
    // Modify the index value to group non-alpha chars
    // PRIVATE
    _processIndex: function(index) {
      return index == Touchee.nonAlphaSortValue ? "#" : index;
    },
    
    
    // Positions the floating index
    // PRIVATE
    _positionFloatingIndex: function() {
      
      if (!this.indices.indices.length)
        return;
      else if (this.el.scrollTop < 0)
        this.$index[0].style.display = 'none';
      else
        this.$index[0].style.display = '';
      
      var blockInfo = this._getBlockInfo();
      
      // Store the top of the index which is below the top line of the view
      var nextIndexTop = blockInfo.height + blockInfo.blockHeight - this.scroller.scrollTop,
          nextIndexIdx = this.indices.indices[blockInfo.idxIdx];
      
      // Set floating index
      var diff = Math.min(0, nextIndexTop - this.calculated.size.indexHeight);
      this.$index[0].innerHTML = this._processIndex(nextIndexIdx);
      this.$index[0].style.webkitTransform = "translate3d(0," + diff + "px,0)";
      
    },
    
    
    // Renders the quickscroll element
    // PRIVATE
    _renderQuickscroll: function() {
      
      // Build element
      var $qs = $('<ol/>')
        .addClass('quickscroll')
        .prependTo(this.$el);
      
      // Set data
      this._qs = {
        alpha:  this.quickscroll == 'alpha',
        $el:    $qs
      };
      
      // Fill with letters if alpha
      if (this._qs.alpha) {
        var qsHTML = "";
        for (var i = 65; i <= 90; i++)
          qsHTML += "<li>" + String.fromCharCode(i) + "</li>";
        qsHTML += "<li>#</li>";
        this._qs.$el.addClass('alpha').html(qsHTML);
      }
      
      // Set more data
      var rect    = this._qs.$el[0].getBoundingClientRect(),
          padding = this._qs.$el.css('border-top-width').numberValue();
      _.extend(this._qs, {
        top:      rect.top,
        height:   rect.height,
        padding:  padding,
        area:     rect.height - 2 * padding
      });
      
    },
    
    
    
    
    // Quickscroll
    // -----------
    
    // Called when a quickscroll is started
    // PRIVATE
    _qsStart: function(ev) {
      delete this._qs.last;
      this._qsScroll(ev);
    },
    
    
    // Called when the user is scrolling the quickscroll
    // PRIVATE
    _qsScroll: function(ev) {
      
      // Get the fraction of the height the user is touching
      var pageY = ev.originalEvent.touches ? ev.originalEvent.touches[0].pageY : ev.pageY,
          pos   = Math.min(Math.max(pageY - this._qs.top - this._qs.padding, 0), this._qs.area),
          par   = pos / this._qs.area
      
      // If we have an alpha scroller, get the corresponding index
      if (this._qs.alpha) {
        var $children = this._qs.$el.children(),
            i         = Math.min(Math.floor(par * $children.length), $children.length - 1),
            idx;
        
        for (i; i < $children.length; i++) {
          idx = $children.eq(i).text().toUpperCase();
          if (_.isNumber(this.indices.posMap[idx])) break;
        }
        par = idx == "#" ? "|" : idx;
      }
      
      // If this position is different then the last, kick the scrollTo method
      if (this._qs.last != par)
        this.scrollTo(par);
      this._qs.last = par;
      
      // Set hover state and disable default touch
      this._qs.$el.addClass('hover');
      ev.preventDefault();
    },
    
    
    // Called when the quickscroll is over;
    // PRIVATE
    _qsEnd: function(ev) {
      this._qs.$el.removeClass('hover').hide().show();
    },
    
    
    // Scroll to the specified position in pixels or fraction of the height
    scrollTo: function(param) {
      var scrollTop;
      
      // A fraction was given
      if (_.isNumber(param)) {
        scrollTop = param * (this.scroller.scrollHeight - this.scroller.clientHeight);
      }
      
      // An index was given
      else if (_.isString(param)) {
        var idx       = param,
            idxIdx    = this.indices.posMap[idx],
            rows      = this.indices.cumulCountMap[idx] - this.indices.count[idxIdx];
        scrollTop = rows * this.calculated.size.height + idxIdx * this.calculated.size.indexHeight;
      }
      
      this.scroller.scrollTop = scrollTop;
    },
    
    
    
    
    // Touch selection
    // ---------------
    
    // An item has been selected
    // ABSTRACT
    selected: function(item, $item) {
      throw("NotImplentedException");
    },
    
    
    // Called when a touch selection has started
    // PRIVATE
    _selectionStart: function(ev) {
      
      // Get the item
      var $item = $(ev.currentTarget);
      
      // Set the onclick handler for anchors
      if ($item[0].tagName.toLowerCase() == 'a')
        $item[0].onclick = "return false;";
      
      // Get currently selected items
      var $previous = $item.siblings('.' + this.selection.klass);
      
      // Selection setter
      var doSelect = function() {
        $item.addClass(this.selection.klass);
        $previous.removeClass(this.selection.klass);
      };
      
      // Set selection data
      this.data.selection = {
        $item:      $item,
        $previous:  $previous,
        timeout:    _.delay(_.bind(doSelect, this), this.selection.delay),
        y:          ev.getCoords().y
      };
      
      // Set bindings
      this.$scroller.on(Touchee.MOVE_EVENT+'.tss', _.bind(this._selectionMove, this));
      this.$scroller.on(Touchee.END_EVENT+'.tss',  _.bind(this._selectionEnd, this));
    },
    
    
    // Called when a touch selection is in progress and the touch position has moved
    // PRIVATE
    _selectionMove: function(ev) {
      
      // Get the data
      var data = this.data.selection;
      if (!data) return;
      
      // If we are moving enough
      var diff = Math.abs(data.y - ev.getCoords().y);
      if (diff > this.selection.distance) {
        
        // If we are moving within the timeout, kill the timeout so the new selection is not set
        if (data.timeout)
          clearTimeout(data.timeout);
        
        // Set that we have moved
        data.moved = true;
        
        // Remove selection on the target item
        data.$item.removeClass(this.selection.klass);
        
        // We do not need move callbacks anymore
        this.$scroller.off(Touchee.MOVE_EVENT + '.tss');
      }
    },
    
    
    // Called when a touch selection is ended
    // PRIVATE
    _selectionEnd: function(ev) {
      
      // Get the data
      var data = this.data.selection;
      if (!data) return;
      
      // If we stopped within the timeout, kill the timeout so the new selection is not set
      if (data.timeout)
        clearTimeout(data.timeout);
      
      // If we have moved, reset the original selection
      if (data.moved)
        data.$previous.addClass(this.selection.klass);
      
      // Else, select the new item
      else {
        
        // Set selection class (for when we have ended the touch during the timeout)
        if (this.selection.keep) {
          data.$item.addClass(this.selection.klass);
          data.$previous.removeClass(this.selection.klass);
        }
        else
          data.$item.removeClass(this.selection.klass);
        
        // If we have a callback function, call that
        if (_.isFunction(this.selected))
          this.selected.call(this, this.getItem(data.$item), data.$item);
        
        // Else, navigate to the anchor
        else if (data.$item.is('a'))
          Backbone.history.navigate(data.$item.attr('href'), {trigger:true});
      }
      
      // Unbind all
      this.$scroller.off(Touchee.MOVE_EVENT + '.tss');
      this.$scroller.off(Touchee.END_EVENT + '.tss');
      delete this.data.selection;
      
      return false;
    }
    
    
  });
  
  return ScrollList;
  
  
});