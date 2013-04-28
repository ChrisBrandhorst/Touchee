define([
  'underscore',
  'Backbone',
  'models/contents_part'
], function(_, Backbone, ContentsPart){
  
  var Albums = ContentsPart.extend({
    
    
    // Filters the models from the contents collection.
    sieve: function(models) {
      
      // Group all tracks by album
      var groups = _.groupBy(models, function(track){
        return track.get('albumID');
      });
      
      // Get the first track from each group
      var tracks = _.map(groups, function(group){
        return group[0];
      });
      
      return tracks;
    },
    
    
    // 
    order: function(enumerator) {
      return enumerator
        .OrderBy("m => m.get('albumSort')")
        .ThenBy("m => m.get('albumArtistSort$')");
    }
    
    
  });
  
  return Albums;

});