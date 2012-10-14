define([
  'underscore',
  'Backbone',
  'models/contents'
], function(_, Backbone, Contents){
  
  var GenreContents = Contents.extend({
    
    getUrl: function(id) {
      return [this.url(), this.filter.toString({type:'channel',genre:id})].join("/");
    }
    
  });
  
  return GenreContents;
  
});