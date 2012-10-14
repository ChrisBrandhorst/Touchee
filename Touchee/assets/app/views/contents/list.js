define([
  'jquery',
  'Underscore',
  'Backbone',
  'views/paged_view'
], function($, _, Backbone, PagedView) {
  
  var ContentsList = PagedView.extend({
    
    // Constructor
    initialize: function(params) {
      this.container = params.container;
      this.type = params.type;
      this.$el = $('<div class="contents"/>');
      this.el = this.$el[0];
      PagedView.prototype.initialize.apply(this, arguments);
    }
    
  });
  
  return ContentsList;
  
});