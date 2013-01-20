define([
  'jquery',
  'underscore',
  'Backbone',
  'Touchee'
], function($, _, Backbone, Touchee) {
  
  var ScrollList = Backbone.View.extend({
    
    // Backbone View options
    tagName:    'section',
    className:  'scrollable scroll_list',
    
    
    // Type of scrolllist
    listType:   'base',
    
    // Element used as floating index
    index:      '<div/>',
    
    
    // Constructor
    initialize: function() {
      this.$el.addClass(this.listType);
    },
    
    
    // 
    render: _.once(function() {
      
      // Check if we are visible
      if (!this.$el.is(':visible'))
        return Touchee.Log.error("Cannot render ScrollList if it is not visible yet!");
      
      // Build the floating index
      this.$index = $(this.index)
        .addClass('scroll_list-' + this.listType + '-index index')
        .html("A")
        .insertBefore(this.$el);
      
    })
    
    
    
  });
  
  return ScrollList;
  
});