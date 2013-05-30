define([
  'underscore',
  'Backbone',
  'models/collections/media'
], function(_, Backbone, Media) {
  
  var Library = {
    
    
    // Init
    initialize: function() {
    }
    

  };
  
  _.extend(Library, Backbone.Events);

  return Library;

});