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
    showIndex:    true,
    quickscroll:  'alpha',
    selectable:   'tr:not(.index)',
    
    
    // The columns to show
    columns:        ['id'],
    
    
    
    
    // ScrollList overrides
    // --------------------
    
    // Renders each item of the table
    // VIRTUAL
    renderItem: function(item, i) {
      var table = this,
          klass = i % 2 == 0 ? "odd" : null;
      
      if (this.data.selection && this.data.selection.item == item)
        klass = (klass || "") + " " + this.selection.klass;

      var rendered = '<tr' + (klass ? ' class="' + klass + '"' : '') + '>';
      
      _.each(this.columns, function(col){
        rendered += "<td>" + table.getAttribute(item, col).toString().htmlEncode() + "</td>";
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
      return val || "";
    }
    
    
  });
  
  return TableView;
  
});