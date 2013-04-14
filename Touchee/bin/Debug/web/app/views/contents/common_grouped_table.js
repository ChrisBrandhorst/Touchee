define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/grouped_table',
  'views/popup/play_actions'
], function($, _, Backbone, GroupedTableView, PlayActionsPopupView) {
  
  var CommonGroupedTableView = GroupedTableView.extend({


    // Get the available groups from the model
    // VIRTUAL
    getGroups: function(groupBy) {
      return this.model.groupBy(groupBy);
    }


  });
  
  return CommonGroupedTableView;

});