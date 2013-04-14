define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/common_split_details',
  'views/popup/play_actions'
], function($, _, Backbone, CommonSplitDetailsView, PlayActionsPopupView) {

  var PlayableSplitDetailsView = CommonSplitDetailsView.extend({
    
    initialize: function() {
      CommonSplitDetailsView.prototype.initialize.apply(this, arguments);
      this.$el.on('tap.delegateEvents' + this.cid, '.control_cluster > [data-button]', _.bind(this.control, this));
    },

    control: function(ev) {
      switch( $(ev.target).attr('data-button') ) {
        case 'play':
          Touchee.Queue.reset(this.model);
          break;
        case 'shuffle':
          Touchee.Queue.reset(this.model, {shuffle:true});
          break;
        case 'menu':
          PlayActionsPopupView.show(this.model, ev.target);
          break;
      }
    }

  });
  
  return PlayableSplitDetailsView;
  
});