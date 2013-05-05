define([
  'underscore',
  'Backbone'
], function(_, Backbone){
  

  var throwNotSupported = function(method, parameters, name) {
    throw(
      ["NotSupportedException: '", method, "' with parameters '", parameters, "' is not supported by device '", name, "'"].join("")
    );
  };


  var Device = Backbone.Model.extend({
    

    // Checks whether the device has the given capability
    can: function(capability) {
      return this.get('capabilities').indexOf(capability) > -1;
    },


    // Returns whether this device is the master volume device
    isMaster: function() {
      return this.get('type') == 'master';
    },




    // Action calls
    // ------------

    // Sets the device on or off, or toggles the device.
    // @param active    A boolean or undefined for toggling the state
    setActive: function(active) {
      // Set on/off
      if (this.can('onOff') && _.isBoolean(active))
        this._sendCommand('active', {value:active});
      // Toggle
      else if (this.can('toggle') && _.isUndefined(active))
        this._sendCommand('active');
      // Not supported
      else
        throwNotSupported('active', active, this.get('name'));
    },

    // Sets the muting of the device on or off, or toggles this.
    // @param muted    A boolean or undefined for toggling the muted state
    setMuted: function(muted) {
      // Set on/off
      if (this.can('muteOnOff') && _.isBoolean(muted))
        this._sendCommand('muted', {value:muted});
      // Toggle
      else if (this.can('muteToggle') && _.isUndefined(muted))
        this._sendCommand('muted');
      // Not supported
      else
        throwNotSupported('muted', muted, this.get('name'));
    },

    // Sets the volume of the device
    setVolume: function(volume) {
      if (this.can('volume') && _.isNumber(volume) && volume >= 0 && volume <= 100)
        this._sendCommand('volume', {value:volume});
      // Not supported
      else
        throwNotSupported('volume', volume, this.get('name'));
    },

    // Sets the LFE volume of the device
    setLFEVolume: function(lfeVolume) {
      if (this.can('lFEVolume') && _.isNumber(lfeVolume) && lfeVolume >= 0 && lfeVolume <= 2)
        this._sendCommand('lfe_volume', {value:lfeVolume});
      // Not supported
      else
        throwNotSupported('lfe_volume', lfeVolume, this.get('name'));
    },



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
  
  return Device;
  
});