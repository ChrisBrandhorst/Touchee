define([
  'underscore',
  'Backbone',
  'models/container',
  './../view_models/songs',
  './../view_models/albums'
], function(_, Backbone, Container, Songs, Albums){
  
  var MasterPlaylist = Container.extend({
    
    views: {
      album:    Albums,
      song:     Songs,
      artist:   {},
      genre:    {},
      playlist: {}
    }
    
  });

  return MasterPlaylist;
  
});