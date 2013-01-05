define([
  'jquery',
  'underscore',
  'Backbone',
  'Touchee'
], function($, _, Backbone, Touchee){
  
  var Log = Touchee.Log;
  
  var Communicator = {
    
    // Connect to the websocket and set callbacks
    connect: function(host, port) {
      var communicator = this;
      
      // Bail out if already connected
      if (this.isReady()) return;
      
      // Build address
      var address = "ws://" + host + ":" + port;
      
      // Open socket
      try {
        this._websocket = new WebSocket(address);
        Log.info("Communicator:: connecting to websocket at " + address + "...");
        communicator.trigger('connecting');
      }
      catch(err) {
        Log.error("Communicator:: unable to open websocket to " + address + " (" + err + ")");
        communicator.trigger('cannotConnect', err);
      }
      
      // Set callbacks
      var communicator = this;
      this._websocket.onopen = function(ev) {
        Log.info("Communicator:: websocket opened");
        communicator.trigger('connected');
      };
      this._websocket.onerror = function(ev) {
        Log.error("Communicator:: ERROR on websocket: " + ev.data);
        communicator.trigger('error', ev.data);
        alert("Error: what now? Websocket ready? " + communicator.isReady());
      };
      this._websocket.onclose = function(ev) {
        Log.warn("Communicator:: websocket closed");
        communicator.trigger('disconnected');
      };
      this._websocket.onmessage = function(ev) {
        Log.debug("Communicator:: received message over websocket:");
        
        // Convert to JS object if needed
        var response = ev.data;
        if (typeof response == "string")
          response = JSON.parse(response); // Much faster then $.parseJSON
        
        Log.debug(response);
        
        communicator.trigger('responseReceived', response);
      };
      
    },
    
    // Returns whether the websocket has established a connection
    isReady: function() {
      return this._websocket && this._websocket.readyState == 1;
    },
    
    // Closes the websocket
    close: function() {
      if (this.isReady())
        this._websocket.close();
    },
    
    // Send the given message over the websocket
    send: function(action, args) {
      Log.debug("Communicator:: sending message over websocket: " + action + " " + args);
      this._websocket.send(action + " " + args);
    }
    
  };
  
  // Enable events for this object
  _.extend(Communicator, Backbone.Events);
  
  return Communicator;
});