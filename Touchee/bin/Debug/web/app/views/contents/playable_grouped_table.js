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
    selected: function(item) {
      Touchee.Queue.reset(this.model, {start: this.getIndex(item)});
    },

    // An item was held
    // VIRTUAL
    held: function(item, $row) {
      PlayActionsPopupView.show(item, $row, $row[0].childNodes[1].innerHTML);
    }
    
  });
  
  return PlayableGroupedTable;
  
});