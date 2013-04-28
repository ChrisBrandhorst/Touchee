define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/common_split_details'
], function($, _, Backbone, CommonSplitDetailsView) {

  var PlayableSplitDetailsView = CommonSplitDetailsView.extend({
    
    initialize: function() {
      CommonSplitDetailsView.prototype.initialize.apply(this, arguments);
      Touchee.enableControlCluster(this, {});
    },

    onRemove: function() {
      this.model.dispose();
    }
    
  });
  
  return PlayableSplitDetailsView;
  
});