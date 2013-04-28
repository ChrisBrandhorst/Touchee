define([
  'jquery',
  'underscore',
  'Backbone',
  'models/artwork',
  'text!./album.html'
], function($, _, Backbone, Artwork, albumTemplate) {
  albumTemplate = _.template(albumTemplate);
  
  var AlbumDetailsView = Backbone.View.extend({
    
    
    events: {
      'tap li': 'clickedTrack'
    },
    

    // Constructor
    initialize: function(options) {
      var firstTrack = this.model.first();
      this
        .listenTo(this.model, 'reset', this.render)
        .listenTo(firstTrack, 'artwork', this.setArtwork)
        .listenTo(firstTrack, 'colors', this.setColors);
    },


    // When this view is removed, dispose of the model
    onRemove: function() {
      this.model.dispose();
    },
    
    
    // Render
    // TODO: not 100% when new render is smaller than old one (cover is hidden)
    render: function() {
      var artwork = Artwork.fromCache(this.model.first());
      this.$el.html(
        albumTemplate({
          artwork:  artwork,
          album:    this.model
        })
      );
      this.setArtwork(artwork);
      this.setColors(artwork && artwork.colors);
      Touchee.enableControlCluster(this, {});
      this.trigger('resize');
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
        this.$el.find('.sec').css('color', "rgb(" + (colors.foreground2 || colors.foreground) + ")");
        this.$el.find('h1 > div').toggleClass('light', !colors.backgroundIsLight);
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

      // Enqueue
      var idx = this.$('li').get().indexOf($li[0]);
      Touchee.Queue.resetAndPlay(this.model, {start:idx});
    }
    
    
    
  });
  
  return AlbumDetailsView;
  
});
