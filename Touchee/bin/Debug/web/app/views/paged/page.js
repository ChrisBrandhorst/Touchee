define([
  'jquery',
  'underscore',
  'Backbone'
], function($, _, Backbone) {
  
  var PageView = Backbone.View.extend({
    
    
    // 
    initialize: function(options) {
      options || (options = {});
      this.$header = $('<h1/>');
    },
    
    
    // 
    getHeader: function() {
      throw "NotImplementedException";
    },
    
    
    // 
    getBackButton: function() {
      return {
        text:       I18n.back,
        className:  'dark'
      };
    },
    
    
    // 
    render: function(options) {
      options || (options = {});
      
      this.$header.html( this.getHeader().toTitleCase() );
      
      if (options.backButton) {
        var back = this.getBackButton();
        if (_.isString(back))
          back = { text:back, className:'dark' };
        this.$header.prepend(
          $('<button type="button" class="ios back"/>').html(back.text).addClass(back.className)
        );
      }
    }
    
    
  });
  
  return PageView;
  
});