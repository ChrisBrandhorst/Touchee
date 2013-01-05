define([
  'underscore',
  'Backbone',
  'models/contents'
], function(_, Backbone, Contents){
  
  var ArtistContents = Contents.extend({
    
    getUrl: function(id) {
      return [this.url(), this.filter.toString({type:'track',artist:id})].join("/");
    },
    
    getTitle: function() {
      var title = this.filter.get('genre');
      if (!title)
        title = Contents.prototype.getTitle.apply(this, arguments);
      return title;
    }
    
  });
  
  return ArtistContents;
  
});