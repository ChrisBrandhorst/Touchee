//    Touchee.js 0.0.1
//    (c) 2010-2012 Chris Brandhorst, ChronoworX

define([
  'underscore', 'jquery', 'Backbone',
  'views/popup/play_actions'
], function(_, $, Backbone, PlayActionsPopupView){
  
  
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
    START_EVENT:  touch ? 'touchstart'  : 'mousedown',
    MOVE_EVENT:   touch ? 'touchmove'   : 'mousemove',
    END_EVENT:    touch ? 'touchend'    : 'mouseup'
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
  _.extend(Backbone.Model.prototype,      LogExtension);
  _.extend(Backbone.Collection.prototype, LogExtension);
  _.extend(Backbone.View.prototype,       LogExtension);
  _.extend(Backbone.Router.prototype,     LogExtension);
  
  
  
  // Touchee.Plugin
  // -----------------
  
  // ...
  var Plugin = Touchee.Plugin = function() {
    
    // Inject CSS
    var cssFiles = _.isString(this.css) ? [this.css] : this.css, name = this.name;
    _.each(cssFiles, function(cssFile){
      var file = "app/plugins/" + name + "/assets/stylesheets/" + cssFile + ".css?_=" + new Date().getTime();
      $('head').append('<link rel="stylesheet" href="'+file+'" type="text/css" />');
    });

    // Inject locale
    // if (_.isObject(this.locale))
    //   $.extend(true, i18n, this.locale);

    // Init the plugin
    this.initialize.apply(this, arguments);
  };
  
  // Set up all inheritable **Touchee.Plugin** properties and methods.
  _.extend(Plugin.prototype, LogExtension, Backbone.Events, {
    
    
    // The name of the plugin
    name:   null,
    
    
    // An instance of the module provided by this plugin.
    module: null,
    
    
    // The locale object for this plugin
    locale: {},
    
    
    // The CSS files(s) for this plugin. All relative to app/plugins/:plugin/assets/stylesheets
    css:    [],
    
    
    // Initialize is an empty function by default. Override it with your own
    // initialization logic.
    initialize: function(){}
  });
  
  
  
  // Touchee.Params
  // -----------------
  
  // ...
  Touchee.Params = {
    
    // Parses an params string (with escaped values) to an object
    parse: function(str) {
      
      var parts = str.split('/');
      if (parts.length % 2 != 0)
        return {};
      
      var parsed = {};
      for (var i = 0; i < parts.length - 1; i += 2) {
        var val = decodeURIComponent(parts[i + 1]);
        parsed[ parts[i] ] = val == "" ? null : val;
      }
      
      return parsed;
    },
    
    // Ouputs an ordered, escaped string representation of the params
    compose: function(params) {
      return _.map(
        _.keys(params),//.sort(),
        function(key) {
          return key + "/" + encodeURIComponent(encodeURIComponent(params[key] || ""));
        }
      ).join('/');
    }
  };



  // Mobile.i18n
  // -----------------
  var i18n = Touchee.i18n = {


    // Registreer nieuwe set aan translations
    r: function(obj) {
      if (!_.isUndefined(obj.r) || !_.isUndefined(obj.t) || !_.isUndefined(obj.l))
        return Log.error("'r', 't' and 'l' cannot be used as top-level translation keys (reserved)");
      $.extend(true, i18n, obj);
    },


    // Translate functie
    t: function(item, options) {
      options || (options = {});
      
      var trans = _.getRef(this, item);
      
      if (_.isObject(trans) && _.isNumber(options.count))
        trans = trans[options.count == 1 ? 'one' : 'more'];
      if (!_.isString(trans))
        trans = item;
      
      return trans;
    },


    // Localize functie
    l: function(item, options) {
    }


  };
  root.i18n = i18n;



  // Touchee.CustomContainers
  // -----------------
  Touchee.CustomContainers = new Backbone.Collection();
  
  
  
  // Utils
  // -----------------
  
  Touchee.getUrl = function(base, params) {
    var url = base;
    if (params) {
      // params = encodeURI(Touchee.Params.compose(params));
      params = Touchee.Params.compose(params);
      if (params.length)
        url += (base.charAt(base.length - 1) == '/' ? '' : '/') + params;
    }
    return url;
  };

  var controlCluster = function(ev, options) {
    switch( $(ev.currentTarget).attr('data-button') ) {
      case 'play':
        Touchee.Queue.resetAndPlay(this.model);
        break;
      case 'shuffle':
        Touchee.Queue.resetAndPlay(this.model, {shuffle:true});
        break;
      case 'menu':
        PlayActionsPopupView.show(this.model, ev.target);
        break;
    }
  };
  Touchee.enableControlCluster = function(view, options) {
    view.$el.on('tap.delegateEvents' + view.cid, '.control_cluster > [data-button]', function(ev){
      controlCluster.call(view, ev, options);
    });
  };
  
  
  // Set up inheritance for the module and plugin.
  Plugin.extend = Backbone.Model.extend;
  
  
  // Configuration object
  Touchee.Config = new (Backbone.Model.extend({

    register: function(options) {
      Touchee.Config.sections.build(options, {add:true});
    },

    sections: new (Backbone.Collection.extend({

      url:    "config",
      model:  Backbone.Model.extend({
        initialize: function(attributes) {
          var view = this.get('view'), section = this;
          if (_.isString(view))
            require([view], function(v){ section.view = v; });
          else {
            this.unset('view');
            this.view = view;
          }
        }
      })

    }))

  }));
  
  
  // Handy constants
  Touchee.nonAlphaSortValue = "|";
  
  
  return Touchee;
  
});