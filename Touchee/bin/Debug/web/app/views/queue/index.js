define([
  'jquery',
  'underscore',
  'Backbone',
  'text!views/queue/_index.html'
], function($, _, Backbone, queueIndexTemplate) {
  queueIndexTemplate = _.template(queueIndexTemplate);

  var QueueIndexView = Backbone.View.extend({
    

    // Backbone View options
    className: 'contents',


    render: function() {
      this.$el.html(queueIndexTemplate());
      this.$el.append('<div class="list" data-message="No upcoming songs." />');
    }


  });
  
  return new QueueIndexView;
  
});