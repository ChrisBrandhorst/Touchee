define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/common_table'
], function($, _, Backbone, CommonTableView) {
  
  var ArtistsView = CommonTableView.extend({
    
    
    // ScrollList properties
    contentType:  'artists',
    index:        'artistSort',
    
    
    // Table properties
    columns: [
      'artist$'
    ]
    
    
  });
  
  return ArtistsView;

});