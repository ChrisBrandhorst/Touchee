define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/scroll_list'
], function($, _, Backbone, ScrollListView) {
  
  var TemplateListView = ScrollListView.extend({
    
    
    // ScrollList properties
    listType:     'template',
    innerTagName: 'ul',
    showIndex:    true,
    quickscroll:  false,
    selectable:   'li:not(.index)',
    dummy:        function(){
      return this.template({});
    },
    
    
    // TemplateList properties
    template:     null,
    

    
    // ScrollList overrides
    // --------------------

    // Renders each item of the list
    // VIRTUAL
    renderItem: function(item, i) {
      return this.template(item);
    },


    // Renders an index item
    // VIRTUAL
    renderIndex: function(index) {
      return '<li class="index" data-index="' + index + '"></li>';
    },


    // Gets the index of the item in the items collection for the given rendered element
    // VIRTUAL
    getItemIndexByElement: function(el) {
      return this.data.lastRender.first + $(el).prevAll(':not(.index)').length;
    }
    
    
  });
  
  return TemplateListView;
  
});