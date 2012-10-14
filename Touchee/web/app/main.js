// Require.js allows us to configure shortcut alias
// There usage will become more apparent futher along in the tutorial.
require.config({
  
  urlArgs: "_=" + (new Date()).getTime(),
  
  paths: {
    underscore: 'lib/underscore.amd',
    Backbone:   'lib/backbone.amd',
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
  'lib/touchee',
  'lib/backbone.extensions',
  
  // jQuery extensions & plugins
  'lib/jquery.extensions',
  'lib/jquery.touchscrollselect',
  'lib/jquery.scrolllist',
  
  // Other
  'lib/js.extensions',
  '../lib/fastclick.min'
  
], function(config, Backbone, $, Touchee){
  
  // Set Touchee shortcut
  this.T = Touchee;
  
  // Set config
  T.Config.set( eval('(' + config + ')') );
  
  // Boot fastclick
  new FastClick(document.body);
  
  // Then, load the app
  require([
    'app',
    'locales/' + T.Config.get('locale')
  ], function(App){
    
    App.initialize();
    
  });

  
});