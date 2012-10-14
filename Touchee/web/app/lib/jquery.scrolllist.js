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
  
  
  // Default options
  var defaultOptions = {
    
    // If indices should be used, this function should return an object containing:
    // - An ordered array of the current indices
    // - An array of items per index
    indices:      false,
    
    // Whether the first item of the data (the ID) should be shown
    // If the number of columns minus the index row is 0, the ID is always shown
    showID:       false,
    
    // Whether indices should be shown
    showIndices:  true,
    
    // The HTML index clone element
    indexClone:   '<div class="index" />',
    
    // jQuery, DOM element or selector for the elements which should be buffered
    elements:     '> table, > ul',
    
    // At least this number of items should be present for buffering
    min:          100,
    
    // The total number of additional rows that should be rendered around the visible portion
    rows:         80,
    
    // Function for determining the number of items of the given element
    count:        function(el,elI){return 0;},
    
    // If set to a function, uses this for custom rendering
    render:       null,
    
    // If set to a function, uses this for custom rendering of each row
    renderItem:   null,
    
    // If set to a funcftion, uses this for custom rendering of each index
    renderIndex:  null,
    
    // If set to a funcftion, uses this for custom rendering of the dummy object
    renderDummy:  null,
    
    // Function for retreiving the data to be displayed
    // Not required if render function is given
    // The first item of each returned array should be the ID
    // If there are only two items in each array, and indices are used, the first item is also seen as content
    data:       function(el,elI,items){return [];}
    
  };
  
  
  var SL = ScrollList = {
    
    // Initialization
    // We only use the first item of the jQuery object
    init: function(options) {
      if (!this.length) return;
      
      options = $.extend({}, defaultOptions, options);
      var $this = this.eq(0);
      
      // Set the index clone
      var indexClone, indexHeight;
      // if (options.indices) {
        indexClone = $(options.indexClone).html('a').addClass('index').css({
          position:                 'absolute',
          width:                    '100%',
          zIndex:                   1,
          webkitTransitionProperty: 'none',
          webkitTransform:          "translate3d(-1000%,0,0)"
        });
        
        indexClone = typeof options.indexClone == 'string'
          ? indexClone.insertBefore($this)[0]
          : indexClone[0];
      // }
      
      // Store data
      var data = {
        options:        options,
        custom:         typeof options.render == 'function',
        customItem:     typeof options.renderItem == 'function',
        customIndex:    typeof options.renderIndex == 'function',
        $elements:      $this.find(options.elements),
        lastScrollTop:  $this[0].scrollTop,
        indexClone:     indexClone
      };
      $this.data('scrolllist', data);
      
      // Do bind
      SL.bind.call($this);
      
      // Resize on window resize / re-orientation
      $(window).resize(_.debounce(function(){
        SL.resize.call($this);
      }, 50));
      
      // Do initial calculations
      SL.calculate.call($this);
      
      //
      SL.render.call($this, true);
    },
    
    
    // Bind all event handlers for the list
    bind: function() {
      var $this = this,
          data = $this.data('scrolllist');
      
      // If we have touches, do some advanced events for efficiency
      if ('ontouchstart' in document.documentElement) {
        $this.bind('touchstart.scrolllist', function(ev){
          data.touching = true;
          data.scrolling = false;
        });
        $this.bind('touchend.scrolllist', function(){
          data.touching = false;
          if (data.scrolling)
            data.scrollTimeout = setTimeout(function(){
              SL.scroll.call($this);
              delete data.scrollTimeout;
            }, 100);
          data.scrolling = false;
        });
        $this.bind('scroll.scrolllist', function(){
          data.scrolling = data.touching;
          SL.positionIndex.call($this);
          if (!data.touching && !data.scrollTimeout)
            SL.scroll.call($this);
        });
      }
      
      // Else, just use regular debounce on scroll
      else {
        $this.bind('scroll.scrolllist', _.debounce(function(){
          SL.scroll.call($this);
        }, 100));
        $this.bind('scroll.scrolllist', function(){
          SL.positionIndex.call($this);
        });
      }
    },
    
    
    // Unbind all event handlers for the list
    unbind: function() {
      var $this = this;
      $this.unbind('.scrolllist');
    },
    
    
    // We have scrolled
    scroll: function() {
      var $this = this;
      SL.render.call($this);
    },
    
    
    // We have resized
    resize: function() {
      var $this = this;
      SL.calculateCapacity.call($this);
    },
    
    
    // Calculates the size of shown elements and how many can be shown in the current viewport
    // Calculation is done for only the first elements: all elements are assumed of equal type
    calculate: function() {
      var $this = this,
          data = $this.data('scrolllist');
      
      // If there are no elements, bail out
      if (!data.$elements.length) return;
      
      // If we are not visible, forget it
      if (!$this.is(':visible')) return;
      
      // Get the first element
      var $element = data.$elements.eq(0),
          $dummy;
      
      // If there are no children, create a dummy
      if (!$element.children().length) {
        $dummy = $(
          typeof data.options.renderDummy == 'function'
            ? data.options.renderDummy.call($this)
            : '<tr><td>&nbsp;</td></tr>'
        ).appendTo($element);
      }
      
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
      
      // Calculate size of clone
      data.size.indexHeight =
        data.options.indexClone && data.options.showIndices
        ? $(data.indexClone).outerHeight()
        : 0;
      
      // Calculate capacity
      SL.calculateCapacity.call($this);
    },
    
    
    // Calculates the capacity of items within the viewport
    calculateCapacity: function() {
      var $this     = this,
          data      = $this.data('scrolllist'),
          container = $this[0];
      
      // If we have no size, nothing to do
      if (!data.size) return;
      
      // Get capacities
      data.capacity = {
        vert: Math.ceil(container.clientHeight / data.size.height) + 1,
        hori: Math.floor(container.clientWidth / data.size.width)
      };
      data.capacity.total = data.capacity.vert * data.capacity.hori;
    },
    
    
    // Render
    render: function(force) {
      var $this = this,
          data = $this.data('scrolllist'),
          start = new Date();
      
      // If there are no elements, no point in going further
      if (!data.$elements.length) return;
      
      // Set scrolling method :-(
      if ($this[0].scrollTop > Math.pow(2,17) - 1000)
        $this[0].style.webkitOverflowScrolling = "auto";
      else
        $this[0].style.webkitOverflowScrolling = "touch";
      
      // Calculate sizes if not present
      if (!data.size)
        SL.calculate.call($this);
      
      // Check if we scrolled up or down
      var scrollTop       = $this[0].scrollTop,
          scrolledDown    = scrollTop >= data.lastScrollTop,
          visibleHeight   = $this[0].clientHeight;
      data.lastScrollTop  = scrollTop;
      
      // Loop through all elements
      data.$elements.each(function(i){
        
        // Get the number of items for this element
        var count       = data.options.count.call(this, this, i),
            size        = data.size,
            capacity    = data.capacity,
            doRender    = true,
            html        = this.innerHTML,
            items       = {},
            indicesCorrection;
        
        // Check if we need to do a full render
        var fullRender = count < data.options.min;
        
        // If the item is below the visible window, render nothing
        if (this.offsetTop - this.style.marginTop.numberValue() > scrollTop + visibleHeight) {
          items = {
            first:  0,
            last:   -1
          };
          doRender    = false;
        }
        
        // If we need to render less then the minimum, render all
        else if (fullRender) {
          // If we have content and are not forced, nothing should be changed
          if (html != "" && !force) return;
          items = {
            first:  0,
            last:   count - 1
          };
        }
        
        // Else, calculate what portion to show
        else {
          var rows  = data.options.rows * capacity.hori,
              above = Math.round(rows * (scrolledDown ? .25 : .75)),
              below = rows - above;
          
          // Calculate the first and last in view based on the viewport
          items.first = Math.floor((scrollTop - (this.offsetTop - this.style.marginTop.numberValue())) / size.height) * capacity.hori;
          items.last  = items.first + capacity.vert * capacity.hori - 1;
          
          // Modify the first in view to take into account the additional rows
          items.first = Math.max(0, items.first - above * capacity.hori);
          items.last  = Math.min(count - 1, items.last + below * capacity.hori);
          
          // Make sure all is within bounds
          items.first = Math.min(count - 1, items.first);
          items.last  = Math.max(0, items.last);
          
          // Compensate for indices
          if (data.options.indices) {
            var indices           = data.options.indices().indices,
            lastCorrectionBefore  = -1,
            getIndicesCorrection  = function(f, l) {
              var firstItem         = data.options.data.call(this, this, i, items.first),
                  lastItem          = data.options.data.call(this, this, i, items.last);
              return {
                before:   indices.indexOf( firstItem[firstItem.length-1] ),
                after:    indices.length - indices.indexOf( lastItem[firstItem.length-1] ) - 1
              };
            };
            
            while(
              (indicesCorrection = getIndicesCorrection(items.first, items.last)).before
              != lastCorrectionBefore
            ) {
              firstD = Math.ceil(indicesCorrection.before / (size.height / size.indexHeight))
              items.first -= Math.min(items.first, firstD);
              lastCorrectionBefore = indicesCorrection.before;
            }
            
          }
          
        }
        
        // Render the data
        if (doRender) {
          
          // Do custom render
          if (data.custom)
            html = data.options.render.call(this, this, i, items);
          
          // Default table render
          else {
            html = [];
            
            // Get the data
            var d   = data.options.data.call(this, this, i, items.first, items.last),
                odd = items.first % 2 > 0,
                lastIndex, columnCount;
            
            if (d.length) {
              columnCount = d[0].length;
              if (data.options.indices) columnCount--;
              if (!data.options.showID) columnCount--;
            }
            
            // Loop through data
            for (j in d) {
              
              // Get row data and pull out ID
              var r = d[j];
              
              // Set index if necessary
              if (data.options.indices) {
                var index = r.pop();
                
                if (data.options.showIndices && index != lastIndex) {
                  lastIndex = index;
                  html = html.concat(
                    data.customIndex
                      ? data.options.renderIndex.call(this, index, columnCount)
                      : ['<tr class="index" data-index="', index, '"><td>', index, '</td>', new Array(columnCount).join('<td></td>'), '</tr>']
                  );
                }
                
              }
              
              // Get id
              var id = data.options.showID || r.length == 1 ? r[0] : r.shift();
              
              // Put HTML
              html = html.concat(
                data.customItem
                  ? data.options.renderItem.call(this, id, r, odd)
                  : [
                      '<tr ',
                      (odd ? 'class="odd" ' : ''),
                      'data-id="', id,
                      '" data-index="', items.first+j*1,
                      '">',
                      $.map(r, function(d){
                        return ['<td>', d ? d.toString().htmlEncode() : "", '</td>'].join('');
                      }).join(''),
                      '</tr>'
                    ]
                  
              );
              odd = !odd;
            }
            
            // Join the HTML
            html = html.join('');
          }
          
        }
        $('#debug').text("rendered in " + (new Date() - start) + " ms");
        // Set the HTML
        this.innerHTML = html;
        
        // Set margins
        if (!fullRender) {
          var marginTop     = (items.first / capacity.hori) * size.height,
              marginBottom  = Math.ceil((count - 1 - items.last) / capacity.hori) * size.height;
          if (indicesCorrection) {
            marginTop     += indicesCorrection.before * size.indexHeight;
            marginBottom  += indicesCorrection.after * size.indexHeight;
          }
          this.style.marginTop    = marginTop + 'px';
          this.style.marginBottom = marginBottom + 'px';
        }
        
      });
      
      
      
    },
    
    
    // 
    positionIndex: function() {
      var $this           = this,
          content         = $this[0],
          data            = $this.data('scrolllist'),
          contentTop      = content.getBoundingClientRect().top,
          cloneInContent  = false;
      
      // Get all indices, bail out if none
      var indices = content.getElementsByClassName('index');
      if (indices.length < 1) return;
      
      // Find the first index in view
      var active, top, i = 0;
      if (indices.length == 1) {
        top = indices[0].getBoundingClientRect().top - contentTop;
        if (top <= 0)
          active = indices[0];
      }
      else {
        var index;
        for (i; i < indices.length; i++) {
          index = indices[i];
          top = index.getBoundingClientRect().top - contentTop;
          
          if (top > 0) {
            i--;
            break;
          }
          active = index;
        }
      }
      
      // If there is no active, hide the clone and bail out
      if (!active)
        return data.indexClone.style.webkitTransform = "translate3d(-1000%,0,0)";
      
      //
      data.indexClone.textContent = active.textContent;
      
      //
      var next    = indices[i+1],
          nextTop = next ? next.getBoundingClientRect().top - contentTop : data.indexClone.offsetHeight;
      
      var y = Math.max(0, data.indexClone.offsetHeight - nextTop);
      data.indexClone.style.webkitTransform = "translate3d(0," + -y + "px,0)";
    },
    
    
    // Scroll to the specified position in pixels or fraction of the height
    scrollTo: function(params) {
      var $this = this,
          data  = $this.data('scrolllist');
      
      // A fraction was given
      if (typeof params.fraction == 'number')
        pos = params.fraction * ($this[0].scrollHeight - $this[0].clientHeight);
      
      // A pixel count was given
      else if (typeof params.pixels == 'number')
        pos = params.pixels;
      
      // An index was given
      else if (typeof params.index != 'undefined') {
        
        // No indices function: bail out
        if (!data.options.indices) return;
        
        // Get the index position
        var indices   = data.options.indices(),
            charCode  = params.index.toUpperCase().charCodeAt(0),
            isAlpha   = !!params.index.match(/[a-z]/i),
            indexIndex;
        for (var i = 0; i < indices.indices.length; i++) {
          if (isAlpha && indices.indices[i].charCodeAt(0) >= charCode || !indices.indices[i].match(/[a-z]/i) && !isAlpha) {
            indexIndex = i;
            break;
          }
        }
        if (typeof indexIndex != 'number') indexIndex = indices.indices.length - 1;
        
        // Count the items
        var itemCount = 0;
        for (i = 0; i < indexIndex; i++)
          itemCount += indices.count[i];
        
        // Get the pixel position of the item
        var element = data.$elements[0];
        pos = (element.offsetTop - element.style.marginTop.numberValue()) + Math.floor(itemCount / data.capacity.hori) * data.size.height;
        pos += indexIndex * data.size.indexHeight;
      }
      
      // Bail out if no valid pos was recieved
      if (typeof pos != 'number') return;
      
      // Unbind so rendering is not kicked
      SL.unbind.call($this);
      
      // Set scrolltop and manually render
      $this[0].scrollTop = pos;
      SL.render.call($this, true);
      SL.positionIndex.call($this);
      
      // Re-bind
      setTimeout(function(){
        SL.bind.call($this);
      });
      
    }
    
  }
  
  
  // Set bufferedlist as jQ function
  $.fn.scrolllist = function( method ) {
    if ( ScrollList[method] )
      return ScrollList[method].apply( this, Array.prototype.slice.call( arguments, 1 ));
    else if ( typeof method === 'object' || ! method )
      return ScrollList.init.apply( this, arguments );
    else
      $.error( 'Method ' +  method + ' does not exist on jQuery.scrolllist' );
  };

  
  
})(jQuery);