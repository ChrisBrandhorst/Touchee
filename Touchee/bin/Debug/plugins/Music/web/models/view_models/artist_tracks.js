define([
  'underscore',
  'Backbone',
  './grouped_tracks'
], function(_, Backbone, GroupedTracks){
  
  var ArtistTracks = GroupedTracks.extend({
    groupByAttr: 'artist'
  });
  
  return ArtistTracks;

});