define([
  'underscore',
  'Backbone',
  'models/contents_part'
], function(_, Backbone, ContentsPart){
  
  var Album = ContentsPart.extend({
    
    // Constructor
    initialize: function(models, options) {
      this.track = options.track;
      options.params = {
        view:   'album',
        album:  this.track.id
      };
      ContentsPart.prototype.initialize.apply(this, arguments);
    },

    
    // Filters the models from the contents collection.
    sieve: function(models) {
      var selector = this.track.getAlbumSelector();
      return _.filter(models, function(track){ return track.getAlbumSelector() == selector; });
    },
    
    
    // 
    order: function(enumerator) {
      return enumerator
        .OrderBy("t => t.get('discNumber') || Touchee.nonAlphaSortValue")
        .ThenBy("t => t.get('trackNumber') || Touchee.nonAlphaSortValue")
        .ThenBy("t => t.get('titleSort')")
        .ToArray();
    }

    
  });
  
  return Album;

});