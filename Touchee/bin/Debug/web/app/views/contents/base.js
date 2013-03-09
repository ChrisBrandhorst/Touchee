define([
  'jquery',
  'underscore',
  'Backbone',
  'Touchee'
], function($, _, Backbone, Touchee) {
  
  var BaseView = Backbone.View.extend({
    
    
    // Navigate the current view given the supplied params
    // VIRTUAL
    navigate: function(params) { }
    
    
  });
  
  return BaseView;
  
});