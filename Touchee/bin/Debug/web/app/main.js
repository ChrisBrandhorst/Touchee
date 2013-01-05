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
    'lib/jquery.scrolllist':              ['jquery'],
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
  'lib/jquery.scrolllist',
  '../lib/jquery-ui-1.9.2.custom.min',
  
  // Other
  'lib/js.extensions',
  '../lib/fastclick.min'
  
], function(config, Backbone, $, Touchee){
  
  Touchee.noConflict();
  
  // Set Touchee shortcut
  this.T = Touchee;
  
  // Set config
  // TODO: eval is evil
  Touchee.Config.set( eval('(' + config + ')') );
  
  // Set locale in i18n config
  require.config({ config: { i18n: { locale: Touchee.Config.get('locale') } } });
  
  // Boot fastclick
  new FastClick(document.body);
  
  // Then, load the app
  require(['app'], function(App){
    
    App.initialize();
    
  });

  
});