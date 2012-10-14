define([
  'lib/touchee.module',
  'Backbone'
], function(Module, Backbone){
  
  var MusicModule = Module.extend({
    
    
    // Only the genre view is inherited
    inheritedTypes: {
      genre:    {view: true},
      webcast:  {view: true}
    },
    
    
    // Use default set contents view unless we have an album
    setContentsView: function(containerView, itemView) {
      if (itemView.contents.getViewType() != "album_track")
        Module.prototype.setContentsView.apply(this, arguments);
    }
    
    
  });
  
  return new MusicModule();
  
});