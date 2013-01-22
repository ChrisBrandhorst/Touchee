define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/scroll_list'
], function($, _, Backbone, ScrollListView) {
  
  var TableView = ScrollListView.extend({
    
    listType:       'table',
    columns:        ['id'],
    innerTagName:   'table',
    dummy:          '<tr><td>&nbsp;</td></tr>',
    indexAttribute: 'titleSort',
    indicesShow:    true,
    
    
    // Renders each item of the table
    renderItem: function(item, options) {
      var rendered = '<tr' + (options.odd ? ' class="odd"' : '') + '>';
      
      _.each(this.columns, function(col){
        rendered += "<td>" + ((col.call ? col.call(item, item) : item.get(col))||"") + "</td>";
      });
      
      rendered += "</tr>";
      return rendered;
    },
    
    
    // Renders an index item
    renderIndex: function(index) {
      var rendered = '<tr class="index"><td>' + index + '</td>';
      for (var i = 1; i < this.columns.length; i++)
        rendered += '<td></td>';
      return rendered + '</tr>';
    }
    
  });
  
  return TableView;
  
});