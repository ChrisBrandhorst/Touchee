define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/scroll_list'
], function($, _, Backbone, ScrollListView) {
  
  var TableView = ScrollListView.extend({
    
    listType:     'table',
    columns:      ['id'],
    innerTagName: 'table',
    dummy:        '<tr><td>&nbsp;</td></tr>',
    showIndices:  false,
    
    // // Constructor
    // initialize: function(options) {
    //   ScrollListView.prototype.initialize.apply(this, arguments);
    // },
    // 
    // 
    // // 
    // render: function() {
    //   ScrollListView.prototype.render.apply(this, arguments);
    // },
    
    
    // Renders each item of the table
    renderItem: function(item, options) {
      var rendered = '<tr' + (options.odd ? ' class="odd"' : '') + '>';
      
      _.each(this.columns, function(col){
        rendered += "<td>" + ((col.call ? col.call(item, item) : item.get(col))||"") + "</td>";
      });
      
      rendered += "</tr>";
      return rendered;
    }
    
  });
  
  return TableView;
  
});