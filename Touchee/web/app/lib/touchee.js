//    Touchee.js 0.0.1
//    (c) 2010-2012 Chris Brandhorst, ChronoworX

define([
  'underscore', 'jquery', 'Backbone'
], function(_, $, Backbone){
  
  
  // Initial Setup
  // -------------
  
  // Save a reference to the global object (`window` in the browser, `global`
  // on the server).
  var root = this;
  
  // Save the previous value of the `Touchee` variable, so that it can be
  // restored later on, if `noConflict` is used.
  var previousTouchee = root.Touchee;
  
  // The top-level namespace. All public Touchee classes and modules will
  // be attached to this. Exported for both CommonJS and the browser.
  var Touchee;
  if (typeof exports !== 'undefined') {
    Touchee = exports;
  } else {
    Touchee = root.Touchee = {};
  }
  
  // Current version of the library.
  Touchee.VERSION = '0.0.1';
  
  // Runs Touchee.js in *noConflict* mode, returning the `Touchee` variable
  // to its previous owner. Returns a reference to this Touchee object.
  Touchee.noConflict = function() {
    root.Touchee = previousTouchee;
    return this;
  };
  
  // Whether we are runnning on a touch enabled device
  var touch = Touchee.touch = 'ontouchstart' in document.documentElement;
  
  // Interaction events are dependent on the touch capabilities of the device
  _.extend(Touchee, {
    START_EVENT:  touch ? 'touchstart' : 'mousedown',
    MOVE_EVENT:   touch ? 'touchmove'  : 'mousemove',
    END_EVENT:    touch ? 'touchend'   : 'mouseup'
  });
  
  
  // Touchee.Overlay
  // -----------------
  
  // Overlay functionality
  var Overlay = Touchee.Overlay = {
    
    // The overlay element
    $el:  $('<div id="overlay" class="overlay" />').hide().appendTo(document.body),
    
    // Sets the (optionally custom) overlay below the given element.
    // Accepts a close function to be called when the overlay is closed.
    set: function($el, closeFunc, $overlay) {
      var $o = $overlay || Overlay.$el;
      
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
    
  };
  
  
  // Touchee.Log
  // -----------------
  
  // Provides logging functionality
  var Log = Touchee.Log = {
    
    // Log levels
    Levels: {
      NONE:   0,
      ERROR:  1,
      WARN:   2,
      INFO:   3,
      DEBUG:  4
    },
    
    // Get or set the current level
    level: function(value) {
      if (value) this._level = value;
      return this._level || this.Levels.error;
    },
    
    // Log functions
    error: function(message) { console && console.error && console.error(message); },
    warn: function(message) { if (this.level() >= this.Levels.WARN) console && console.warn && console.warn(message); },
    info: function(message) { if (this.level() >= this.Levels.INFO) console && console.info && console.info(message); },
    debug: function(message) { if (this.level() >= this.Levels.DEBUG) console && console.debug && console.debug(message); }
    
  };
  var LogExtension = { Log: Log };
  
  // Set up logging for Backbone models, collections and views
  _.extend(Backbone.Model.prototype, LogExtension);
  _.extend(Backbone.Collection.prototype, LogExtension);
  _.extend(Backbone.View.prototype, LogExtension);
  _.extend(Backbone.Router.prototype, LogExtension);
  
  
  // Touchee.ContentsModule
  // -----------------
  
  // Modules receive requests for content pages from the user interface and process them
  // specificly for the module type
  var ContentsModule = Touchee.ContentsModule = function() {
    this.initialize.apply(this, arguments);
  };
  
  
  // Set up all inheritable **Touchee.ContentsModule** properties and methods.
  _.extend(ContentsModule.prototype, LogExtension, Backbone.Events, {
    
    
    // Defines which content / view types should be inherited from other modules
    // - { track: true }                        both model and view generic
    // - { genre: {model: true} }               model generic, view custom
    // - { genre: {model:'music'} }             model from music module, view custom
    // - { genre: {model:'music', view: true} } model from music module, view generic
    inheritedTypes: {},
    
    
    // Initialize is an empty function by default. Override it with your own
    // initialization logic.
    initialize: function(){},
    
    
    // Gets the model this module uses for displaying the given container
    // with the given type.
    getContentModelPath: function(container, type) {
      var opts = this.inheritedTypes[type];
      var module = this.name;
      
      if (opts) {
        if (opts === true || opts.model === true)
          module = false;
        else if (typeof opts.model == 'string')
          module = opts.model;
      }
      
      return module
        ? ('modules/' + module + '/models/contents/' + type)
        : 'models/contents';
    },
    
    
    // Gets the view this module uses for displaying the given contents.
    getContentViewPath: function(contents) {
      var type      = contents.getViewType(),
          opts      = this.inheritedTypes[type];
          var module = this.name;
      
      if (opts) {
        if (opts === true || opts.view === true)
          module = false;
        else if (typeof opts.view == 'string')
          module = opts.view;
      }
      
      return module
        ? ('modules/' + module + '/views/contents/' + type)
        : 'views/contents/table_base';
    },
    
    
    // Default setContentPage
    setContentPage: function(containerView, type, filter) {
      var containerIsEmpty  = containerView.isEmpty();
      
      // If there are no pages yet
      // if (containerIsEmpty && filter || !containerIsEmpty && !filter)
      //   return console.error('ShouldNotHappenException');
      
      // Get the container from the view
      var container         = containerView.container,
          contentModelPath  = this.getContentModelPath(container, type);
          _this             = this;
      
      // No content model path? Something went wrong
      if (!contentModelPath)
        return console.error('ShouldNotHappenException');
      
      // Create the contents
      require([contentModelPath], function(Contents){
        
        var contents = new Contents({
          container:  container,
          type:       type,
          filter:     filter
        });
        
        // Get and check content view path
        var contentViewPath = _this.getContentViewPath(contents);
        if (!contentViewPath)
          return console.error('ShouldNotHappenException');
        
        // Get the corresponding view
        require([contentViewPath], function(ContentsItemView){
          
          // Init the view
          var itemView = new ContentsItemView({
            contents:     contents,
            back:         containerIsEmpty ? false : containerView.activePage.contents.getTitle(),
            fragment:     Backbone.history.fragment
          });
          
          // Set the view
          _this.setContentsView(containerView, itemView);
          
          // Load the content
          contents.fetch();
          
        });
        
      });
      
    },
    
    
    // Default setContentsView
    setContentsView: function(containerView, itemView) {
      itemView.render();
      containerView.storePage(itemView.contents.filter.toString(), itemView);
      containerView.activate(itemView);
    }
    
  });
  
  
  // Touchee.Plugin
  // -----------------
  
  // ...
  var Plugin = Touchee.Plugin = function() {
    this.initialize.apply(this, arguments);
  };
  
  // Set up all inheritable **Touchee.Plugin** properties and methods.
  _.extend(Plugin.prototype, LogExtension, Backbone.Events, {
    
    
    // Initialize is an empty function by default. Override it with your own
    // initialization logic.
    initialize: function(){}
    
  });
  
  
  
  // Set up inheritance for the contentsModule and plugin.
  ContentsModule.extend = Plugin.extend = Backbone.Model.extend;
  
  
  // Configuration object
  Touchee.Config = new Backbone.Model();
  
  
  return Touchee;
  
});