// Require.js allows us to configure shortcut alias
// There usage will become more apparent futher along in the tutorial.
require.config({
  
  urlArgs:      "_=" + (new Date()).getTime(),
  waitSeconds:  1,
  
  paths: {
    underscore: 'lib/underscore.amd',
    Backbone:   'lib/backbone.amd',
    Touchee:    'lib/touchee',
    text:       '../lib/text-2.0.3',
    i18n:       '../lib/i18n-2.0.1',
    jquery:     '../lib/jquery-1.8.2.min'
  },
  
  shim: {
    'Backbone':                           ['underscore'],
    'lib/jquery.extensions':              ['jquery'],
    'lib/jquery.touchscrollselect':       ['jquery'],
    '../lib/jquery.hammer':               ['jquery'],
    '../lib/jquery-ui-1.9.2.custom.min':  ['jquery']
  }
  
});


// Load all vendor files first
require([
  
  // Config file
  'text!config.js',
  
  // Main libraries
  'Backbone',
  'jquery',
  
  // Backbone extensions
  'Touchee',
  'lib/backbone.extensions',
  
  // jQuery extensions & plugins
  'lib/jquery.extensions',
  'lib/jquery.touchscrollselect',
  '../lib/jquery.hammer',
  '../lib/jquery-ui-1.9.2.custom.min',
  
  // Other
  'lib/underscore.extensions',
  'lib/js.extensions',
  '../lib/fastclick.min',
  '../lib/linq'
  
], function(config, Backbone, $, Touchee){
  
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
  
  // Boot fastclick
  // new FastClick(document.body);
  Hammer(document.body,{
    hold_timeout:           750,
    drag_min_distance:      5,
    drag_block_horizontal:  true,
    show_touches:           false,
    swipe:                  false,
    transform:              false,
    prevent_mouseevents:    Touchee.touch
  });
  
  // Load the locale file so we can place it in the gloval scope (we are lazy)
  require(['i18n!nls/locale'], function(I18n){
    this.I18n = I18n;
    // Then, load the app
    require(['app'], function(App){
      App.initialize();
    });
  });
  
});