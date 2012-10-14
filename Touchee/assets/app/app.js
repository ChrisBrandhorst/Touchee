define([
  'jquery',
  'Underscore',
  'Backbone',
  'communicator',
  'router',
  'logger',
  'models/collections/media'
], function($, _, Backbone, Communicator, Router, Logger, Media) {
  
  
  $.extend(Touchee, {
    SUPPORTS_TOUCHES: 'ontouchstart' in document.documentElement
  });
  $.extend(Touchee, {
    START_EVENT:  Touchee.SUPPORTS_TOUCHES ? 'touchstart' : 'mousedown',
    MOVE_EVENT:   Touchee.SUPPORTS_TOUCHES ? 'touchmove'  : 'mousemove',
    END_EVENT:    Touchee.SUPPORTS_TOUCHES ? 'touchend'   : 'mouseup',
    knownContentTypes: ['music', 'video', 'pictures']
  });
  
  
  $.extend(Touchee, {
    
    $overlay: $('<div id="overlay" class="overlay" />').hide().appendTo(document.body),
    setOverlay: function($el, closeFunc, $overlay) {
      var $o = $overlay || T.$overlay;
      
      $o
        .css('z-index', Number($el.css('z-index')) - 1)
        .show()
        .bind(T.START_EVENT, function(ev){
          var $target = $(ev.target);
          if (!$target.parents().is($el)) {
            if (_.isFunction(closeFunc))
              closeFunc.apply(this, arguments);
            $o.hide().unbind(T.START_EVENT);
            $el.hide();
          }
          ev.preventDefault();
          return false;
        });
      
      $el.show();
    }
    
  });
  
  
  
  var App = {
    
    
    // Init the app
    initialize: function(options) {
      
      // Set log level
      Logger.level(options.logLevel);
      
      // Set events on the communicator
      if (T.config.debug)
        Router.initialize(T.config.debug);
      else {
        Communicator.on('connected', this.firstConnected);
        Communicator.on('connected', this.connected, this);
        Communicator.on('disconnected', this.disconnected, this);
        Communicator.on('responseReceived', this.responseReceived, this);
      }
      
      // Get configuration from server and, consequently, open the websocket
      this.getServerInfo();
    },
    
    
    // Gets the server configuration
    getServerInfo: function() {
      
      Logger.debug("Application:: getting server info...");
      
      $.ajax({
        url:      "server_info",
        success:  function(data) {
          var si = App.serverInfo = data;
          if (!T.config.debug)
            Communicator.connect(window.location.hostname, si.websocketPort);
        },
        error: function(jqXHR, textStatus, errorThrown) {
          var err = "Unable to get server info: " + errorThrown;
          Logger.error(err);
          _.delay(function(){
            alert("reconnect");
          }, 5000);
        }
        
      });
      
    },
    
    
    // Called when the websocket is (re-)opened
    connected: function() {
      // TODO: remove connecting indicator, enable UI input
    },
    
    
    // Called when the websocket is opened for the first time
    firstConnected: function() {
      // No more first connected callback
      Communicator.off('connected', this.firstConnected);
      
      // Get session ID from cookie
      var sessionID = document.cookie.match(/ToucheeSession=([a-f0-9-]+)/);
      if (!sessionID)
        return alert("Please enable cookies");
      sessionID = sessionID[1];
      
      // Link the websocket and HTTP session
      Communicator.send("IDENTIFY", sessionID);
      
      // Start the router
      Router.initialize();
    },
    
    
    // Called when the websocket is disconnected
    disconnected: function() {
      delete this.serverInfo;
      // TODO: set connecting indicator, block UI input
      console.error("disconnected");
      // this.getServerInfo();
    },
    
    
    // Called when a message was received over the websocket
    responseReceived: function(response) {
      
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
});