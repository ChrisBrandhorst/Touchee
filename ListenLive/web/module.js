define([
  'underscore',
  'modules/music/module',
  './models/channel.js',
  './models/genre.js',
  './views/channel.js'
], function(_, MusicModule, Channel, Genre, ChannelView) {
  
  var ListenLiveModule = MusicModule.extend({
    
    // 
    getContentsModel: function(type) {
      switch (type) {
        case 'channel': return Channel;
        case 'genre':   return Genre;
        default:        return MusicModule.prototype.getContentsModel.apply(this, arguments);
      }
    },
    
    
    // 
    getContentsView: function(type, contents) {
      return type == 'channel' ? ChannelView : MusicModule.prototype.getContentsView.apply(this, arguments);
    },
    
  });
  
  return ListenLiveModule;

});