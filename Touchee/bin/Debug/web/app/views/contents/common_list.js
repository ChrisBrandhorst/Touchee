define([
  'jquery',
  'underscore',
  'Backbone',
  'models/dummy_item',
  'views/contents/template_list',
  'text!./_common_list_item.html'
], function($, _, Backbone, DummyItem, TemplateListView, commonListItemTemplate) {

  var CommonListView = TemplateListView.extend({
    
    innerTagName:   'nav',
    innerClass:     'list icons',
    selectable:     'a',
    selection:      { keep: true },
    template:       _.template(commonListItemTemplate),
    classAttribute: 'id',
    titleAttribute: 'name',

    dummy:          function() {
      return this.template({
        model:  new DummyItem(),
        view:   this
      });
    },

    renderItem: function(item, i) {
      return this.template({
        model:  item,
        view:   this
      });
    },
    
  });
  
  return CommonListView;
  
});