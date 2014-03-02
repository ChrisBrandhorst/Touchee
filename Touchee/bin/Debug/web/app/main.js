// Require.js allows us to configure shortcut alias
// There usage will become more apparent futher along in the tutorial.
require.config({
  
  urlArgs:      "_=" + (new Date()).getTime(),
  waitSeconds:  1,
  
  paths: {
    underscore: 'lib/underscore.amd',
    Backbone:   'lib/backbone.amd',
    Touchee:    'lib/touchee',
    text:       '../lib/text-2.0.6',
    i18n:       'lib/i18n-2.0.4.mod',
    jquery:     '../lib/jquery'
  },
  
  shim: {
    'Backbone':                           ['underscore'],
    'lib/jquery.extensions':              ['jquery'],
    'lib/jquery.touchscrollselect':       ['jquery'],
    '../lib/jquery.hammer':               ['jquery'],
    '../lib/jquery-ui-1.10.4.custom.min': ['jquery'],
    '../lib/jquery-ui.touch-punch-0.2.3': ['jquery']
  }
  
});


// Load all vendor files first
require([
  
  // Main libraries
  'Backbone',
  'jquery',
  
  // Config file
  'text!config.js',

  // Backbone extensions
  'Touchee',
  'lib/backbone.extensions',
  
  // jQuery extensions & plugins
  'lib/jquery.extensions',
  'lib/jquery.touchscrollselect',
  '../lib/jquery.hammer',
  '../lib/jquery-ui-1.9.2.custom.min',
  '../lib/jquery-ui.touch-punch-0.2.3',
  
  // Other
  'lib/underscore.extensions',
  'lib/js.extensions',
  '../lib/linq'
  
], function(Backbone, $, config, Touchee){

  Touchee.noConflict();
  
  
  // Set Touchee shortcut in global scope
  this.T = this.Touchee = Touchee;
  
  // Set config
  // TODO: eval is evil
  Touchee.Config.set( eval('(' + config + ')') );
  
  // Set locale in i18n config
  require.config({ config: { i18n: { locale: Touchee.Config.get('locale') } } });
  
  // Set log level
  Touchee.Log.level(Touchee.Config.get('logLevel'));
  
  // Hammertime!
  Hammer(document.body,{
    hold_timeout:           750,
    drag_min_distance:      5,
    drag_block_horizontal:  true,
    show_touches:           false,
    swipe:                  false,
    transform:              false,
    prevent_mouseevents:    Touchee.touch
  });
  
  // Load the locale file
  require(['i18n!nls/locale'], function(){
    require(['app'], function(App){
      App.initialize();
    });
  });
  
});