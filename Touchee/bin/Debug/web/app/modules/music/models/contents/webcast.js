define([
  'underscore',
  'Backbone',
  'models/contents'
], function(_, Backbone, Contents){
  
  var WebcastContents = Contents.extend({
    
    getUrl: function(id) {
      var filter = {};
      filter[this.idAttribute] = id;
      return this.url().replace(/contents$/, "play/" + this.filter.toString(filter));
    }
    
  });
  
  return WebcastContents;
  
});