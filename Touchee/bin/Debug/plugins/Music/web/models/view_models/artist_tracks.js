define([
  'underscore',
  'Backbone',
  'models/contents_part'
], function(_, Backbone, ContentsPart){
  
  var ArtistTracks = ContentsPart.extend({
    
    
    // Get all tracks for the current artist
    sieve: function(models) {
      var artist = this.params.artist.toLowerCase();
      return _.filter(models, function(track){
        var a = track.get('artist');
        return a == null ? artist == null : a.toLowerCase() == artist;
      });
    },
    
    
    // 
    order: function(enumerator) {
      return enumerator
        .OrderBy("m => m.attributes.albumSort")
        .ThenBy("m => m.attributes.discNumber")
        .ThenBy("m => m.attributes.trackNumber");
    }
    
    
  });
  
  return ArtistTracks;

});