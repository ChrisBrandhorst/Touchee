define([
  'underscore',
  'Backbone',
  'models/contents_part'
], function(_, Backbone, ContentsPart){
  
  var GroupedTracks = ContentsPart.extend({


    // Constructor
    initialize: function(models, options) {
      ContentsPart.prototype.initialize.apply(this, arguments);
      if (options.groupByAttr) this.groupByAttr = options.groupByAttr;
    },


    // Get all tracks for the groupby attribute
    sieve: function(models) {
      var attr  = this.groupByAttr,
          val   = this.params[attr] ? this.params[attr].toLowerCase() : null;
      return _.filter(models, function(track){
        var v = track.get(attr);
        return v == null ? val == null : v.toLowerCase() == val;
      });
    },


    // Order the tracks
    order: function(enumerator) {
      return enumerator
        .OrderBy("m => m.get('albumArtistSort')")
        .ThenBy("m => m.get('albumSort')")
        .ThenBy("m => m.get('discNumber')")
        .ThenBy("m => m.get('trackNumber')");
    },


    // Gets the number of albums in this collection
    getAlbumCount: function() {
      return _.size(
        this.groupBy(function(track){
          return track.getAlbumSelector();
        })
      );
    }


  });
  
  return GroupedTracks;

});