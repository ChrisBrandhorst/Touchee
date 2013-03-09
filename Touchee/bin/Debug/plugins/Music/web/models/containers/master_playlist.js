define([
  'underscore',
  'Backbone',
  'models/container',
  './../track',
  './../view_models/tracks',
  './../view_models/albums',
  './../view_models/artists',
  './../view_models/artist_tracks'
], function(_, Backbone, Container,
            Track,
            Tracks, Albums, Artists,
            ArtistTracks
  ){
  
  var MasterPlaylist = Container.extend({
    
    views: {
      artists:    Artists,
      albums:     Albums,
      tracks:     Tracks,
      genres:     {},
      playlists:  {},
      
      artist:     ArtistTracks
    },
    
    contentsItemModel: Track
    
  });

  return MasterPlaylist;
  
});