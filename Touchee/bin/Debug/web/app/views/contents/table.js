define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/scroll_list'
], function($, _, Backbone, ScrollListView) {
  
  var TableView = ScrollListView.extend({
    
    // ScrollList properties
    dummy:        '<tr><td colspan="4">&nbsp;</td></tr>',
    listType:     'table',
    innerTagName: 'table',
    indicesShow:  true,
    quickscroll:  'alpha',
    
    
    // The columns to show
    columns:        ['id'],
    
    
    // Renders each item of the table
    renderItem: function(item, options) {
      var table = this;
      var rendered = '<tr' + (options.odd ? ' class="odd"' : '') + '>';
      
      _.each(this.columns, function(col){
        rendered += "<td>" + table.getColumnValue(item, col).htmlEncode() + "</td>";
      });
      
      rendered += "</tr>";
      return rendered;
    },
    
    
    // Renders an index item
    renderIndex: function(index) {
      var rendered = '<tr class="index" data-index="' + index + '"><td>' + index + '</td>';
      for (var i = 1; i < this.columns.length; i++)
        rendered += '<td></td>';
      return rendered + '</tr>';
    },
    
    
    // Gets the value for the given column for the given model
    getColumnValue: function(model, col) {
      var val = col.call ? col.call(model, model) : model.get(col);
      if ((!val || val == "") && _.isString(col))
        val = this.getUnknownColumnValue(model, col);
      return val || "";
    },
    
    
    // Gets the unknown value for the given attribute of the model
    getUnknownColumnValue: function(model, attr) {
      return "";
    }
    
  });
  
  return TableView;
  
});