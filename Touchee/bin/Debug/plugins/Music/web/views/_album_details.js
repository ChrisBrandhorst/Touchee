define([
  'jquery',
  'underscore',
  'Backbone',
  'models/artwork',
  'text!./_album_details.html'
], function($, _, Backbone, Artwork, albumDetailsTemplate) {
  albumDetailsTemplate = _.template(albumDetailsTemplate);
  
  var AlbumDetailsView = Backbone.View.extend({
    
    // Constructor
    initialize: function(options) {
      this.model.on('colors', this.setColors, this);
    },
    
    
    // Render
    render: function() {
      var artwork = Artwork.fromCache(this.model);
      this.$el.html(
        albumDetailsTemplate({
          artwork:  artwork,
          tracks:   this.model.getTracksOfAlbum()
        })
      );
      this.setColors(artwork && artwork.colors);
      return this;
    },
    
    
    // 
    setColors: function(colors) {
      
      var $parent = this.$el.parent();
      
      if (colors) {
        $parent.css('backgroundColor', "rgb(" + colors.background + ")");
        this.$el.find('.prim').css('color', "rgb(" + colors.foreground + ")");
        this.$el.find('.sec').css('color', "rgb(" + colors.foreground2 + ")");
      }
      else {
        $parent.css('backgroundColor', "");
      }
      
    }
    
  });
  
  return AlbumDetailsView;
  
});
