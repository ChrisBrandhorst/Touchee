define([
  'underscore',
  'Backbone',
  'models/container',
  './../track',
  './../view_models/songs',
  './../view_models/albums'
], function(_, Backbone, Container, Track, Songs, Albums){
  
  var MasterPlaylist = Container.extend({
    
    views: {
      album:    Albums,
      song:     Songs,
      artist:   {},
      genre:    {},
      playlist: {}
    },
    
    contentsItemModel: Track
    
  });

  return MasterPlaylist;
  
});