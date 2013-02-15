define([
  'underscore',
  'Backbone',
  'models/params'
], function(_, Backbone, Params){
  
  var Track = Backbone.Model.extend({
    
    artworkUrl: function(params) {
      var parts = [this.collection.container.url(), "artwork/id", this.id];
      if (params) parts.push(new Params(params).toString());
      return parts.join('/');
    },
    
    
    getAlbumSelector: function() {
      return (this.get('album') || Touchee.nonAlphaSortValue) + "|" + (this.get('albumArtist') || this.get('artist') || Touchee.nonAlphaSortValue).toLowerCase();
    },
    
    
    getTracksOfAlbum: function() {
      var selector = this.getAlbumSelector(),
          tracks   = this.collection.filter(function(track){ return track.getAlbumSelector() == selector; });
      
      return Enumerable.From(tracks)
        .OrderBy("t => t.discNumber || Touchee.nonAlphaSortValue")
        .ThenBy("t => t.trackNumber || Touchee.nonAlphaSortValue")
        .ToArray();
    }
    
    
  });

  return Track;
  
});