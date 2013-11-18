define([
  'jquery',
  'underscore',
  'Backbone'
], function($, _, Backbone) {

  var BasicContentView = Backbone.View.extend({
    
    tagName:  'section',
    template: null,


    initialize: function() {
      this.$el.addClass('basic_content scrollable');
    },


    render: function() {
      // Content renderen als template is gegeven
      if (this.template) {
        var templateFunc = _.isString(this.template) ? _.template(this.template) : this.template;
        this.$el.html(templateFunc(this));
      }
    }

  });
  
  return BasicContentView;
  
});