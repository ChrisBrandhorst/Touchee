define([
  'underscore',
  'Backbone'
], function(_, Backbone){
  
  var Playback = Backbone.Model.extend({
    

    // URL
    url: "playback",
    

    // 
    initialize: function() {

      this.on('change:playing', this.setAutoIncrement, this);

    },


    setAutoIncrement: function() {

      if (this.get('playing')) {
        this.interval = setInterval(_.bind(function(){
          this.set('position', this.get('position') + 500)
        }, this), 500);
      }
      else {
        clearInterval(this.interval);
      }

    },



    // Action calls
    // ------------

    // 
    play: function() {
      this._sendCommand('play');
    },

    //
    pause: function() {
      this._sendCommand('pause');
    },

    // 
    setPosition: function(value) {
      this.set('position', value);
      this._setPosition();
    },

    _setPosition: _.debounce(function(){
      this._sendCommand('position', {value:this.get('position')});
    }, 10),
    


    // Internals
    // ---------

    // 
    _sendCommand: function(command, data) {
      return Backbone.ajax({
        url:  _.result(this, 'url') + "/" + command,
        type: 'PUT',
        data: data
      });
    }

    
  });


  return Touchee.Playback = new Playback;
  
});