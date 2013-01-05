define([
  'jquery',
  'underscore',
  'Backbone',
  'views/paged_view'
], function($, _, Backbone, PagedView) {
  
  var ContentsList = PagedView.extend({
    
    className:  'contents',
    
    // Constructor
    initialize: function(params) {
      this.container = params.container;
      this.type = params.type;
      PagedView.prototype.initialize.apply(this, arguments);
    }
    
  });
  
  return ContentsList;
  
});