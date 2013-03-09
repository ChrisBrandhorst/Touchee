define([
  'underscore',
  'Backbone',
  'models/contents_part'
], function(_, Backbone, ContentsPart){
  
  var Tracks = ContentsPart.extend({
    
    
    // 
    order: function(enumerator) {
      return enumerator
        .OrderBy("m => m.attributes.titleSort")
        .ThenBy("m => m.attributes.artistSort");
    }
    
    
  });
  
  return Tracks;

});