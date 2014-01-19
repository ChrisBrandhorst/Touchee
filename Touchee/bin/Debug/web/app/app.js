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

  'views/config/general'
], function($, _, Backbone,
            Communicator, Router, Library,
            ServerInfo, Queue,
            Playback, Media, Devices,
            GeneralConfigView) {
  
  var App = {


    // Init the app
    initialize: function(options) {
      
      // Set events on the communicator
      Communicator.on('connected', this.connected, this);
      Communicator.on('cannotConnect disconnected', this.reconnect, this);
      Communicator.on('responseReceived', this.responseReceived, this);
      
      // Set events on Media
      Media.on('sync:containers:all', this.containersSynced, this);

      // TODO: nettere plek?
      Touchee.Config.register({
        id:   'general',
        name: i18n.t('config.general'),
        view: 'views/config/general'
      });

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
      
      // If we have not connected before, get the session ID from the cookie
      if (Communicator.isFirstConnection()) {
        var sessionID = document.cookie.match(/ToucheeSession=([a-f0-9-]+)/);
        if (!sessionID)
          return T.Log.error("Please enable cookies");
        this.sessionID = sessionID[1];
      }
      
      // Link the websocket and HTTP session at the server
      Communicator.send("IDENTIFY", this.sessionID);
      
      // Init
      Devices.fetch();
      Playback.fetch();

      // Wait until other Communicator connected callbacks have finished
      _.defer(_.bind(function(){
        Media.fetch();
      }, this));
    },


    // Called when all containers have been synced
    containersSynced: function() {
      Queue.fetch();
      if (Communicator.isFirstConnection()) {
        Router.initialize();
        if (Media.length)
          Backbone.history.navigate(Media.first().url(), {trigger:true});
      }
      if (ServerInfo.changed.revision) {
        T.Log.warn('New revision: update existing containers');
      }
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
          medium.containers.set(obj.containers);
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


      // Plugin stuff
      if (obj = response.plugin) {
        for (pluginKey in obj) {
          var plugin = ServerInfo.plugins[pluginKey];
          if (plugin)
            plugin.responseReceived(obj[pluginKey]);
          else
            T.Log.error("Message received for unknown plugin '" + pluginKey + "'");
        }
      }








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