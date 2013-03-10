define([
  'jquery',
  'underscore',
  'Backbone',
  './_artist_tracks',
  'text!./_artist_header.html'
], function($, _, Backbone, ArtistTracksView, artistHeaderTemplate) {  
  artistHeaderTemplate = _.template(artistHeaderTemplate);
  
  var ArtistView = Backbone.View.extend({
    
    
    // Backbone View options
    tagName:      'section',
    className:    'artist',
    
    
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