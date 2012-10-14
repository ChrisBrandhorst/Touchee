define([
  'Underscore',
  'Backbone',
  'models/contents'
], function(_, Backbone, Contents){
  
  var ChannelContents = Contents.extend({
    
    getUrl: function(id) {
      var filter = {};
      filter[this.idAttribute] = id;
      return ["play", "container", this.container.id, this.filter.toString(filter)].join("/");
    },
    
    
    getTitle: function() {
      var title = this.filter.get('genre');
      if (!title)
        title = Contents.prototype.getTitle.apply(this, arguments);
      return title;
    }
    
  });
  
  return ChannelContents;
  
});