define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/common_table'
], function($, _, Backbone, CommonTableView) {
  
  var GenresView = CommonTableView.extend({
    
    
    // ScrollList properties
    contentType:  'genres',
    index:        'genreSort',
    selection:    { keep:true },
    
    
    // Table properties
    columns: [
      'genre$'
    ]
    
    
  });
  
  return GenresView;

});