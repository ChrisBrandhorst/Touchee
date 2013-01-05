define([
  'underscore',
  'Backbone',
  'models/contents'
], function(_, Backbone, Contents){
  
  var AlbumContents = Contents.extend({
    
    getUrl: function(id) {
      return [this.url(), this.filter.toString({type:'track',albumid:id})].join("/");
    }
    
  });
  
  return AlbumContents;
  
});