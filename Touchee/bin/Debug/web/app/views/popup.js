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
    arrow:        true,
    arrowSize:    19,
    minMargin:    11,
    removeOnHide: false,
    
    
    // Constructor
    initialize: function(options) {
      options || (options = {});
      
      // this.$el.html(this.dummy);
      if (this.arrow) {
        this.$arrow = $('<div class="arrow" />');
        this.$el.prepend(this.$arrow);
      }

      this.render();
    },
    
    
    // Gets the required content height of this popup
    getRequiredContentHeight: function() {
      return this.$el.height() + 1;
    },
    
    
    // Resizes the popup nicely to fit the content size (given by getRequiredContentHeight)
    resizeToContents: function() {
      // Get some params
      var requiredContentHeight = this.getRequiredContentHeight() - 7,
          currentInnerHeight    = this.$el.height();
      
      // Do not resize if not required
      if (requiredContentHeight <= currentInnerHeight) return;
      
      // Get some more
      var currentOuterHeight  = this.$el.outerHeight(),
          currentTop          = this.$el.offset().top,
          paddingAndBorder    = currentOuterHeight - currentInnerHeight,
          targetHeight        = requiredContentHeight + paddingAndBorder,
          targetTop           = currentTop,
          bottomOverflow      = 0,
          topOverflow;
      
      // Unless the arrow is at the bottom, make sure the popup is not enlarged too much downward
      if (!this.$el.hasClass('bottom')) {
        bottomOverflow = currentTop + targetHeight - (document.body.offsetHeight - this.minMargin);
        if (bottomOverflow > 0)
          targetHeight -= bottomOverflow;
      }
      
      
      // If we have overflow at the bottom, or the arrow is at the bottom, enlarge the popup upwards
      if (bottomOverflow > 0 || this.$el.hasClass('bottom')) {
        
        // Unless the arrow is at the top, move the popup upwards
        if (!this.$el.hasClass('top')) {
          // If we have a bottom overflow, move by that overflow
          if (bottomOverflow > 0) {
            targetTop     -= bottomOverflow;
            targetHeight  += bottomOverflow;
          }
          // Else, move by the amount required
          else
            targetTop -= requiredContentHeight - currentInnerHeight;
        }
        
        // Check that the popup is not enlarged too much upward
        if ((topOverflow = this.minMargin - targetTop) > 0) {
          targetHeight  -= topOverflow;
          targetTop     += topOverflow;
        }
      }
      
      // Get arrow params
      var arrowTop  = this.$arrow.position().top,
          topDiff   = currentTop - targetTop;
      
      // Do animate
      var view = this;
      this.$arrow.css('top', arrowTop);
      _.defer(function(){
        view.$el.addClass('resize');
        view.$arrow.css('top', arrowTop + topDiff + 'px');
        view.$el.css({
          height: targetHeight  + 'px',
          top:    targetTop     + 'px'
        });
        _.delay(function(){
          view.$el.removeClass('resize');
        }, 200);
      });

      return this;
    },
    
    
    // Show the popup relative to the given DOM element target
    showRelativeTo: function(target) {
      var $target = $(target).first();
      target = $target[0];
      
      // Set on body
      this.$el.addClass('hidden');
      if (!this.el.parentNode)
        this.$el.appendTo(document.body);
      
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
        
        // Check if the arrow fits near the top or bottom in the left or right position
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
      var view = this, $popup = this.$el;
      this.$overlay = this.$el
        .css(pos.active)
        .css({height:this.$el.outerHeight()})
        .removeClass('hidden top right bottom left').addClass(pos.active.arrow)
        .withOverlay({ remove: _.bind(this.hide, this) });
      return this;
    },
    
    
    // Hides or removes the popup
    hide: function(options) {
      if (!this.$overlay) return;
      options = _.extend({trigger:true}, options || {});

      if (options.trigger) this.trigger('beforeHide');
      this.$el
        .addClass('animate hidden')
        .on('webkitTransitionEnd', _.bind(function(){

          this.$el.removeClass('animate hidden');
          var remove = _.isFunction(this.removeOnHide) ? this.removeOnHide.call(this) : this.removeOnHide === true;
          if (remove !== false)
            Backbone.View.prototype.remove.apply(this, arguments);
          else
            this.$el.hide();
          if (options.trigger) this.trigger('hide');
        }, this));

      this.$overlay.remove();
      delete this.$overlay;
      return this;
    }
    
    
  });
  
  return PopupView;
  
});