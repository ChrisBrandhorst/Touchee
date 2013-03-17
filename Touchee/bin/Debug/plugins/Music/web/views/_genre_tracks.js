define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/common_grouped_table'
], function($, _, Backbone, CommonGroupedTableView) {  
  
  var GenreTracksView = CommonGroupedTableView.extend({
    

    // ScrollList properties
    contentType:  'tracks',
    quickscroll:  true,
    index:        function(item) {
      return item.get('album$') + " - " + item.get('albumArtist$');
    },
    
    
    // Table properties
    columns: [
      'trackNumber',
      'title',
      'duration$'
    ],
    artworkSize:  250


  });
  
  return GenreTracksView;
  
});