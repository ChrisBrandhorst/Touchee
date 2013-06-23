define([
  'jquery',
  'underscore',
  'Backbone',
  'views/popup/base',
  'views/queue/index'
], function($, _, Backbone, PopupView, QueueIndexView) {
  
  var QueuePopupView = PopupView.extend({
    
    
    // Backbone View options
    id: 'queue_popup',


    // Render
    render: function() {
      QueueIndexView.render();
      this.$el.append(QueueIndexView.$el);
      PopupView.prototype.render.apply(this, arguments);
    },
    
    
    // 
    getRequiredContentHeight: function() {
      // TODO: return full screen height
      return 500;
    }
    
    
  });
  
  return new QueuePopupView;
  
});