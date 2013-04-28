define([
  'underscore',
  'Backbone',
  'models/contents_part'
], function(_, Backbone, ContentsPart){
  
  var Album = ContentsPart.extend({
    
    // Constructor
    initialize: function(models, options) {
      this.albumID = options.track.get('albumID');
      options.params = {
        view:   'album',
        album:  this.albumID
      };
      ContentsPart.prototype.initialize.apply(this, arguments);
    },

    
    // Filters the models from the contents collection.
    sieve: function(models) {
      var albumID = this.albumID;
      return _.filter(models, function(track){ return track.get('albumID') == albumID; });
    },
    
    
    // 
    order: function(enumerator) {
      return enumerator
        .OrderBy("t => t.get('discNumber') || Touchee.nonAlphaSortValue")
        .ThenBy("t => t.get('trackNumber') || Touchee.nonAlphaSortValue")
        .ThenBy("t => t.get('titleSort')");
    }

    
  });
  
  return Album;

});