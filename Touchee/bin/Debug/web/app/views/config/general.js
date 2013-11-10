define([
  'jquery',
  'underscore',
  'Backbone',
  'views/basic_content',
  'text!./general.html'
], function($, _, Backbone, BasicContentView, template) {

  var GeneralConfigView = BasicContentView.extend({
    
    template: template,
    id:       "general_config"

  });
  
  return new GeneralConfigView;
  
});