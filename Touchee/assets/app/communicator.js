define([
  'jquery',
  'Underscore',
  'Backbone',
  'logger'
], function($, _, Backbone, Logger){
  
  var Communicator = {
    
    // Connect to the websocket and set callbacks
    connect: function(host, port) {
      
      // Bail out if already connected
      if (this.isReady()) return;
      
      // Build address
      var address = "ws://" + host + ":" + port;
      
      // Open socket
      try {
        Logger.info("Communicator:: connecting to websocket at " + address + "...");
        this._websocket = new WebSocket(address);
      }
      catch(err) {
        Logger.error("Communicator:: unable to open websocket to " + address + " (" + err + ")");
      }
      
      // Set callbacks
      var communicator = this;
      this._websocket.onopen = function(ev) {
        Logger.info("Communicator:: websocket opened");
        communicator.trigger('connected');
      };
      this._websocket.onerror = function(ev) {
        Logger.error("Communicator:: ERROR on websocket: " + ev.data);
        alert("Error: what now? Websocket ready? " + communicator.isReady());
      };
      this._websocket.onclose = function(ev) {
        Logger.warn("Communicator:: websocket closed");
        communicator.trigger('disconnected');
      };
      this._websocket.onmessage = function(ev) {
        Logger.debug("Communicator:: received message over websocket:");
        
        // Convert to JS object if needed
        var response = ev.data;
        if (typeof response == "string")
          response = JSON.parse(response); // Much faster then $.parseJSON
        
        Logger.debug(response);
        
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
      this._websocket.send(action + " " + args);
    }
    
  };
  
  // Enable events for this object
  _.extend(Communicator, Backbone.Events);
  
  return Communicator;
});