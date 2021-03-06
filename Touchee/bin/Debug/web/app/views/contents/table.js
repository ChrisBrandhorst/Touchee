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
      
      if (this.selection.keep && this.data.selectedItem == item)
        klass = (klass || "") + " " + this.selection.klass;

      var rendered = '<tr' + (klass ? ' class="' + klass + '"' : '') + '>';
      
      _.each(this.columns, function(col){
        rendered += "<td>" + _.escape(table.getAttribute(item, col)) + "</td>";
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


    // Gets the index of the item in the items collection for the given rendered element
    // VIRTUAL
    getItemIndexByElement: function(el) {
      return this.data.lastRender.first + $(el).prevAll(':not(.index)').length;
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