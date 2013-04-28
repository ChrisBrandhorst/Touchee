define([
  'underscore',
  'Backbone',
  'models/contents_part'
], function(_, Backbone, ContentsPart){
  
  var Artists = ContentsPart.extend({
    
    
    // Filters the models from the contents collection.
    sieve: function(models) {
      
      // Group all tracks by artist
      var groups = _.groupBy(models, function(track){
        var artist = track.get('artist');
        return artist ? artist.toLowerCase() : null;
      });
      
      // Get the first track from each group
      var artists = _.map(groups, function(group){
        return group[0];
      });
      
      return artists;
    },
    
    
    // Sort by artist sort value
    order: function(enumerator) {
      return enumerator
        .OrderBy("m => m.get('artistSort')");
    },
    
    
    // Get the artist URL for the given track
    getUrlFor: function(track) {
      return this.url({
        artist: track.get('artist')
      });
    }
    
    
  });
  
  return Artists;

});