define([
  'underscore',
  'Backbone',
  'models/contents_container',
  './../track'
], function(_, Backbone, ContentsContainer, Track){
  
  var MasterPlaylist = ContentsContainer.extend({
    
    // The model used for the items within the contents object
    contentsItemModel: Track
    
  });

  return MasterPlaylist;
  
});