define([
  'jquery',
  'underscore',
  'Backbone',
  'views/popup/base',
  'views/queue/list',
  'text!views/queue/_popup.html'
], function($, _, Backbone, PopupView, QueueListView, queuePopupTemplate) {
  queuePopupTemplate = _.template(queuePopupTemplate);
  
  var QueuePopupView = PopupView.extend({
    
    
    // Backbone View options
    id: 'queue_popup',


    // Render
    render: function() {
      this.list = new QueueListView();
      this.$contents = $(queuePopupTemplate());
      this.$contents.append(this.list.$el);
      this.$el.append(this.$contents);

      this.on('show', this.firstShow, this);

      PopupView.prototype.render.apply(this, arguments);
    },


    firstShow: function() {
      this.list.render();
      this.off('show', this.firstShow);
    },
    
    
    // 
    getRequiredContentHeight: function() {
      // TODO: return full screen height
      return 500;
    }
    
    
  });
  
  return new QueuePopupView;
  
});