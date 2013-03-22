define([
  'jquery',
  'underscore',
  'Backbone',
  './../models/view_models/artist_tracks',
  './_artist_tracks',
  'text!./_artist_header.html'
], function($, _, Backbone, ArtistTracks, ArtistTracksView, artistHeaderTemplate) {  
  artistHeaderTemplate = _.template(artistHeaderTemplate);
  
  var ArtistView = Backbone.View.extend({
    
    
    // Backbone View options
    tagName:      'section',
    className:    'artist',
    

    // Which model this view is supposed to show
    viewModel:    ArtistTracks,
    

    // Constructor
    initialize: function(options) {
      this.$header = $('<header/>').appendTo(this.$el);
      this.trackView = new ArtistTracksView(options);
    },
    
    
    // 
    render: function() {
      this.$header.html(
        artistHeaderTemplate(this.model)
      );
      this.trackView.$el.appendTo(this.$el);
      this.trackView.render();
    }
    
    
  });
  
  return ArtistView;
  
});