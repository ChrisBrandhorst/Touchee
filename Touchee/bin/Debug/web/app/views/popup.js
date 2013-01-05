define([
  'jquery',
  'underscore',
  'Backbone'
], function($, _, Backbone) {
  
  var PopupView = Backbone.View.extend({
    
    // Backbone view options
    tagName:    'section',
    className:  'popup arrow with_header',
    
    // Touchee options
    arrow:      false,
    
    
    dummy: '<div class="paged">  <header>    <h1>Blackbox II</h1>  </header>  <div>    <nav class="list scrollable icons">      <a href="#" class="music">Music</a>      <a href="#" class="movies">Films</a>      <a href="#" class="tv">TV Programmes</a>      <a href="#" class="sharedlibrary more selected">Blackbox II asd asd asda sdasd asdasd</a>      <a href="#" class="">Podcasts</a>      <a href="#" class="">iTunes U</a><a href="#" class="">Music</a>      <a href="#" class="">Films</a>      <a href="#" class="">TV Programmes</a>      <a href="#" class="">Podcasts</a>      <a href="#" class="">iTunes U</a>    <a href="#" class="">Music</a>      <a href="#" class="">Films</a>      <a href="#" class="">TV Programmes</a>      <a href="#" class="">Podcasts</a>      <a href="#" class="">iTunes U</a> <a href="#" class="">Music</a>      <a href="#" class="">Films</a>      <a href="#" class="">TV Programmes</a>      <a href="#" class="">Podcasts</a>      <a href="#" class="">iTunes U</a></nav>  </div></div>',
    
    arrowSize:  19,
    minMargin:  11,
    
    
    // Constructor
    initialize: function(options) {
      options || (options = {});
      
      this.$el.html(this.dummy);
      this.$arrow = $('<div class="arrow" />');
      this.$el.prepend(this.$arrow);
    },
    
    
    
    showRelativeTo: function(target) {
      var $target = $(target).first();
      target = $target[0];
      
      // Set on body
      this.$el.addClass('hidden').appendTo(document.body);
      
      // Reset popup
      this.$el.css({height:""});
      this.$arrow.css({left:"",top:""});
      
      // Get some vars
      var targetPos     = $target.offset(),
          targetWidth   = $target.outerWidth(),
          targetHeight  = $target.outerHeight(),
          popupWidth    = this.$el.outerWidth(),
          popupHeight   = this.$el.outerHeight(),
          bodyWidth     = document.body.offsetWidth,
          bodyHeight    = document.body.offsetHeight;
      
      // Position check function
      var checkPosition = _.bind(function(pos) {
        
        // Calculate overflows
        var checks = {
          topOverflow:    this.minMargin - pos.top,
          bottomOverflow: pos.top + popupHeight - (bodyHeight - this.minMargin),
          leftOverflow:   this.minMargin - pos.left,
          rightOverflow:  pos.left + popupWidth - (bodyWidth - this.minMargin)
        };
        
        // Set checks
        _.extend(checks, {
          top:    checks.topOverflow <= 0,
          bottom: checks.bottomOverflow <= 0,
          left:   checks.leftOverflow <= 0,
          right:  checks.rightOverflow <= 0
        });
        
        // Set ok
        checks.ok = checks.top && checks.left && checks.right && checks.bottom;
        return checks;
      }, this);
      
      // Pos storage
      var pos = {};
      
      // Calculate positions
      pos.top = {
        arrow:  'bottom',
        top:    targetPos.top - popupHeight - this.arrowSize,
        left:   targetPos.left + targetWidth / 2 - popupWidth / 2
      };
      pos.bottom = {
        arrow:  'top', 
        top:    targetPos.top + targetHeight + this.arrowSize,
        left:   pos.top.left
      };
      pos.left = {
        arrow:  'right',
        top:    targetPos.top + targetHeight / 2 - popupHeight / 2,
        left:   targetPos.left - popupWidth - this.arrowSize
      };
      pos.right = {
        arrow:  'left',
        top:    pos.left.top,
        left:   targetPos.left + targetWidth + this.arrowSize
      };
      
      // Check if any side is ok
      _.find(['top', 'bottom', 'left', 'right'], function(s){
        var side    = pos[s],
            checks  = side.checks = checkPosition(side);
        if (checks.ok) {
          pos.active = side;
          return true;
        }
      });
      
      // If no side is ok
      if (!pos.active) {
        
        // Check if the arrow fits at the top or bottom of the popup in left/right position
        var moreSpaceOnTop  = pos.top.checks.topOverflow <= pos.bottom.checks.bottomOverflow,
            moreSpaceOnLeft = pos.left.checks.leftOverflow <= pos.right.checks.rightOverflow,
            arrowFits = moreSpaceOnTop
              ? targetPos.top + targetHeight / 2 < bodyHeight - this.arrowSize - 24
              : targetPos.top + targetHeight / 2 > this.arrowSize + 37;
        
        // Arrow fits, so place the popup to the left or right of the target
        // and position the arrow correctly.
        if (arrowFits) {
          // Set active side
          pos.active = moreSpaceOnLeft ? pos.left : pos.right;
          
          // If both top and bottom are OK
          if (pos.active.checks.top && pos.active.checks.bottom) {}
          
          // If only top is OK, move the popup up
          else if (pos.active.checks.top) {
            pos.active.top -= pos.active.checks.bottomOverflow;
            pos.active.arrowTop = popupHeight / 2 + pos.active.checks.bottomOverflow;
          }
          
          // If only bottom is OK, move the popup down
          else if (pos.active.checks.bottom) {
            pos.active.top += pos.active.checks.topOverflow;
            pos.active.arrowTop = popupHeight / 2 - pos.active.checks.topOverflow;
          }
          
        }
        
        // Arrow cannot fit, so we place the popup at the top or bottom of the target
        // and position the arrow correctly.
        else {
          // Set active side
          pos.active = moreSpaceOnTop ? pos.top : pos.bottom;
          
          // If both left and right are OK
          if (pos.active.checks.left && pos.active.checks.right) {
          }
          
          // If only left is OK, move the popup left
          else if (pos.active.checks.left) {
            pos.active.left -= pos.active.checks.rightOverflow;
            pos.active.arrowLeft = popupWidth / 2 + pos.active.checks.rightOverflow;
          }
          
          // If only right is OK, move the popup right
          else if (pos.active.checks.right) {
            pos.active.left += pos.active.checks.leftOverflow;
            pos.active.arrowLeft = popupWidth / 2 - pos.active.checks.leftOverflow;
          }
          
        }
        
        
        // Correct for top / bottom overflow
        var heightCorrection = 0;
        pos.active.checks = checkPosition(pos.active);
        // Top overflows, so lower popup and shrink vertically
        if (!pos.active.checks.top) {
          pos.active.top += pos.active.checks.topOverflow;
          heightCorrection -= pos.active.checks.topOverflow;
          pos.active.arrowTop = (pos.active.arrowTop || 0) - pos.active.checks.topOverflow;
        }
        // Bottom overflows, so shrink somewhat
        if (!pos.active.checks.bottom)
          heightCorrection -= pos.active.checks.bottomOverflow;
        // Set height correction
        if (heightCorrection)
          this.$el.css('height', popupHeight + heightCorrection);
        
        // Placed left or right
        if (arrowFits)
          this.$arrow.css('top', targetPos.top + targetHeight / 2 - pos.active.top);
        // Placed top or bottom
        else
          this.$arrow.css('left', targetPos.left + targetWidth / 2 - pos.active.left);
        
      }
      
      // Set position and arrow class
      this.$el
        .css(pos.active)
        .removeClass('hidden top right bottom left').addClass(pos.active.arrow)
        .withOverlay({
          remove: function($overlay) {
            var $popup = this;
            $popup.addClass('animate hidden');
            _.delay(function(){
              $popup.remove().removeClass('animate hidden');
            }, 200);
          }
        });
      
    }
    
    
    
  });
  
  return PopupView;
  
});