define([
  'jquery',
  'underscore',
  'Backbone',
  'models/artwork',
  'text!./_album_details.html'
], function($, _, Backbone, Artwork, albumDetailsTemplate) {
  albumDetailsTemplate = _.template(albumDetailsTemplate);
  
  var AlbumDetailsView = Backbone.View.extend({
    
    
    events: {
      'click li': 'clickedTrack'
    },
    
    
    // Constructor
    initialize: function(options) {
      this
        .listenTo(this.model.collection, 'reset change add remove', this.render)
        .listenTo(this.model, 'artwork', this.setArtwork)
        .listenTo(this.model, 'colors', this.setColors);
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
        this.colors = colors;
      }
      else {
        this.$el.css('backgroundColor', "");
        delete this.colors;
      }
      
    },
    
    
    // 
    clickedTrack: function(ev) {
      var $li = $(ev.target).closest('li'),
          id  = $li.attr('data-id');
      
      // Return if no id
      if (!id) return;
      
      // Set click animation
      var bgColor = this.colors && this.colors.foreground2;
      $li.css('background-color', bgColor ? "rgb(" + bgColor + ")" : "").addClass('clicked');
      _.defer(function(){ $li.removeClass('clicked').css('background-color', ""); });
    }
    
    
    
  });
  
  return AlbumDetailsView;
  
});
