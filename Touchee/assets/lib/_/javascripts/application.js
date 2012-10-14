$.ajaxSetup({
  dataType: 'json',
  cache:    false
});


App = Application = new JS.Singleton({
  
  
  // Default options for the application
  defaultOptions: {
    debug:    false,
    logLevel: Logger.Levels.ERROR
  },
  
  
  // If we are on windows or not
  windows:    navigator.platform.match(/win/i),
  
  
  // Start
  start: function(options) {
    
    // Set options
    this.options = $.extend(this.defaultOptions, options || {});
    
    // Set that we are starting
    this.starting = true;
    
    // Set log level
    Logger.level(this.options.logLevel);
    
    // Build communicator
    this.c = this.communicator = new Communicator();
    
    // Get configuration from server
    Logger.debug("Application:: getting server info...");
    
    // Connect to the erver
    this.connect();
  },
  
  
  // Connects the application to the server
  connect: function() {
    
    // Get the server info
    this.communicator.get("server_info", {
      error: function(jqXHR, textStatus, errorThrown) {
        var err = "Unable to get server info: " + errorThrown;
        Logger.error(err);
        setTimeout(function(){
          Application.connect();
        }, 5000);
      }
    });
    
  },
  
  
  // Called when the websocket is disconnected
  disconnected: function() {
    // TODO: reset interface
    Application.connect();
  },
  
  
  //
  serverInfo: function(serverInfo) {
    
    // Connect to websocket
    this.communicator.connect(serverInfo.hostname, serverInfo.websocketPort);
    
    // TODO: Set devices
    // TODO: Time settings
    
    // TODO: Get media
    this.getMedia();
  },
  
  
  // Get all media from the server
  getMedia: function() {
    this.communicator.get("media");
  },
  // Media were received
  media: function(media) {
    Navigation.setMedia(media.items);
  },
  
  
  // Get all containers for the given medium
  getContainers: function(medium) {
    this.communicator.get("media/" + medium.id + "/containers");
  },
  // Containers were received
  containers: function(containers) {
    Navigation.setContainers(containers);
  }
  
  
});