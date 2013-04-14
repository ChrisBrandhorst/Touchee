define([
  'underscore',
  './module.js'
], function(_, ListenLiveModule) {
  
  var ListenLivePlugin = Touchee.Plugin.extend({
    
    name:   "ListenLive.eu",
    module: new ListenLiveModule
    
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
