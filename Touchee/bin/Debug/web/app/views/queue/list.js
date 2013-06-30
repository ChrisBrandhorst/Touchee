define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/template_list',
  'text!views/queue/_item.html'
], function($, _, Backbone, TemplateListView, queueItemTemplate) {

  var QueueListView = TemplateListView.extend({
    
    showIndex:  'custom',
    index:      function(item, i){
      return 0 < this.model.priorityCount && this.model.priorityCount <= i
        ? "Back To"
        : "Up Next";
    },
    template:   _.template(queueItemTemplate),
    
    // Gets the model count
    getCount: function() {
      return this.model.length - 1;
    },
    
    
    // Gets the models
    getItems: function(first, count) {
      return this.model.models.slice(first + 1, first + count + 1);
    },
    
    
    // An item from the queue was selected
    selected: function(item, idx, $row) {
      
    }
    
    
  });
  
  return QueueListView;

});