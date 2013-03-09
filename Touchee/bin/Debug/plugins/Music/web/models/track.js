define([
  'underscore',
  'Backbone',
  'Touchee'
], function(_, Backbone, Touchee){
  
  var Track = Backbone.Model.extend({
    
    artworkUrl: function(params) {
      return Touchee.getUrl(
        [this.collection.container.url(), "artwork/id", this.id].join('/'),
        params
      );
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