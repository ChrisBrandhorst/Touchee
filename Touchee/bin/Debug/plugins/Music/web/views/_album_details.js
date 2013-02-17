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
      this.model.on('artwork', this.setArtwork, this);
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
      this.setArtwork(artwork);
      this.setColors(artwork && artwork.colors);
      return this;
    },
    
    
    // 
    setArtwork: function(artwork) {
      if (artwork && artwork.exists() === true)
        this.$('.artwork').html('<img src="' + artwork.url({largest:true}) + '" />');
    },
    
    
    // 
    setColors: function(colors) {
      
      if (colors) {
        this.$el
          .css('backgroundColor', "rgb(" + colors.background + ")")
          .find('.prim').css('color', "rgb(" + colors.foreground + ")");
        this.$el.find('.sec').css('color', "rgb(" + colors.foreground2 + ")");
      }
      else {
        this.$el.css('backgroundColor', "");
      }
      
    }
    
  });
  
  return AlbumDetailsView;
  
});
