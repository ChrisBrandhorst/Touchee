define([
  'jquery'
], function($){
  
  $.Event.prototype.getCoords = function() {
    if (this.originalEvent.targetTouches)
      return {x:this.originalEvent.targetTouches[0].pageX,y:this.originalEvent.targetTouches[0].pageY};
    else
      return {x:this.pageX,y:this.pageY};
  };
  
  $.fn.withOverlay = function(options) {
    var options   = (options || {}),
        $overlay  = $(options.overlay || document.createElement('div')).addClass('overlay'),
        $this     = this,
        $parent   = options.parent ? $(options.parent) : $this.parent();
    
    $overlay.bind('touchstart mousedown', function(ev){
      if ($(ev.target).parents().is($this)) return;
      if (options.remove)
        $this.remove();
      else
        $this.hide();
      $overlay.remove();
      ev.preventDefault();
      return false;
    });
    
    if ($overlay[0].parentNode == $this[0].parentNode)
      $overlay.insertBefore($this);
    else {
      $overlay.appendTo($parent);
      var z = $this.css('z-index');
      if (z) $overlay.css('z-index', z - 1);
    }
    
    $overlay.show();
    $this.show();
    
    return $overlay;
  }
  
});