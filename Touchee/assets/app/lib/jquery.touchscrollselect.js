(function($){
  
  function getCoords(ev) {
    if (ev.originalEvent.targetTouches)
      return {x:ev.originalEvent.targetTouches[0].pageX,y:ev.originalEvent.targetTouches[0].pageY};
    else
      return {x:ev.pageX,y:ev.pageY};
  }
  
  var SUPPORTS_TOUCHES  = 'ontouchstart' in document.documentElement,
      START_EVENT       = SUPPORTS_TOUCHES ? 'touchstart' : 'mousedown',
      MOVE_EVENT        = SUPPORTS_TOUCHES ? 'touchmove'  : 'mousemove',
      END_EVENT         = SUPPORTS_TOUCHES ? 'touchend'   : 'mouseup';
  
  $.fn.touchscrollselect = function(options) {
    
    // Set options hash
    var defaultOptions = {
      delay:          0,
      distance:       5,
      selectable:     'a',
      selectedClass:  'selected',
      keepSelection:  true
    };
    if (typeof options == 'function') options = {callback:options};
    options = $.extend({}, defaultOptions, options);
    
    
    function startSelect(ev) {
      
      // Get the correct item, bail out if none found
      var $item = $(ev.target).closest(options.selectable);
      if (!$item.length) return;
      
      // Set the onclick handler for anchors
      if ($item[0].tagName.toLowerCase() == 'a')
        $item[0].onclick = "return false;";
      
      // Get currently selected items
      var $previous = $item.siblings('.' + options.selectedClass);
      
      // Selection kicks in after a small delay
      var delayed = function() {
        $item.addClass(options.selectedClass);
        $previous.removeClass(options.selectedClass);
      };
      
      // Set selection data
      this.__tss = {
        $item:      $item,
        $previous:  $previous,
        timeout:    setTimeout($.proxy(delayed, this), options.delay),
        y:          ev.getCoords().y
      };
      
      // Set bindings
      this.bind(MOVE_EVENT + '.tss', $.proxy(moveSelect, this));
      this.bind(END_EVENT + '.tss',  $.proxy(endSelect, this));
    }
    
    
    function moveSelect(ev) {
      
      // Get the data
      var d = this.__tss;
      if (!d) return;
      
      // If we are moving enough
      var diff = Math.abs(d.y - ev.getCoords().y);
      if (diff > options.distance) {
        
        // If we are moving within the timeout, kill the timeout so the new selection is not set
        if (d.timeout)
          clearTimeout(d.timeout);
        
        // Set that we have moved
        d.moved = true;
        
        // Remove selection on the target item
        d.$item.removeClass(options.selectedClass);
        
        // We do not need move callbacks anymore
        this.unbind(MOVE_EVENT + '.tss');
      }
    }
    
    
    function endSelect(ev) {
      
      // Get the data
      var d = this.__tss;
      if (!d) return;
      
      // If we stopped within the timeout, kill the timeout so the new selection is not set
      if (d.timeout)
        clearTimeout(d.timeout);
      
      // If we have moved, reset the original selection
      if (d.moved)
        d.$previous.addClass(options.selectedClass);
      
      // Else, select the new item
      else {
        
        // Set selection class (for when we have ended the touch during the timeout)
        if (options.keepSelection) {
          d.$item.addClass(options.selectedClass);
          d.$previous.removeClass(options.selectedClass);
        }
        else
          d.$item.removeClass(options.selectedClass);
        
        // If we have a callback function, call that
        if (typeof options.callback == 'function')
          callback.call(this, d.$item);
        
        // Else, navigate to the anchor
        else if (d.$item.is('a'))
          window.location.replace(d.$item.attr('href'));
      }
      
      // Unbind all
      this.unbind(MOVE_EVENT + '.tss');
      this.unbind(END_EVENT + '.tss');
      delete this.__tss;
      
      return false;
    }
    
    
    return this.addClass('touchscrollselect').bind(START_EVENT + '.tss', function(ev){
      var $area = $(ev.target).closest('.touchscrollselect');
      startSelect.call($area, ev);
    });
    
  };
  
})(jQuery);