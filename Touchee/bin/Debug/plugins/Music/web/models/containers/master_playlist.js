define([
  'underscore',
  'Backbone',
  'models/container',
  './../view_models/songs',
  './../view_models/albums'
], function(_, Backbone, Container, Songs, Albums){
  
  var MasterPlaylist = Container.extend({
    
    views: {
      song:     Songs,
      album:    Albums,
      artist:   {},
      genre:    {},
      playlist: {}
    }
    
  });

  return MasterPlaylist;
  
});