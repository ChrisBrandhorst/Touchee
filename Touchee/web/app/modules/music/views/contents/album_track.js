define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/base',
  'text!modules/music/views/contents/album_track.html'
], function($, _, Backbone, ContentsBase, albumPopupTemplate) {
  albumPopupTemplate = _.template(albumPopupTemplate);
  
  var AlbumPopup = ContentsBase.extend({
    
    
    // View classname
    className: 'album_track',
    
    
    // Events
    events: {
      'click tr': 'clickedRow'
    },
    
    
    // 
    initialize: function() {
      ContentsBase.prototype.initialize.apply(this, arguments);
      
      // Set overlay
      this.$overlay = $('<div class="overlay album_track_overlay" />');
      
      // Do render
      this.render();
    },
    
    
    // 
    render: function() {
      var $album = this.$el;
      
      // Get the album thumb that was clicked
      var $thumb = this.$thumb;
      if (!$thumb)
        $thumb = this.$thumb = $('.contents:not(.hidden) section.type-album li[data-id=' + this.contents.filter.get('albumid') + ']').first();
      if (!$thumb.length)
        return;
      else
        this.backgroundImage = $thumb[0].childNodes[0].style.backgroundImage;
      
      // 
      var $spans = $thumb.children();
      this.contents.set({
        album:        $spans.eq(0).text(),
        albumArtist:  $spans.eq(1).text()
      });
      
      // Render contents
      $album.html(
        albumPopupTemplate(
          _.extend({
            backgroundImage: this.backgroundImage
          }, this.contents)
        )
      );
      
      // Set touch scroll select
      $album.find('.scrollable').touchscrollselect({
        selectable:     'tr',
        selectedClass:  'selected',
        keepSelection:  false
      });
      
      // Already placed, bail out
      if (this.el.parentNode) return;
      
      // Get some values
      var $scrollable = $thumb.closest('.scrollable'),
          thumbPos    = $thumb.position();
          thumbMargin = {
            top:  $thumb.css('margin-top').numberValue(),
            left: $thumb.css('margin-left').numberValue()
          };
      
      // Modify thumbpos, taking into account the margins
      thumbPos    = {
        top:  thumbPos.top + thumbMargin.top,
        left: thumbPos.left + thumbMargin.left
      };
      
      // Place the overlay on the page
      this.$overlay.appendTo( $scrollable.closest('.contents > section') );
      
      // Place the album on the same place as the thumbnail
      $album.appendTo(this.$overlay).css({
        top:  thumbPos.top,
        left: thumbPos.left
      });
      
      // Calculate at what position the album will be shown
      var albumPos = {
        top:  thumbPos.top + $album.css('margin-top').numberValue(),
        left: thumbPos.left + $album.css('margin-left').numberValue()
      };
      
      // Get left margin of ul containing thumb
      var ulMarginFix = $thumb.parent().css('margin-left').numberValue();
      
      // Calculate deltas
      var deltaX = Math.min(
        $scrollable[0].offsetWidth - thumbMargin.left - this.el.offsetWidth - ulMarginFix,
        Math.max(thumbMargin.left, albumPos.left) + ulMarginFix
      ) - albumPos.left;
      var deltaY = Math.min(
        $scrollable[0].offsetHeight - thumbMargin.top - this.el.offsetHeight,
        Math.max(thumbMargin.top, albumPos.top)
      ) - albumPos.top;
      
      // Hide thumbnail
      $thumb.addClass('hidden');
      
      // Animate!
      _.defer(function(){
        $album
          .addClass('flip')
          .css('-webkit-transform', "translate(" + deltaX + "px," + deltaY + "px)");
      });
      
      // Set overlay
      var view = this;
      this.$overlay.bind(T.START_EVENT, function(ev){
        
        // If we are clicking outside of the album
        if (!$(ev.target).parents().is($album)) {
          
          // Animate the album back to thumb
          $album
            .removeClass('flip')
            .css('-webkit-transform', '');
          
          // After animation is complete, remove this view
          _.delay(function(){
            $thumb.removeClass('hidden');
            _.delay(function(){
              view.$overlay.remove();
              view.remove();
            }, 100);
          }, parseFloat($album.css('-webkit-transition-duration')) * 1000);
          
          ev.preventDefault();
          return false;
        }
        
      });
      
    },
    
    
    //
    update: function() {
      this.render();
    },
    
    
    // Clicked on a row in the table
    clickedRow: function(ev) {
      var id = $(ev.target).closest('tr').attr('data-' + this.contents.idAttribute);
      if (typeof id != 'undefined')
        Backbone.history.navigate(this.contents.getUrl(id), {trigger:true});
    }
    
    
  });
  
  return AlbumPopup;
  
});