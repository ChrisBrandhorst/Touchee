define([
  'underscore',
  'Backbone',
  'models/filter'
], function(_, Backbone, Filter){
  
  var Track = Backbone.Model.extend({
    
    artworkUrl: function(filter) {
      var parts = [this.collection.container.url(), "artwork/id", this.id];
      if (filter) parts.push(new Filter(filter).toString());
      return parts.join('/');
    }
    
  });

  return Track;
  
});