define([
  'Touchee',
  'modules/base/module',
  './models/contents/album',
  './models/contents/artist',
  './models/contents/genre',
  './models/contents/track',
  './models/contents/webcast'
], function(Touchee, BaseModule){
  
  var MusicModule = BaseModule.extend({
    
    
    initialize: function() {
    },
    
    // Get the Contents model for the given type
    getContentsModel: function(type) {
      return require('./models/contents/' + type);
    },
    
    
    
    // Only the genre view is inherited
    inheritedTypes: {
      genre:    {view: true},
      webcast:  {view: true}
    },
    
    
    // Use default set contents view unless we have an album
    setContentsView: function(containerView, itemView) {
      if (itemView.contents.getViewType() != "album_track")
        BaseModule.prototype.setContentsView.apply(this, arguments);
    }
    
    
  });
  
  return MusicModule;
  
});