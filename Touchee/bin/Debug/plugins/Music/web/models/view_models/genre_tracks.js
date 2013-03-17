define([
  'underscore',
  'Backbone',
  './grouped_tracks'
], function(_, Backbone, GroupedTracks){
  
  var GenreTracks = GroupedTracks.extend({
    groupByAttr: 'genre'
  });
  
  return GenreTracks;

});