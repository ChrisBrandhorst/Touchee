define([
  'underscore',
  'Backbone',
  'models/item'
], function(_, Backbone, Item){
  
  var Track = Item.extend({
    

    // The artwork URL for this track (and thus, the album)
    artworkUrl: function(params) {
      return Touchee.getUrl(
        [this.collection.container.url(), "artwork/album", this.get('albumID')].join('/'),
        params
      );
    },

    
    // Computed properties
    computed: {
      duration:         function() { return String.duration(this.attributes.duration); },
      albumArtist:      function() { return this.attributes.albumArtist || this.attributes.artist; },
      albumArtistSort:  function() { return this.attributes.albumArtist ? this.attributes.albumArtistSort : this.attributes.artistSort; },
      displayLine1:     'title$',
      displayLine2:     function() { return this.get('artist$') + " &mdash; " + this.get('album$'); }
    }

  });

  return Track;
  
});