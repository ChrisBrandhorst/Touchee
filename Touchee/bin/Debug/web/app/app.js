define([
  'jquery',
  'underscore',
  'Backbone',

  'communicator',
  'router',
  'library',

  'models/server_info',
  'models/queue',

  'models/playback',
  'models/collections/media',
  'models/collections/devices',

  'views/browser/index'
], function($, _, Backbone,
            Communicator, Router, Library,
            ServerInfo, Queue,
            Playback, Media, Devices) {
  
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
      var firstTime = Communicator.connectedCount == 1;
      
      // If we have not connected before, get the sessionID from the cookie
      if (firstTime) {
        
        // Get session ID from cookie
        var sessionID = document.cookie.match(/ToucheeSession=([a-f0-9-]+)/);
        if (!sessionID)
          return T.Log.error("Please enable cookies");
        this.sessionID = sessionID[1];
        
      }
      
      // Link the websocket and HTTP session at the server
      Communicator.send("IDENTIFY", this.sessionID);
      
      // Init
      if (firstTime) Library.initialize();
      Devices.fetch();
      Playback.fetch();

      // When all containers are loaded
      Library.on('loaded:containers', function(){
        Queue.fetch();
        if (Communicator.connectedCount == 1) // TODO: weird, firstTime is always true here...
          Router.initialize();
      });

      // Wait until other Communicator connected callbacks have finished
      _.defer(_.bind(function(){
        Library.load(firstTime);
      }, this));
    },
    
    
    // Reconnect with a delay of 2000 ms
    reconnect: function() {
      _.delay(function(){
        App.connect();
      }, 5000);
    },
    
    
    // Called when a message was received over the websocket
    responseReceived: function(response) {
      var obj;

      // Revision update
      if (obj = response.revision) {
        var current = ServerInfo.get('revision'),
            next    = obj.revision;
        if (next > current + 1) {
          // TODO: RELOAD ALL
          console.warn('Skipped revision!');
        }
        else {
          // Upcoming data belongs to this new revision
        }
        ServerInfo.set('revision', next);
      }


      // The devices list has been updated
      if (obj = response.devices)
        Devices.set(obj);


      // A single device has been changed
      if (obj = response.device) {
        Devices.set(obj);
      }


      // The media list has been updated
      if (obj = response.media)
        Media.set(obj);


      // The containers of a medium have been changed
      if (obj = response.containers) {
        var medium = Media.get(obj.mediumID);
        if (medium)
          medium.containers.set(obj.items);
      }


      // A single container has been changed
      if (obj = response.container) {
        var mediumID    = obj.mediumID,
            medium      = Media.get(mediumID);
        if (medium)
          medium.containers.set(obj);
      }


      // The contents of a container has been changed
      if (obj = response.contentsChanged) {
        var mediumID    = obj.mediumID,
            containerID = obj.containerID,
            medium      = Media.get(mediumID);
        if (medium) {
          var container = medium.containers.get(containerID);
          if (container)
            container.notifyContentsChanged();
        }
      }


      // The queue has been changed
      if (obj = response.queue)
        Queue.reset( Queue.parse(obj) );


      // The playback status has changed
      if (obj = response.playback)
        Playback.set(obj);







      return;

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