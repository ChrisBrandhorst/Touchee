define([
  'underscore',
  'Touchee',
  './module.js'
], function(_, Touchee, ListenLiveModule) {


  // TODO: in seperate file
  _.extend(T.T.items, {
    channel: {
      one:    'channel',
      more:   'channels'
    }
  });
  
  
  var ListenLivePlugin = Touchee.Plugin.extend({
    
    name:   "ListenLive.eu",
    module: new ListenLiveModule
    
  });
  
  return new ListenLivePlugin;
  
});