(function($){


  $.fn.touchscrollselect = function(options) {

    // Disable
    if (options == false) {
      this.off('.tss');
      return;
    }

    // Set options hash
    var defaultOptions = {
      selectable: 'a',
      klass:      'selected',
      keep:        true
    };
    if (typeof options == 'function') options = {callback:options};
    options = $.extend({}, defaultOptions, options);

    var $el = this, $item, $previous, cancelled;


    // Started touching
    function touch(ev) {

      // Get the correct item, bail out if none found
      $item = $(ev.target).closest(options.selectable);
      if (!$item.length || ($previous && $item[0] == $previous[0])) return;

      // Get currently selected item
      $previous = $item.siblings(options.selectable).filter('.' + options.klass).first().removeClass(options.klass);

      // Show selection
      $item.addClass(options.klass);

      // Set bindings
      $el
        .on('drag.tss hold.tss', cancel)
        .on('release.tss', release);
    }


    // When the tap should be cancelled
    function cancel(ev) {
      // Set that we should cancel the tap
      cancelled = true;
      // Remove selection on the target item
      $item.removeClass(options.klass);
      // We do not need cancel callbacks anymore
      $el.unbind('drag.tss hold.tss');
    }


    // When the touch is released
    function release(ev) {

      // Unbind stuff
      $el.unbind('drag.tss release.tss');

      // If we have moved, reset the original selection
      if (cancelled) {
        if ($previous)
          $previous.addClass(options.klass);
        cancelled = false;
        ev.preventDefault();
        return false;
      }

      // Else, select the new item
      else {
        
        // Set selection class (for when we have ended the touch during the timeout)
        if (options.keep) {
          $item.addClass(options.klass);
          if ($previous)
            $previous.removeClass(options.klass);
        }
        else
          $item.removeClass(options.klass);
        
        // If we have a callback function, call that
        if (typeof options.callback == 'function')
          callback.call($el, $item);
        else if ($item.is('a'))
          window.location.replace($item.attr('href'));
        
        // Trigger callback
        $el.trigger('selected', [$item]);
      }

      $previous = $item;
    }

    return this.on('touch.tss', touch);

  };
  
})(jQuery);