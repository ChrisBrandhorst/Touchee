define([
  'underscore',
  'Backbone',
  'models/contents_part'
], function(_, Backbone, ContentsPart){
  
  var Albums = ContentsPart.extend({
    
    
    // Filters the models from the contents collection.
    sieve: function(models) {
      
      // Group all tracks by album
      var groups = _.groupBy(models, function(model){
        return (model.get('album') || Touchee.nonAlphaSortValue) + "|" + (model.get('albumArtist') || model.get('artist') || Touchee.nonAlphaSortValue);
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
        .OrderBy("m => m.attributes.albumArtistSort")
        .ThenBy("m => m.attributes.albumSort");
    }
    
  });
  
  return Albums;

});