define([
  'jquery',
  'underscore',
  'Backbone',
  'communicator',
  'router',
  'library',
  'models/server_info',
  'models/collections/media',
  'views/browser'
], function($, _, Backbone,
            Communicator, Router, Library,
            ServerInfo, Media,
            BrowserView) {
  
  var App = {
    
    
    // Init the app
    initialize: function(options) {
      
      // Set events on the communicator
      // Communicator.on('connecting', this.connecting, this);
      Communicator.on('connected', this.connected, this);
      Communicator.on('cannotConnect disconnected', this.reconnect, this);
      Communicator.on('responseReceived', this.responseReceived, this);
      
      // Connect to the server
      this.connect();
      
    },
    
    
    // Connect to the server
    connect: function() {
      T.Log.info("Application:: connecting...")
      
      // Get server info
      T.Log.info("Application:: getting server info...");
      ServerInfo.fetch({
        success: function() {
          Communicator.connect(window.location.hostname, ServerInfo.get('websocketPort'));
        },
        error: function(model, jqXHR, options) {
          var err = "Application:: unable to get server info: " + jqXHR.statusText;
          T.Log.error(err);
          App.reconnect();
        }
      });
      
    },
    
    
    // Called when the websocket is (re-)opened
    connected: function() {
      var isFirstConnection = Communicator.connectedCount == 1;
      
      // If we have not connected before, get the sessionID from the cookie
      if (isFirstConnection) {
        
        // Get session ID from cookie
        var sessionID = document.cookie.match(/ToucheeSession=([a-f0-9-]+)/);
        if (!sessionID)
          return T.Log.error("Please enable cookies");
        this.sessionID = sessionID[1];
        
      }
      
      // Link the websocket and HTTP session
      Communicator.send("IDENTIFY", this.sessionID);
      
      // First-time init
      if (isFirstConnection) {
        Library.initialize();
        Router.initialize();
      }
      
      // Wait until other Communicator connected callbacks have finished
      _.defer(_.bind(function(){
        // (re)Load the library
        Library.load(this.wasConnected);
        
        // Set that we have connected one time
        this.wasConnected = true;
      }, this));
      
    },
    
    
    // Reconnect with a delay of 2000 ms
    reconnect: function() {
      _.delay(function(){
        App.connect();
      }, 2000);
    },
    
    
    // Called when a message was received over the websocket
    responseReceived: function(response) {
      
      
      return;
      
      if (response.media)
        Media.updateAll(response.media.items);
      
      if (response.containers) {
        var medium = Media.get(response.containers.mediumID);
        if (medium)
          medium.containers.updateAll(response.containers.items);
      }
      
      if (response.contents) {
        var container = Container.get(response.contents.containerID);
        if (container) {
          // var contents = container.findOrCreateContents(response.contents.contents);
        }
      }
      
      if (response.artwork) {
        var url   = response.artwork.url,
            imgs  = $('img[src="' + url + '"], [style$="' + url + ')"]'); // \\\'
        // imgs.attr('src', "data:image/png;base64," + response.artwork.data);
        imgs.each(function(){
          if (this.tagName.toLowerCase() == 'img') {
            this.setAttribute('src', "");
            this.setAttribute('src', url);
          }
          else {
            this.style.backgroundImage = "";
            this.style.backgroundImage = "url(" + url + ")";
          }
        });
      }
      
      
    }
    
  };
  
  
  return App;
});;