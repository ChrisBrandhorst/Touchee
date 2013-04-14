define([
  'jquery',
  'underscore',
  'Backbone',
  './../models/view_models/tracks',
  'views/contents/common_table',
  'views/popup/play_actions'
], function($, _, Backbone, Tracks, CommonTableView, PlayActionsPopupView) {
  
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
    
    
    // A track  was selected
    selected: function(track) {
      Touchee.Queue.reset(this.model, {start: this.getIndex(track)});
    },


    // A track was held
    held: function(track, $row) {
      PlayActionsPopupView.show(track, $row, $row[0].childNodes[1].innerHTML);
    }


  });
  
  return TracksView;
  
});