define([
  'underscore',
  'Backbone',
  'models/contents'
], function(_, Backbone, Contents){
  
  var ChannelContents = Contents.extend({
    
    getTitle: function() {
      var title = this.filter.get('genre');
      if (!title)
        title = Contents.prototype.getTitle.apply(this, arguments);
      return title;
    }
    
  });
  
  return ChannelContents;
  
});