// Require.js allows us to configure shortcut alias
// There usage will become more apparent futher along in the tutorial.
require.config({
  
  urlArgs:      "_=" + (new Date()).getTime(),
  waitSeconds:  1,
  
  paths: {
    underscore: 'lib/underscore.amd',
    Backbone:   'lib/backbone.amd',
    jquery:     '../lib/jquery-1.8.2.min'
  },

  shim: {
    'Backbone':                           ['underscore']
  }
  
});


// Load all vendor files first
require([
  
  // Main libraries
  'Backbone',
  'jquery'
  
], function(Backbone, $){


  var Base = Backbone.Collection.extend({
    url: "media/1/containers/1/contents",
    parse: function(response) {
      return response.contents;
    }
  });

  var Other = Backbone.Collection.extend({
    comparator: 'title'
  });


  var B = new Base();
  var O = new Other();

  B.fetch({
    success: function(collection){
      console.log(collection.first().get('title'));
      O.reset(collection.models);
      console.log(O.first().get('title'));
      // O.sort();
      // console.log(O.first().get('title'));
    }
  });
  
});