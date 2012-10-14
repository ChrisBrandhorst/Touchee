define([
  'underscore',
  'Backbone',
  'models/contents'
], function(_, Backbone, Contents){
  
  var TrackContents = Contents.extend({
    
    
    idAttribute: 'index',
    
    
    getUrl: function(id) {
      var filter = {};
      filter[this.idAttribute] = id;
      return ["play", "container", this.container.id, this.filter.toString(filter)].join("/");
    },
    
    
    getTitle: function() {
      var title = this.filter.get('artist');
      if (title == "")
        title = T.T.items.album.more.toTitleCase();
      if (!title)
        title = this.filter.get('genre');
      if (!title)
        title = Contents.prototype.getTitle.apply(this, arguments);
      return title;
    },
    
    
    //
    getViewType: function() {
      var viewType = Contents.prototype.getViewType.apply(this, arguments);
      
      if (typeof this.filter.get('artist') != 'undefined')
        viewType = "artist_" + viewType;
      else if (this.filter.get('albumid'))
        viewType = "album_" + viewType;
      
      return viewType;
    }
    
  });
  
  return TrackContents;
  
});