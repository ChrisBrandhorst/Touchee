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
    
    
    
    
    // ScrollList View options
    // -----------------------
    
    // Type of scrolllist
    listType:       '',
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
    showIndex:      false,
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
    
    // The render delay to use (after how many ms of no scrolling a re-render is done)
    renderDelay:    100,
    
    
    
    // Privates
    // -----------------------
    
    // Rendering is enabled
    _renderingEnabled: true,
    
    
    
    
    // Initialization
    // --------------
    
    // Constructor
    initialize: function() {
      this.$el.addClass(this.listType);
      this.$el.addClass('scroll_list');
      
      this.calculated = {};
      this.data = {};
      
      if (this.selectable)
        this.selection = _.extend({}, selectionDefaults, this.selection);

      this.listenTo(this.model, 'reset add remove change', this._contentChanged);
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
      if (this.index && this.showIndex)
        this.renderFloatingIndex();
      
      // Calculate size of elements and capacity
      this.calculateSizes();
      this._calculateCapacity();
      
      // Set quickscroll
      if (this.quickscroll)
        this._renderQuickscroll();

      // Bind events
      this._bind();
      
      // Do first render
      this._contentChanged();
    },
    

    // Bind all event handlers for the list
    // PRIVATE
    _bind: function() {
      var touching    = false,
          scrolling   = false,
          scrollList  = this,
          deBounceRender = _.debounce(_.bind(this._renderInView, this), this.renderDelay),
          scrollTimeout;

      // If we have touches, do some advanced events for efficiency
      if ('ontouchstart' in document.documentElement) {
        // Start touch
        this.$scroller.bind(Touchee.START_EVENT+'.scroll_list', function(ev){
          touching = true;
          scrolling = false;
        });
        // If we are scrolling, position the floating index.
        // We only render if we are scrolling without touching
        this.$scroller.bind('scroll.scroll_list', function(){
          scrolling = touching;
          scrollList._calculateInView();
          if (!touching && !scrollTimeout)
            deBounceRender();
          if (scrollList.showIndex)
            scrollList.positionFloatingIndex();
        });
        // Touch end: if we are scrolling, do a render after a small delay
        this.$scroller.bind(Touchee.END_EVENT+'.scroll_list touchcancel.scroll_list', function(){
          touching = false;
          if (scrolling)
            scrollTimeout = setTimeout(function(){
              scrollList._calculateInView();
              scrollList._renderInView();
              if (scrollList.showIndex)
                scrollList.positionFloatingIndex();
              scrollTimeout = null;
            }, scrollList.renderDelay * 2);
          scrolling = false;
        });
      }
      
      // Else, just use regular debounce on scroll
      else {
        this.$scroller.bind('scroll.scroll_list', _.bind(this._calculateInView, this));
        this.$scroller.bind('scroll.scroll_list', deBounceRender);
        if (scrollList.showIndex)
          this.$scroller.bind('scroll.scroll_list', _.bind(this.positionFloatingIndex, this));
      }
      
      // Recalculate capacity when the view is resized
      $(window).bind('resize.scroll_list-' + this.id, _.debounce(
        _.bind(this.onResize, this), 100
      ));
      
      // Bind stuff for quickscroll
      if (this.quickscroll) {
        var qs = this.data.qs;
        qs.$el.bind(Touchee.START_EVENT, _.bind(this._qsStart, this));
        qs.$el.bind(Touchee.MOVE_EVENT, _.bind(this._qsScroll, this));
        qs.$el.bind(Touchee.END_EVENT+' mouseout touchcancel', _.bind(this._qsEnd, this));
      }
      
      // Touch scroll selection
      if (this.selectable)
        this.$scroller.on(Touchee.START_EVENT+'.scroll_list', this.selectable, _.bind(this._selectionStart, this));
      
    },
    
    
    // Unbind all event handlers for the list
    // PRIVATE
    _unbind: function() {
      $(window).unbind('resize.scroll_list-' + this.id);
      this.$scroller.unbind('.scroll_list').off('.scroll_list');
      if (this.quickscroll)
        this.data.qs.$el.unbind();
    },
    
    
    // Called when the view is disposed
    // VIRTUAL
    onRemove: function() {
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
    getModelIndex: function(item) {
      throw('NotImplementedException');
    },
    
    
    // Gets the index of the element representing the given item
    getElementIndex: function(item) {
      return this.getModelIndex(item) + this.data.lastRender.first;
    },
    
    
    // Gets the item for the given rendered element
    getItem: function(el) {
      return !el || !el.length
        ? null
        : this.getModel( this.data.lastRender.first + $(el).prevAll(':not(.index)').length );
    },
    
    
    // Should be called when the content of the model which is viewed is changed
    // Implement contentChanged for crazy things
    _contentChanged: function() {
      if (_.isFunction(this.contentChanged))
        this.contentChanged();
      if (this.index)
        this.calculated.indices = this.getIndices();
      this._calculateInView(true);
      this._renderInView();
    },


    // Gets the index for the given item
    // VIRTUAL
    getIndex: function(item) {
      return _.isString(this.index)
        ? item.get(this.index)[0].toUpperCase()
        : this.index.call(this, item);
    },
    
    
    
    
    // Calculation of content
    // ----------------------
    
    // Calculates the size of shown elements]
    // VIRTUAL
    calculateSizes: function() {
      var $children = this.$inner.children(':not(.index)');
      
      // Create a dummy
      // if (!$children.length)
      var $dummy = $( _.result(this, 'dummy') ).appendTo(this.$inner);
      
      // Get both the height and width including padding, border and margin
      var size = {
        height: $dummy.outerHeight(true),
        width:  $dummy.outerWidth(true)
      };
      
      // Set margins and 'inner' size
      var margins = _.map($dummy.css('margin').replace(/px/g, "").split(" "), function(m){return +m;});
      
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
      
      // Calculate size of indices
      if (this.$index)
        size.indexHeight = this.showIndex ? this.$index.outerHeight() : 0;
      
      // Set data in model
      this.calculated.size = size;

      // Return dummy for other use
      return $dummy.remove();
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
      var models      = this.getModels(0, this.getCount()),
          data        = {indices:[],count:[],items:[],cumulCountMap:{},posMap:{}},
          scrollList  = this,
          prevIdx;
      
      _.each(models, function(model, i){
        var idx = scrollList.getIndex(model);
        
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
    
    // Calculates what should be rendered
    // PRIVATE
    _calculateInView: function(force) {
      // Set scrolling method :-(
      // if (scrollTop > Math.pow(2,17) - 1000)
      //   scroller.style.webkitOverflowScrolling = "auto";
      // else
      //   scroller.style.webkitOverflowScrolling = "touch";
      // $('#output').text(new Date().getTime());

      if (!this._renderingEnabled) return this.data.lastCalc;

      // Get some vars we need
      var scroller      = this.scroller,
          size          = this.calculated.size,
          capacity      = this.calculated.capacity,
          data          = this.data,
          total         = this.getCount(),
          renderIndices = this.index && this.showIndex;
      
      // Start the calculated object
      var calc = {
        scrollTop:    scroller.scrollTop,
        total:        total,
        fullRender:   total < this.min,
        needsRender:  true,
        firstInView:  0,
        block:        renderIndices ? this._getBlockInfo() : null,
        first:        0,
        count:        total,
        indices:      { above:0, below:0 }
      };
      
      // If we are already fully rendered, but we're not forced, we don't need another render
      if (data.lastCalc && data.lastCalc.fullRender && force !== true)
        calc.needsRender = false;
      
      // Calculate the extra rows to be added to the top and bottom
      var scrolledUp      = calc.scrollTop < (data.lastCalc && data.lastCalc.scrollTop),
          extraCount      = calc.fullRender ? total : this.extraRows * capacity.hori,
          extraRowsAbove  = Math.round(this.extraRows * (scrolledUp ? .75 : .25)),
          extraAbove      = calc.fullRender ? total : extraRowsAbove * capacity.hori;
      
      // If we show indices, do fancy calculation for first item in view
      if (renderIndices) {
        var inBlock   = Math.floor(Math.max(0, calc.scrollTop - calc.block.height - size.indexHeight) / size.height),
            idx       = this.calculated.indices.indices[calc.block.idxIdx - 1];
        calc.firstInView = (idx ? this.calculated.indices.cumulCountMap[idx] : 0) + inBlock;
      }

      // Else, calculate the first and last in view based on the viewport
      else {
        calc.firstInView = Math.floor(calc.scrollTop / size.height) * capacity.hori;
      }

      // Correct for extra rows
      calc.first = Math.max(0, calc.firstInView - extraAbove);
      calc.count = Math.min(calc.total - calc.first, capacity.vert * capacity.hori + extraCount);
      // Count the number of items in the view
      calc.countInView = Math.min(calc.total - calc.firstInView, capacity.vert * capacity.hori);

      // Set indices above / below
      if (renderIndices) {
        var firstIdx = this.calculated.indices.items[calc.first];
        calc.indices.above = this.calculated.indices.posMap[firstIdx];
        var lastIdx = this.calculated.indices.items[calc.first + calc.count - 1];
        calc.indices.below = this.calculated.indices.indices.length - this.calculated.indices.posMap[lastIdx] - 1;
      }

      return data.lastCalc = calc;
    },


    // Renders the visible items
    // PRIVATE
    _renderInView: function() {
      if (!this._renderingEnabled) return;

      var items         = this.data.lastCalc,
          size          = this.calculated.size,
          capacity      = this.calculated.capacity,
          renderIndices = this.index && this.showIndex;

      // Do nothing if we need no render
      if (items.needsRender !== false) {

        // Set the HTML
        this.$inner[0].innerHTML = this.renderItems(items);
        
        // Calculate margins
        var marginTop     = (items.first / capacity.hori) * size.height,
            marginBottom  = Math.ceil((items.total - items.count - items.first) / capacity.hori) * size.height;

        // Correct for indices
        if (renderIndices) {
          marginTop     += items.indices.above * size.indexHeight;
          marginBottom  += items.indices.below * size.indexHeight;
        }

        // Set margins
        this.$inner[0].style.marginTop    = marginTop + 'px';
        this.$inner[0].style.marginBottom = marginBottom + 'px';

        // Save the items which were rendered
        this.data.lastRender = _.extend({}, items, {timestamp:new Date().getTime()});

        // Position index
        if (renderIndices)
          this.positionFloatingIndex();
      }

      // After render
      this.afterRender(items);
    },
    
    
    // Renders a set of items
    // VIRTUAL
    renderItems: function(items) {
      var models  = this.getModels(items.first, items.count),
          html    = "",
          odd     = items.first % 2 == 0,
          prevIdx;
      
      // Go through all models
      _.each(models, _.bind(function(model, i){
        
        // Draw index if requested
        if (this.calculated.indices && this.showIndex) {
          var idx = this.calculated.indices.items[items.first + i];
          if (idx != prevIdx) {
            html += this.renderIndex( this.getFloatingIndexValue(idx) );
            prevIdx = idx;
          }
        }
        
        // Render the item
        html += this.renderItem(model, items.first + i);
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
    // VIRTUAL
    renderFloatingIndex: function() {
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
      this._calculateInView(true);
      this._renderInView();
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
        blockHeight = size.indexHeight + Math.ceil(this.calculated.indices.count[idxIdx]) * size.height;
      } while ((height + blockHeight < this.scroller.scrollTop) && ++idxIdx);
      
      return {
        height:       height,
        blockHeight:  blockHeight,
        idxIdx:       idxIdx
      };
      
    },
    
    
    // Positions the floating index
    // VIRTUAL
    positionFloatingIndex: function() {
      var indexEl = this.$index[0],
          pulled  = this.scroller.scrollTop < 0,
          enabled = this.showIndex && this.calculated.indices && this.calculated.indices.indices.length;
          block   = this.data.lastCalc.block;

      // Hide index if we are pulling
      indexEl.style.display = pulled || !enabled ? 'none' : '';
      if (!enabled) return;

      // Store the top of the index which is below the top line of the view
      var nextIndexTop = block.height + block.blockHeight - this.scroller.scrollTop,
          nextIndexIdx = this.calculated.indices.indices[block.idxIdx];

      // Set floating index
      var diff = Math.min(0, nextIndexTop - this.calculated.size.indexHeight),
          left = pulled < 0 ? -10000 : 0;
      this.updateFloatingIndex(this.$index, nextIndexIdx);
      this.$index[0].style.webkitTransform = "translate3d(" + left + "px," + diff + "px,0)";
    },


    // Sets the index display
    // VIRTUAL
    updateFloatingIndex: function($index, index) {
      $index[0].innerHTML = this.getFloatingIndexValue(index);
    },


    // Gets the value to display for the given index
    // VIRTUAL
    getFloatingIndexValue: function(index) {
      return index == Touchee.nonAlphaSortValue ? "#" : index;
    },


    // Renders the quickscroll element
    // PRIVATE
    _renderQuickscroll: function() {
      
      // Build element
      var $qs = $('<ol/>')
        .addClass('quickscroll')
        .prependTo(this.$el);
      
      // Set data
      var qs = this.data.qs = {
        alpha:  this.quickscroll == 'alpha',
        $el:    $qs
      };
      
      // Fill with letters if alpha
      if (qs.alpha) {
        var qsHTML = "";
        for (var i = 65; i <= 90; i++)
          qsHTML += "<li>" + String.fromCharCode(i) + "</li>";
        qsHTML += "<li>#</li>";
        qs.$el.addClass('alpha').html(qsHTML);
      }
      
      // Set more data
      var rect    = qs.$el[0].getBoundingClientRect(),
          padding = qs.$el.css('border-top-width').numberValue();
      _.extend(qs, {
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
      delete this.data.qs.last;
      this._qsScroll(ev);
    },
    
    
    // Called when the user is scrolling the quickscroll
    // PRIVATE
    _qsScroll: function(ev) {
      var qs = this.data.qs;

      // Get the fraction of the height the user is touching
      var pageY = ev.originalEvent.touches ? ev.originalEvent.touches[0].pageY : ev.pageY,
          pos   = Math.min(Math.max(pageY - qs.top - qs.padding, 0), qs.area),
          par   = pos / qs.area
      
      // If we have an alpha scroller, get the corresponding index
      if (qs.alpha) {
        var $children = qs.$el.children(),
            i         = Math.min(Math.floor(par * $children.length), $children.length - 1),
            idx;
        
        for (i; i < $children.length; i++) {
          idx = $children.eq(i).text().toUpperCase();
          if (_.isNumber(this.calculated.indices.posMap[idx])) break;
        }
        par = idx == "#" ? "|" : idx;
      }
      
      // If this position is different then the last, kick the scrollTo method
      if (qs.last != par)
        this.scrollTo(par);
      qs.last = par;
      
      // Set hover state and disable default touch
      qs.$el.addClass('hover');
      ev.preventDefault();
    },
    
    
    // Called when the quickscroll is over;
    // PRIVATE
    _qsEnd: function(ev) {
      this.data.qs.$el.removeClass('hover');
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
            idxIdx    = this.calculated.indices.posMap[idx],
            rows      = this.calculated.indices.cumulCountMap[idx] - this.calculated.indices.count[idxIdx];
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

      // Get the element
      var $el = $(ev.currentTarget);
      
      // Set the onclick handler for anchors
      if ($el[0].tagName.toLowerCase() == 'a')
        $el[0].onclick = "return false;";
      
      // Get currently selected items
      var $previous = $el.siblings('.' + this.selection.klass);
      
      // Selection setter
      var doSelect = function() {
        $el.addClass(this.selection.klass);
        $previous.removeClass(this.selection.klass);
      };
      
      // Set selection data
      this.data.selection = {
        $el:        $el,
        $previous:  $previous,
        timeout:    _.delay(_.bind(doSelect, this), this.selection.delay),
        y:          ev.getCoords().y,
        item:       this.getItem($el),
        previous:   this.data.selection && this.data.selection.item
      };
      
      // Set bindings
      this.$scroller.on(Touchee.MOVE_EVENT+'.tss', _.bind(this._selectionMove, this));
      this.$scroller.on(Touchee.END_EVENT+'.tss touchcancel.tss',  _.bind(this._selectionEnd, this));
    },
    
    
    // Called when a touch selection is in progress and the touch position has moved
    // PRIVATE
    _selectionMove: function(ev) {
      
      // Get the data
      var data = this.data.selection;
      // if (!data) return;
      
      // If we are moving enough
      var diff = Math.abs(data.y - ev.getCoords().y);
      if (diff > this.selection.distance) {
        
        // If we are moving within the timeout, kill the timeout so the new selection is not set
        if (data.timeout)
          clearTimeout(data.timeout);
        
        // Set that we have moved
        data.moved = true;
        
        // Remove selection on the target element
        data.$el.removeClass(this.selection.klass);
        
        // We do not need move callbacks anymore
        this.$scroller.off(Touchee.MOVE_EVENT + '.tss');
      }
    },
    
    
    // Called when a touch selection is ended
    // PRIVATE
    _selectionEnd: function(ev) {
      
      // Get the data
      var data = this.data.selection;
      // if (!data) return;
      
      // If we stopped within the timeout, kill the timeout so the new selection is not set
      if (data.timeout)
        clearTimeout(data.timeout);
      
      // If we have moved, reset the original selection
      if (data.moved) {
        data.$previous.addClass(this.selection.klass);
        data.previous = data.item;
      }
      
      // Else, select the new element
      else {
        
        // Set selection class (for when we have ended the touch during the timeout)
        if (this.selection.keep) {
          data.$el.addClass(this.selection.klass);
          data.$previous.removeClass(this.selection.klass);
        }
        else
          data.$el.removeClass(this.selection.klass);
        
        // If we have a callback function, call that
        if (_.isFunction(this.selected))
          this.selected.call(this, data.item, data.$el);
        
        // Else, navigate to the anchor
        else if (data.$el.is('a'))
          Backbone.history.navigate(data.$el.attr('href'), {trigger:true});
      }
      
      // Unbind all
      this.$scroller.off(Touchee.MOVE_EVENT + '.tss');
      this.$scroller.off(Touchee.END_EVENT + '.tss touchcancel.tss');
      // delete this.data.selection;
      
      return false;
    }
    
    
  });
  
  return ScrollList;
  
  
});