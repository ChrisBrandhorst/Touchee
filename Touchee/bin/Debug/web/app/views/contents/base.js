define([
  'jquery',
  'underscore',
  'Backbone'
], function($, _, Backbone) {
  
  var BaseView = Backbone.View.extend({
    
    
    // Navigate the current view given the supplied params
    // VIRTUAL
    navigate: function(params) { }
    
    
  });
  
  return BaseView;
  
});