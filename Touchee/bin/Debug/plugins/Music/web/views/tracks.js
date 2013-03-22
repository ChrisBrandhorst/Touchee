define([
  'jquery',
  'underscore',
  'Backbone',
  './../models/view_models/tracks',
  'views/contents/common_table'
], function($, _, Backbone, Tracks, CommonTableView) {
  
  var TracksView = CommonTableView.extend({
    
    
    // ScrollList properties
    contentType:  'tracks',
    index:        'titleSort',
    
    
    // Table properties
    columns: [
      'title',
      'artist',
      'album',
      'duration$'
    ],
    
    
    // Which model this view is supposed to show
    viewModel: Tracks
    
    
  });
  
  return TracksView;
  
});