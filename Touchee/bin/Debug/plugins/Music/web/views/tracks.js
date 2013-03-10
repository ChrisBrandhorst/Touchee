define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/common_table'
], function($, _, Backbone, CommonTableView) {
  
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
    ]
    
    
  });
  
  return TracksView;
  
});