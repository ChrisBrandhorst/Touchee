define([
  'jquery',
  'underscore',
  'Backbone',
  'models/queue',
  'text!views/queue/_index.html',
  'text!views/queue/_list.html',
  'text!views/queue/_item.html'
], function($, _, Backbone, Queue, queueIndexTemplate, queueListTemplate, queueItemTemplate) {
  queueIndexTemplate = _.template(queueIndexTemplate);
  queueListTemplate = _.template(queueListTemplate);
  queueItemTemplate = _.template(queueItemTemplate);

  var QueueIndexView = Backbone.View.extend({
    

    // Backbone View options
    className:  'contents',
    model:      Queue,


    initialize: function(options) {
      this.listenTo(this.model, 'reset add remove change', this.update);
    },


    render: function() {
      this.$el.html(queueIndexTemplate());
      this.$list = this.$('> ul');
      this.update();
    },


    update: function() {

      var $items    = $(queueListTemplate({
            queue:        this.model,
            itemTemplate: queueItemTemplate
          })),
          $oldItems = this.$list.contents();

      $items.appendTo(this.$list);
      $oldItems.remove();

      this.$list.attr('data-message', this.model.length ? "" : I18n.queue.empty);

    }


  });
  
  return new QueueIndexView;
  
});