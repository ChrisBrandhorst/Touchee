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
    getModels: function(first, count) {
      return this.model.models.slice(first, first + count);
    },
    
    
    // Gets the index of the given item
    getModelIndex: function(item) {
      return this.model.models.indexOf(item);
    },

    // An item has been selected
    // VIRTUAL
    selected: function(item, $row) {
      Backbone.history.navigate(this.model.getUrl(item), {trigger:true});
    }
    
    
  });
  
  return CommonTableView;

});