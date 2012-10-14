define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/base',
  'text!views/contents/quickscroll.html'
], function($, _, Backbone, ContentsBase, quickscrollTemplate) {
  quickscrollTemplate = _.template(quickscrollTemplate);
  
  var Quickscroll = Backbone.View.extend({
    
    events: {
      'touchstart': 'start',
      'touchstart': 'hover',
      'touchmove':  'hover',
      
      'mousedown':  'start',
      'mousedown':  'hover',
      'mousemove':  'hover'
    },
    
    // Constructor
    initialize: function(params) {
      this.alpha = params.alpha;
      this.callback = params.callback;
      this.$el = $(quickscrollTemplate(this));
    },
    
    start: function() {
      delete this.lastPos;
    },
    
    // 
    hover: function(ev) {
      
      var top           = this.$el.offset().top,
          paddingTop    = this.$el.css('padding-top').numberValue(),
          height        = this.$el.height(),
          pageY         = ev.originalEvent.touches ? ev.originalEvent.touches[0].pageY : ev.pageY,
          pos           = Math.min(Math.max(pageY - top - paddingTop, 0), height),
          perc          = pos / height;
      
      if (this.alpha) {
        var $children = this.$el.children(),
            i         = Math.min(Math.floor(perc * $children.length), $children.length - 1),
            index     = $children.eq(i).text().toUpperCase();
        pos = index;
      }
      else
        pos = perc;
      
      if (this.lastPos != pos)
        this.callback(pos);
      this.lastPos = pos;
      
      ev.preventDefault();
    },
    
    // 
    end: function(ev) {
      // delete this.lastPos;
    }
    
  });
  
  
  return Quickscroll;
});