Communicator = new JS.Class({
  
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
    this._websocket.onopen = function(ev){
      Logger.info("Communicator:: websocket opened");
    };
    this._websocket.onerror = function(ev){
      Logger.error("Communicator:: ERROR on websocket: " + ev.data);
      alert("error: what now? Webspace ready? " + communicator.isReady());
    };
    this._websocket.onclose = function(ev){
      Logger.warn("Communicator:: websocket closed");
      spawn(communicator, 'disconnected', ev.data);
      spawn(2000, communicator, 'connect', host, port);
    };
    this._websocket.onmessage = function(ev){
      Logger.debug("Communicator:: received message over websocket");
      spawn(communicator, 'process', ev.data);
    };
    
  },
  
  // Returns whether the websocket has established a connection
  isReady: function() {
    return this._websocket && this._websocket.readyState == 1;
  },
  
  // Closes the webconnection
  close: function() {
    if (this.isReady())
      this._websocket.close();
  },
  
  // Called when we are disconnected from the server
  disconnected: function() {
    Application.disconnected();
  },
  
  // Request an object from the server
  get: function(url, options) {
    options = options || {};
    if (typeof options == 'function')
      options = {process:options};
    
    Logger.debug("Communicator:: getting " + url);
    var communicator = this;
    
    $.ajax({
      url:      Application.options.debug ? "debug/" + url + ".php" : url,
      success:  function(response) {
        communicator.process(response, options);
      },
      error:    function(jqXHR, textStatus, errorThrown) {
        Logger.error("Communicator:: request failed: " + errorThrown);
        if (typeof options.error == 'function')
          options.error(jqXHR, textStatus, errorThrown);
      }
    });
    
  },
  
  // Processes the given message
  process: function(response, options) {
    options = options || {};
    
    // Convert to JS object if needed
    if (typeof response == "string")
      response = JSON.parse(response); // Much faster then $.parseJSON
    
    Logger.debug("Communicator:: processing object below");
    Logger.debug(response);
    
    for (key in response) {
      if (key == 'containers')
        var i = 0;
    }
    
    // If a process function was given, execute that
    var doDefault = true;
    if (typeof options.process == 'function')
      options.process(response);
      
    // Else, give the message to the application for further processing
    else
      for (key in response) {
        if (typeof Application[key] == 'function')
          Application[key](response[key]);
      }
    
    // Do after processing if requested
    if (typeof options.afterProcess == 'function')
      options.afterProcess(response);
    
  }


});