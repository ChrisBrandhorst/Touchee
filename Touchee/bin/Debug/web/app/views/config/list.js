define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/common_list',
  'text!./_item.html'
], function($, _, Backbone, CommonListView, configItemTemplate) {

  var ConfigListView = CommonListView.extend({
    
    innerTagName: 'nav',
    innerClass:   'list icons',
    selectable:   'a',
    selection:    { keep: true },
    model:        Touchee.Config.sections
    
  });
  
  return new ConfigListView;
  
});