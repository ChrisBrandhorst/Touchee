define([
  'underscore',
  'Backbone',
  'models/item'
], function(_, Backbone, Item){
  
  var Track = Item.extend({
    

    // The artwork URL for this track (and thus, the album)
    artworkUrl: function(params) {
      return Touchee.buildUrl(
        this.getContainer().url(),
        "artwork/album",
        this.get('albumID'),
        params
      );
    },

    
    // Computed properties
    computed: {
      duration:         function() { return String.duration(this.get('duration')); },
      albumArtist:      function() { return this.get('albumArtist') || this.get('artist'); },
      albumArtistSort:  function() { return this.get('albumArtist') ? this.get('albumArtistSort') : this.get('artistSort'); },
      displayLine1:     'title$',
      displayLine2:     function() { return this.get('artist$') + " &mdash; " + this.get('album$'); }
    }

  });

  return Track;
  
});