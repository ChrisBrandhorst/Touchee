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
    
    $overlay.bind('tap', function(ev){
      if ($(ev.target).parents().is($this)) return;
      if (typeof options.remove == 'function')
        options.remove.call($this);
      else if (options.remove === true)
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
  };
  
  $.fn.getAnimDuration = function() {
    var durationStr = this.css('-webkit-transition-duration'), duration = 0;
    
    if (durationStr) {
      var match = durationStr.match(/[\d.]*/);
      if (match && match[0])
        duration = Number((Number(match[0]) * 1000).toFixed(0));
    }
    
    return duration;
  };
  
  $.fn.invisible = function() {
    this.each(function(){
      jQuery._data( this, "oldvisibility", jQuery.css(this, "visibility") );
      this.style.visibility = "hidden";
    });
  };
  
  $.fn.visible = function() {
    this.each(function(){
      this.style.visibility = jQuery._data( this, "oldvisibility" ) || "";
    });
  };
  
  $.fn.disable = function() {
    this.attr('disabled', "disabled");
  };
  
  $.fn.enable = function() {
    this.removeAttr('disabled');
  };
  
});