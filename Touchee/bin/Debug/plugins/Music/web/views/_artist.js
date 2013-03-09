define([
  'jquery',
  'underscore',
  'Backbone',
  'text!./_artist.html'
], function($, _, Backbone, artistTemplate) {  
  artistTemplate = _.template(artistTemplate);
  
  var ArtistView = Backbone.View.extend({
    
    
    // Backbone View options
    tagName:      'section',
    className:    'artist',
    
    
    // Constructor
    initialize: function(options) {
      
    },
    
    
    // 
    render: function() {
      this.$el.html(
        artistTemplate(this.model)
      );
    }
    
    
  });
  
  return ArtistView;
  
});