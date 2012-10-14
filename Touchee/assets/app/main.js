// Require.js allows us to configure shortcut alias
// There usage will become more apparent futher along in the tutorial.
require.config({
  urlArgs: "_=" + (new Date()).getTime(),
  paths: {
    jquery:     '../lib/jquery-1.7.1.min',
    Underscore: 'lib/underscore.amd',
    Backbone:   'lib/backbone.amd',
    order:      '../lib/order-1.0.5.min',
    text:       '../lib/text-1.0.7.min'
  }
});



var Touchee = {}, T = Touchee;



// Load all vendor files first
require([
  
  // Main libraries
  'order!Underscore',
  'order!Backbone',
  'order!jquery',
  
  // Backbone extensions
  'order!lib/backbone.extensions',
  
  // jQuery extensions & plugins
  'order!lib/jquery.extensions',
  'order!lib/jquery.touchscrollselect',
  'order!lib/jquery.scrolllist',
  
  // Other
  'lib/js.extensions',
  '../lib/fastclick.min',
  
  // App config
  'config'
  
], function(){
  
  // Boot fastclick
  new FastClick(document.body);
  
  // Then, load the app
  require([
    'app',
    'logger',
    'locales/' + T.config.locale
  ], function(App, Logger){
    App.initialize({
      logLevel:   Logger.Levels.DEBUG
    });
  });

  
});