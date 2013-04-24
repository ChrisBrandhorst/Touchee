define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/common_grouped_table',
  'views/popup/play_actions'
], function($, _, Backbone, CommonGroupedTableView, PlayActionsPopupView) {

  var PlayableGroupedTable = CommonGroupedTableView.extend({


    // An item was selected
    // VIRTUAL
    selected: function(item, idx, $row) {
      console.log(idx);
      Touchee.Queue.reset(this.model, {start:idx});
    },


    // An item was held
    // VIRTUAL
    held: function(item, $row) {
      PlayActionsPopupView.show(item, $row, $row[0].childNodes[1].innerHTML);
    }


  });
  
  return PlayableGroupedTable;
  
});