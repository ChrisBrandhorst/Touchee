(function($){
  
  // Using some Underscore methods
  var _ = {}
  _.debounce = function(func, wait) {
    var timeout;
    return function() {
      var context = this, args = arguments;
      var later = function() {
        timeout = null;
        func.apply(context, args);
      };
      clearTimeout(timeout);
      timeout = setTimeout(later, wait);
    };
  };
  
  
  var BC = BufferedContent = {
    
    // Default options
    defaultOptions: {
      
      // jQuery, DOM element or selector for the elements which should be buffered
      elements:   '',
      
      // Template for calculating the size of elements
      template:   "",
      
      // At least this number of items should be present for buffering
      min:        100,
      
      // The number of rows above and below the visible portion to render
      rows:       80,
      
      // Function for determining the number of items of the given element
      count:      function(element,i){return 0;},
      
      // Rendering function. Should return HTML
      render:     function(element,i){return "";}
    },
    
    
    // Initialization
    // We only use the first item of the jQuery object
    init: function(options) {
      if (!this.length) return;
      
      options = $.extend({}, BufferedContent.defaultOptions, options);
      var $this = this.eq(0);
      
      // Store data
      $this.data('bufferedcontent', {
        options:        options,
        $elements:      $this.find(options.elements),
        lastScrollTop:  $this[0].scrollTop
      });
      
      // Bind scroll and resize handler
      $this.scroll(_.debounce(function(){
        BC.scroll.call($this);
      }, 100));
      $this.scroll(function(){
        BC.placeIndex.call($this);
      });
      $(window).resize(_.debounce(function(){
        BC.resize.call($this);
      }, 50));
      
      // Do initial calculations
      BC.calculate.call($this);
    },
    
    
    // We have scrolled
    scroll: function() {
      var $this = this;
      BC.render.call($this);
    },
    
    
    // We have resized
    resize: function() {
      var $this = this;
      
      // Recalculate element capacity
      BC.calculateCapacity.call($this);
    },
    
    
    // Calculates the size of shown elements and how many can be shown in the current viewport
    // Calculation is done for only the first elements: all elements are assumed of equal type
    calculate: function() {
      var $this = this,
          data = $this.data('bufferedcontent');
      
      // If there are no elements, bail out
      if (!data.$elements.length) return;
      
      // If we are not visible, forget it
      if (!$this.is(':visible')) return;
      
      // Get the first element
      var $element = data.$elements.eq(0),
          $dummy;
      
      // If there are no children, create a dummy
      if (!$element.children().length)
        $dummy = $(data.options.dummy).appendTo($element);
      
      // If we have a table, get the height based on the inner height of the table
      // and the number of rows. Width is irrelevant
      if ($element.is('table'))
        data.size = {
          height: $element.innerHeight() / $element.children().length,
          width:  $element.outerWidth()
        };
      
      // Else, get both the height and width including padding, border and margin
      else {
        var $first = $element.children(':first-child');
        data.size = {
          height: $first.outerHeight(true),
          width:  $first.outerWidth(true)
        }
      }
      
      // Remove dummy
      if ($dummy)
        $dummy.remove();
      
      // Calculate capacity
      BC.calculateCapacity.call($this);
    },
    
    
    // Calculates the capacity of items within the viewport
    calculateCapacity: function() {
      var $this     = this,
          data      = $this.data('bufferedcontent'),
          container = $this[0];
      
      // If we have no size, nothing to do
      if (!data.size) return;
      
      // Get capacities
      data.capacity = {
        vert: Math.ceil(container.clientHeight / data.size.height + 1),
        hori: Math.floor(container.clientWidth / data.size.width)
      };
      data.capacity.total = data.capacity.vert * data.capacity.hori;
    },
    
    
    // Render
    render: function() {
      var $this = this,
          data = $this.data('bufferedcontent');
      
      // If there are no elements, no point in going further
      if (!data.$elements.length) return;
      
      // Calculate sizes if not present
      if (!data.size)
        BC.calculate.call($this);
      
      // Check if we scrolled up or down
      var scrollTop     = $this[0].scrollTop,
          scrolledDown  = scrollTop >= data.lastScrollTop;
      data.lastScrollTop = scrollTop;
      
      // Loop through all elements
      data.$elements.each(function(i){
        
        // Get the number of items for this element
        var count = data.options.count.call(this, this, i),
            firstInView, lastInView, size, capacity,
            correction = {top:0,bottom:0};
        
        // Check if we need to do a full render
        var fullRender = count < data.options.min;
        
        // If we need to render less then the minimum, render all
        if (fullRender) {
          firstInView = 0;
          lastInView = count - 1;
        }
        
        // Else, calculate which items to render
        else {
          var size      = data.size,
              capacity  = data.capacity,
              above     = Math.round(data.options.rows * (scrolledDown ? .25 : .75)),
              below     = data.options.rows - above;
          
          // Calculate the first and last in view based on the viewport
          firstInView = Math.floor((scrollTop - (this.offsetTop - this.style.marginTop.numberValue())) / size.height) * capacity.hori;
          lastInView = firstInView + capacity.vert * capacity.hori - 1;
          
          // Modify the first in view to take into account the additional rows
          firstInView = Math.max(0, firstInView - above * capacity.hori);
          lastInView = Math.min(count - 1, lastInView + below * capacity.hori);
          
          // Make sure all is within bounds
          firstInView = Math.min(count - 1, firstInView);
          lastInView = Math.max(0, lastInView);
          
          // Get correction
          if (data.options.correction) {
            
            var lastCorrectionTop = -1;
            while(
              (correction = data.options.correction.call(this, this, i, firstInView, lastInView)).top
              != lastCorrectionTop
            ) {
              firstD = Math.min(firstInView, Math.ceil(correction.top / size.height));
              firstInView -= firstD;
              lastCorrectionTop = correction.top;
            }
            
          }
          
          // If we have a table, make sure we always render an even amount (because of odd/even styling)
          // if (firstInView % 2 > 0 && this.tagName.toLowerCase() == 'table')
          //   firstInView--;
        }
        
        // Render the data
        var html = data.options.render.call(this, this, i, firstInView, lastInView);
        this.innerHTML = html;
        if (!fullRender) {
          
          var marginTop     = (firstInView / capacity.hori) * size.height,
              marginBottom  = Math.ceil((count - 1 - lastInView) / capacity.hori) * size.height;
          
          marginTop += correction.top;
          marginBottom += correction.bottom;
          
          // if (this.nextSibling)
          //   this.nextSibling.style.marginTop = marginTop + marginBottom + 'px';
          // else
          //   this.style.marginBottom = marginTop + marginBottom + 'px';
          
          this.style.marginTop = marginTop + 'px';
          this.style.marginBottom = marginBottom + 'px';
          // this.style.webkitTransform = "translate3d(0," + marginTop + "px,0)";
          
        }
        
      });
      
      BC.placeIndex.call($this);
      
    },
    
    
    // Places the indices (if any)
    placeIndex: function() {
      var $this   = this,
          content = $this[0],
          data    = $this.data('bufferedcontent');
      
      // Get all indices, bail out if none
      var indices = content.getElementsByClassName('index');
      if (indices.length < 2) return;
      
      // 
      var active, top, i = 1;
      if (indices.length == 2) {
        top = $(indices[1]).position().top;
        if (top < 0)
          active = indices[1];
      }
      else {
        var index;
        for (i; i < indices.length; i++) {
          index = indices[i];
          top = $(index).position().top;
          if (top >= 0) {
            i--;
            break;
          }
          active = index;
        }
      }
      
      // Get clone
      var clone = indices[0];
      
      // 
      if (!active) {
        clone.style.webkitTransform = "translate3d(-100%,0,0)";
        return;
      }
      
      //
      clone.textContent = active.textContent;
      
      //
      var next    = indices[i+1],
          nextTop = next ? $(next).position().top : 0;
      
      var y = $this[0].scrollTop;
      if (nextTop <= active.offsetHeight)
        y -= active.offsetHeight - nextTop + 1;
      
      // 
      clone.style.webkitTransform = "translate3d(0," + y + "px,0)";
      
      
    }
    
    
  };
  
  // Set bufferedlist as jQ function
  $.fn.bufferedcontent = function( method ) {
    if ( BufferedContent[method] )
      return BufferedContent[method].apply( this, Array.prototype.slice.call( arguments, 1 ));
    else if ( typeof method === 'object' || ! method )
      return BufferedContent.init.apply( this, arguments );
    else
      $.error( 'Method ' +  method + ' does not exist on jQuery.bufferedcontent' );
  };
  
  
})(jQuery);