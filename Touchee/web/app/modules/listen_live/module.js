define([
  'underscore',
  'lib/touchee.module'
], function(_, Module){
  
  
  // TODO: in seperate file
  _.extend(Touchee.T.items, {
    channel: {
      one:    'channel',
      more:   'channels'
    }
  });
  
  
  var ListenLiveModule = Module.extend({
    
    // 
    inheritedTypes: {
      genre:    {view: true}
    }
    
  });
  
  return new ListenLiveModule();
  
});