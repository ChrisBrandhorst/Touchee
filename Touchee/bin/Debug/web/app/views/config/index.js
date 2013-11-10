define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/split',
  'views/config/list'
], function($, _, Backbone, SplitView, ConfigListView) {

  var ConfigView = SplitView.extend({
    
    tagName:  'section',
    id:       "config",
    left:     ConfigListView
    
  });
  
  return new ConfigView;
  
});