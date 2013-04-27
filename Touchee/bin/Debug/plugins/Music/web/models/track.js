define([
  'underscore',
  'Backbone',
  'models/item'
], function(_, Backbone, Item){
  
  var Track = Item.extend({
    

    // The artwork URL for this track (and thus, the album)
    artworkUrl: function(params) {
      return Touchee.getUrl(
        [this.collection.container.url(), "artwork/id", this.id].join('/'),
        params
      );
    },
    
    
    // The string representation of the album. Needed for getting all unique albums
    // TODO: create an albumID
    getAlbumSelector: function() {
      return (this.get('album') || Touchee.nonAlphaSortValue) + "|" + (this.get('albumArtist') || this.get('artist') || Touchee.nonAlphaSortValue).toLowerCase();
    },
    
    
    // Computed properties
    computed: {
      duration:         function() { return String.duration(this.attributes.duration); },
      albumSelector:    function() { return this.getAlbumSelector(); },
      albumArtist:      function() { return this.attributes.albumArtist || this.attributes.artist; },
      albumArtistSort:  function() { return this.attributes.albumArtist ? this.attributes.albumArtistSort : this.attributes.artistSort; },
      displayLine1:     'title$',
      displayLine2:     function() { return this.get('artist$') + " &mdash; " + this.get('album$'); }
    }

  });

  return Track;
  
});