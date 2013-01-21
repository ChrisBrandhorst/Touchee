define([
  'jquery',
  'underscore',
  'Backbone',
  'Touchee'
], function($, _, Backbone, Touchee) {
  
  var ScrollList = Backbone.View.extend({
    
    // Backbone View options
    tagName:      'section',
    className:    'scrollable scroll_list',
    
    
    // Type of scrolllist
    listType:     'base',
    // Element used as floating index
    index:        '<div/>',
    // jQuery object for the inner element. If this is not given, a new element is made
    // using the innerTagName value
    $inner:       null,
    // The tag for the inner element
    innerTagName: 'div',
    // Whether to show indices in the list
    showIndices:  true,
    // From which number of items the list is rendered in one piece
    min:          0,
    // The number of extra rows to render outside the visible portion
    extraRows:    80,
    
    
    // Constructor
    initialize: function() {
      this.$el.addClass(this.listType);
      
      this.calculated = {};
      this.data = {};
    },
    
    
    // Render
    render: _.once(function() {
      
      // Check if we are visible
      if (!this.$el.is(':visible'))
        return Touchee.Log.error("Cannot render ScrollList if it is not visible yet!");
      
      // Build the floating index if required
      if (this.showIndices)
        this.$index = $(this.index)
          .addClass('scroll_list-' + this.listType + '-index index')
          .html("?")
          .insertBefore(this.$el);
      
      // Create inner element
      if (!this.$inner)
        this.$inner = $('<' + (this.innerTagName || 'div') + '/>').prependTo(this.$el);
      
      // Calculate size of elements and capacity
      this.calculateSizes();
      this.calculateCapacity();
      
      // Bind events
      this.bind();
      
      // Do first render
      this.renderInView();
    }),
    
    
    // Bind all event handlers for the list
    bind: function() {
      var touching    = false,
          scrolling   = false,
          scrollList  = this,
          scrollTimeout;
      
      // If we have touches, do some advanced events for efficiency
      if ('ontouchstart' in document.documentElement) {
        // Start touch
        this.$el.bind('touchstart.scroll_list', function(ev){
          touching = true;
          scrolling = false;
        });
        // Touch end: if we are scrolling, do a render after a small delay
        this.$el.bind('touchend.scroll_list', function(){
          touching = false;
          if (scrolling)
            scrollTimeout = setTimeout(function(){
              scrollList.renderInView();
              scrollTimeout = null;
            }, 100);
          scrolling = false;
        });
        // If we are scrolling whilst not touching, do a render
        this.$el.bind('scroll.scroll_list', function(){
          scrolling = touching;
          scrollList.renderIndex();
          if (!touching && !scrollTimeout)
            scrollList.renderInView();
        });
      }
      
      // Else, just use regular debounce on scroll
      else {
        this.$el.bind('scroll.scroll_list', _.debounce(_.bind(this.renderInView, this), 100));
        this.$el.bind('scroll.scroll_list', _.bind(this.renderIndex, this));
      }
      
      // Recalculate capacity when the view is resized
      $(window).bind('resize.scroll_list-' + this.id, _.debounce(
        _.bind(this.calculateCapacity, this), 100, true
      ));
      
    },
    
    
    // Unbind all event handlers for the list
    unbind: function() {
      $(window).unbind('resize.scroll_list-' + this.id);
      this.$el.unbind('.scroll_list');
    },
    
    
    // Calculates the size of shown elements
    calculateSizes: function() {
      var $children = this.$inner.children(),
          size      = {},
          $dummy, $item;
      
      // Get a reference to a child item. If there are no children, create a dummy
      if (!$children.length)
        $dummy = $( _.result(this, 'dummy') ).appendTo(this.$inner);
      else
        $item = $children.first();
      
      // If we have a table, get the height based on the inner height of the table
      // and the number of rows. Width is irrelevant
      if (this.$inner.is('table'))
        size = {
          height: this.$inner.innerHeight() / this.$inner.children().length,
          width:  this.$inner.outerWidth()
        };
      
      // Else, get both the height and width including padding, border and margin
      else {
        size = {
          height: $item.outerHeight(true),
          width:  $item.outerWidth(true)
        }
      }
      
      // Remove dummy
      if ($dummy)
        $dummy.remove();
      
      // Calculate size of indices
      size.indexHeight = this.showIndices ? this.$index.outerHeight() : 0;
      
      // Set data in model
      this.calculated.size = size;
      
    },
    
    
    // Calculates the capacity of the view
    calculateCapacity: function() {
      var container = this.$el[0],
          capacity  = {};
      
      // Calculates the capacity of items within the viewport
      capacity = {
        vert: Math.ceil(container.clientHeight / this.calculated.size.height),
        hori: Math.floor(container.clientWidth / this.calculated.size.width)
      };
      capacity.total = capacity.vert * capacity.hori;
      
      // Set data in model
      this.calculated.capacity = capacity;
    },
    
    
    // Renders the visible items
    renderInView: function(force) {
      var el        = this.el,
          size      = this.calculated.size,
          capacity  = this.calculated.capacity,
          data      = this.data;
      
      // Set scrolling method :-(
      if (el.scrollTop > Math.pow(2,17) - 1000)
        el.style.webkitOverflowScrolling = "auto";
      else
        el.style.webkitOverflowScrolling = "touch";
      
      // If we are already fully rendered, but we're not forced, bail out
      if (data.fullRender && !force) return;
      else data.fullRender = false;
      
      // Get the total number of items
      // Check if we need to do a full render
      // Check if we scrolled up or down
      var total         = this.getCount(),
          fullRender    = total < this.min,
          scrollTop     = el.scrollTop,
          visibleHeight = el.clientHeight,
          items         = {};
      
      // Render first to last item with a full render
      if (fullRender)
        items = { first: 0, count: total };
      
      // Else, calculate which item we should show
      else {
        var scrolledDown  = scrollTop >= data.lastScrollTop
            extraCount    = this.extraRows * capacity.hori,
            extraAbove    = Math.round(extraCount * (scrolledDown ? .25 : .75)),
            extraBelow    = extraCount - extraAbove;
        
        // Calculate the first and last in view based on the viewport
        items.first = Math.floor((scrollTop - (el.offsetTop - el.parentNode.offsetTop)) / size.height) * capacity.hori;
        items.count = capacity.vert * capacity.hori;
        // Modify the first in view to take into account the additional rows
        items.first -= extraAbove;
        items.count += extraCount;
        // Make sure all is within bounds
        items.first = Math.max(0, items.first);
        items.count = Math.min(total - items.first, items.count);
      }
      
      // Get the models. The items object may be changed!
      var models  = this.getModels(items),
          odd     = items.first % 2 == 0;
      
      // Set the HTML
      this.$inner[0].innerHTML = this.renderItems(models, {odd:odd})
      
      // Set the margins
      var marginTop     = (items.first / capacity.hori) * size.height,
          marginBottom  = Math.ceil((total - items.count - items.first) / capacity.hori) * size.height;
      this.$inner[0].style.marginTop    = marginTop + 'px';
      this.$inner[0].style.marginBottom = marginBottom + 'px';
      
      // Store stuff for next time
      data.lastScrollTop = scrollTop;
      data.fullRender = fullRender;
    },
    
    
    // Gets the model count
    getCount: function() {
      throw('NotImplementedException');
    },
    
    
    // Gets the models
    getModels: function(items) {
      throw('NotImplementedException');
    },
    
    
    // Renders a set of item
    renderItems: function(models, options) {
      var html = "", odd = options.odd;
      _.each(models, _.bind(function(model){
        html += this.renderItem(model, {odd:odd});
        odd = !odd;
      }, this));
      return html;
    },
    
    
    // Renders a single item
    renderItem: function(item, options) {
      throw('NotImplementedException');
    },
    
    
    // Renders the floating index
    renderIndex: function() {
      
    },
    
    
  });
  
  return ScrollList;
  
});