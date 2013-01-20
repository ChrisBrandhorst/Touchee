define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/scroll_list'
], function($, _, Backbone, ScrollListView) {
  
  var TableView = ScrollListView.extend({
    
    
    listType: 'table',
    columns:  ['id'],
    
    
    // Constructor
    initialize: function(options) {
      ScrollListView.prototype.initialize.apply(this, arguments);
    },
    
    
    // 
    render: function() {
      ScrollListView.prototype.render.apply(this, arguments);
    },
    
    
    // Renders each item of the table
    renderItem: function(item) {
      var rendered = "<tr>";
      
      _.each(this.columns, function(col){
        rendered += "<td>" + (col.call ? col.call(item, item) : item.get(col)) + "</td>";
      });
      
      rendered += "</tr>";
      return rendered;
    }
    
  });
  
  return TableView;
  
});