define([
  'underscore',
  './contents_module.js'
], function(_, ListenLiveContentsModule) {
  
  var ListenLivePlugin = Touchee.Plugin.extend({
    
    name:   "ListenLive.eu",
    module: new ListenLiveContentsModule
    
  });
  
  return new ListenLivePlugin;
  
});









// TODO: in seperate file
// _.extend(T.T.items, {
//   channel: {
//     one:    'channel',
//     more:   'channels'
//   }
// });
