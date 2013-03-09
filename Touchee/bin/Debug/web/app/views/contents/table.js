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
    selectable:   'tr:not(.index)',
    
    // The columns to show
    columns:        ['id'],
    
    
    
    
    // ScrollList overrides
    // --------------------
    
    // Renders each item of the table
    // VIRTUAL
    renderItem: function(item, i) {
      var table = this;
      var rendered = '<tr' + (i % 2 == 0 ? ' class="odd"' : '') + '>';
      
      _.each(this.columns, function(col){
        rendered += "<td>" + table.getAttribute(item, col).htmlEncode() + "</td>";
      });
      
      rendered += "</tr>";
      return rendered;
    },
    
    
    // Renders an index item
    // VIRTUAL
    renderIndex: function(index) {
      var rendered = '<tr class="index" data-index="' + index + '"><td>' + index + '</td>';
      for (var i = 1; i < this.columns.length; i++)
        rendered += '<td></td>';
      return rendered + '</tr>';
    },
    
    
    // An item has been selected
    // VIRTUAL
    selected: function(item, $item) {
      Backbone.history.navigate(this.model.getUrl(item), {trigger:true});
    },
    
    
    
    
    // Attribute value getting
    // -----------------------
    
    // Gets the value for the given attribute for the given model
    // VIRTUAL
    getAttribute: function(model, attr) {
      var val = attr.call ? attr.call(model, model) : model.get(attr);
      if ((!val || val == "") && _.isString(attr))
        val = this.getUnknownAttributeValue(model, attr);
      return val || "";
    },
    
    
    // Gets the unknown value for the given attribute of the model
    // VIRTUAL
    getUnknownAttributeValue: function(model, attr) {
      return "&nbsp;";
    }
    
    
  });
  
  return TableView;
  
});