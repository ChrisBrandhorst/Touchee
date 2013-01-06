define([
  'jquery',
  'underscore',
  'Backbone',
  'Touchee',
  'communicator'
], function($, _, Backbone){
  
  var Communicator = {
    
    connectedCount: 0,
    
    // Connect to the websocket and set callbacks
    connect: function(host, port) {
      var communicator = this;
      
      // Bail out if already connected
      if (this.isConnected()) return;
      
      // Build address
      var address = "ws://" + host + ":" + port;
      
      // Open socket
      try {
        this._websocket = new WebSocket(address);
        T.Log.info("Communicator:: connecting to websocket at " + address + "...");
        communicator.trigger('connecting');
      }
      catch(err) {
        T.Log.error("Communicator:: unable to open websocket to " + address + " (" + err + ")");
        communicator.trigger('cannotConnect', err);
      }
      
      // Set callbacks
      var communicator = this;
      this._websocket.onopen = function(ev) {
        T.Log.info("Communicator:: websocket opened");
        communicator.connectedCount++;
        communicator.trigger('connected');
      };
      this._websocket.onerror = function(ev) {
        T.Log.error("Communicator:: ERROR on websocket: " + ev.data);
        communicator.trigger('error', ev.data);
        alert("Error: what now? Websocket ready? " + communicator.isConnected());
      };
      this._websocket.onclose = function(ev) {
        T.Log.warn("Communicator:: websocket closed");
        communicator.trigger('disconnected');
      };
      this._websocket.onmessage = function(ev) {
        T.Log.debug("Communicator:: received message over websocket:");
        
        // Convert to JS object if needed
        var response = ev.data;
        if (typeof response == "string")
          response = JSON.parse(response); // Much faster then $.parseJSON
        
        T.Log.debug(response);
        
        communicator.trigger('responseReceived', response);
      };
      
    },
    
    // Returns whether the websocket has established a connection
    isConnected: function() {
      return this._websocket && this._websocket.readyState == 1;
    },
    
    // Closes the websocket
    close: function() {
      if (this.isConnected())
        this._websocket.close();
    },
    
    // Send the given message over the websocket
    send: function(action, args) {
      T.Log.debug("Communicator:: sending message over websocket: " + action + " " + args);
      this._websocket.send(action + " " + args);
    }
    
  };
  
  // Enable events for this object
  _.extend(Communicator, Backbone.Events);
  
  return Communicator;
});