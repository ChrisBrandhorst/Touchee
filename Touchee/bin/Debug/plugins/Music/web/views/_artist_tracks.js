define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/common_grouped_table'
], function($, _, Backbone, CommonGroupedTableView) {  
  
  var ArtistTracksView = CommonGroupedTableView.extend({
    

    // ScrollList properties
    contentType:  'tracks',
    quickscroll:  true,
    index:        function(item) {
      return item.get('album$');
    },
    
    
    // Table properties
    columns: [
      'trackNumber',
      'title',
      'duration$'
    ],
    artworkSize:  250


  });
  
  return ArtistTracksView;
  
});