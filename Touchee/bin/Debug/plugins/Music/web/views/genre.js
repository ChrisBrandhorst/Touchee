define([
  'jquery',
  'underscore',
  'Backbone',
  './_genre_tracks',
  'text!./_genre_header.html'
], function($, _, Backbone, GenreTracksView, genreHeaderTemplate) {  
  genreHeaderTemplate = _.template(genreHeaderTemplate);
  
  var GenreView = Backbone.View.extend({
    
    
    // Backbone View options
    tagName:      'section',
    className:    'genre',
    
    
    // Constructor
    initialize: function(options) {
      this.$header = $('<header/>').appendTo(this.$el);
      this.trackView = new GenreTracksView(options);
    },
    
    
    // 
    render: function() {
      this.$header.html(
        genreHeaderTemplate(this.model)
      );
      this.trackView.$el.appendTo(this.$el);
      this.trackView.render();
    }
    
    
  });
  
  return GenreView;
  
});