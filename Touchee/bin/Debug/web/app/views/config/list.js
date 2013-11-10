define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/template_list',
  'text!./_item.html'
], function($, _, Backbone, TemplateListView, configItemTemplate) {

  var ConfigListView = TemplateListView.extend({
    
    innerTagName: 'nav',
    selectable:   'a',
    selection:    {
      keep: true
    },
    innerClass:   'list icons',
    template:     _.template(configItemTemplate),
    model:        Touchee.Config.sections
    
  });
  
  return new ConfigListView;
  
});