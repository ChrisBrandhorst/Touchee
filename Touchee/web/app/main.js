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
    jquery:     '../lib/jquery-1.8.2.min'
  },
  
  shim: {
    'Backbone':                     ['underscore'],
    'lib/jquery.extensions':        ['jquery'],
    'lib/jquery.touchscrollselect': ['jquery'],
    'lib/jquery.scrolllist':        ['jquery']
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
  
  // Boot fastclick
  new FastClick(document.body);
  
  // Then, load the app
  require([
    'app',
    'locales/' + Touchee.Config.get('locale')
  ], function(App){
    
    App.initialize();
    
  });

  
});