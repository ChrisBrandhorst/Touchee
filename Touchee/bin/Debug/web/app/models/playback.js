define([
  'underscore',
  'Backbone',
  'models/collections/media'
], function(_, Backbone, Media){
  
  
  var Playback = Backbone.Model.extend({
    
    url: "playback",
    
    parse: function(response) {
      return response;
    },

    play: function() { PlaybackCommand.execute('play'); },
    pause: function() { PlaybackCommand.execute('pause'); },

    masterVolume: function(level) { PlaybackCommand.execute('volume', {level:level}); },
    masterMute: function(muted) { PlaybackCommand.execute('mute', {muted:muted}); }
    
  });



  var PlaybackCommand = Backbone.Model.extend({

    url: function() { return "playback"; },

    save: function(attributes, options) {
      options || (options = {});
      options.wait = true;
      options.url = [this.url(), options.action].join('/');
      Backbone.Model.prototype.save.call(this, attributes, options);
    }

  }, {

    execute: function(action, attributes) {
      new PlaybackCommand().save(attributes || {}, {action:action});
    }

  });



  return Touchee.Playback = new Playback;
  
});