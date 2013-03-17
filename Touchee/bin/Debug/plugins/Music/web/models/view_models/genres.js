define([
  'underscore',
  'Backbone',
  'models/contents_part'
], function(_, Backbone, ContentsPart){
  
  var Genres = ContentsPart.extend({
    
    
    // Filters the models from the contents collection.
    sieve: function(models) {
      
      // Group all tracks by artist
      var groups = _.groupBy(models, function(track){
        var genre = track.get('genre');
        return genre ? genre.toLowerCase() : null;
      });
      
      // Get the first track from each group
      var genres = _.map(groups, function(group){
        return group[0];
      });
      
      return genres;
    },
    
    
    // Sort by genre sort value
    order: function(enumerator) {
      return enumerator
        .OrderBy("m => m.get('genreSort')");
    },
    
    
    // Get the genre URL for the given track
    getUrl: function(track) {
      return this.url({
        genre: track.get('genre')
      });
    }
    
    
  });
  
  return Genres;

});