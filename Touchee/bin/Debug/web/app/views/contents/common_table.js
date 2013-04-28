define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/table'
], function($, _, Backbone, TableView) {
  
  var CommonTableView = TableView.extend({
    
    
    // Gets the model count
    getCount: function() {
      return this.model.length;
    },
    
    
    // Gets the models
    getItems: function(first, count) {
      return this.model.models.slice(first, first + count);
    },
    
    
    // 
    // VIRTUAL
    selected: function(item, idx, $row) {
      Backbone.history.navigate(this.model.getUrlFor(item), {trigger:true});
    }
    
    
  });
  
  return CommonTableView;

});