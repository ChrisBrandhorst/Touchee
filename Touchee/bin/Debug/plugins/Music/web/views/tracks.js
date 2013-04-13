define([
  'jquery',
  'underscore',
  'Backbone',
  './../models/view_models/tracks',
  'views/contents/common_table',
  'views/popup/actions'
], function($, _, Backbone, Tracks, CommonTableView, ActionsPopupView) {
  
  var TracksView = CommonTableView.extend({
    
    
    // ScrollList properties
    className:  'tracks',
    index:      'titleSort',
    
    
    // Table properties
    columns: [
      'title',
      'artist',
      'album',
      'duration$'
    ],
    
    
    // Which model this view is supposed to show
    viewModel: Tracks,


    events: {
      'hold tr': 'hold'
    },


    hold: function(ev) {
      var row = ev.target.parentNode;

      var p = new ActionsPopupView({
        header: row.childNodes[0].innerHTML,
        buttons: [
          { text: 'Play Next' },
          { text: 'Add to Up Next' }
        ]
      });
      p.showRelativeTo(row);

    }

    
    
  });
  
  return TracksView;
  
});