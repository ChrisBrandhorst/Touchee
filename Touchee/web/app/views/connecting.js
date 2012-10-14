define([
  'jquery',
  'underscore',
  'Backbone',
  'text!views/connecting.html'
], function($, _, Backbone, connectingTemplate) {
  connectingTemplate = _.template(connectingTemplate);
  
  var ConnectingView =  Backbone.View.extend({
    
    
    id: 'connecting',
    
    
    initialize: function() {
      this.render();
    },
    
    
    render: function() {
      this.$el.html(
        connectingTemplate()
      );
      this.$header = this.$el.find('p');
      this.$connecting = this.$el.find('span');
      this.$popup = this.$el.find('.popup');
      return this;
    },
    
    
    setStatus: function(connectingText, headerText) {
      if (headerText && headerText != "")
        this.$header.html(headerText).show();
      
      this.$connecting.html(connectingText);
      return this;
    },
    
    
    show: function() {
      var $el = this.$el;
      if (!$el.is(':visible')) {
        $el.addClass('hidden');
        this.render();
        $el.appendTo(document.body);
        _.defer(function(){
          $el.removeClass('hidden');
        });
      }
      return this;
    },
    
    
    hide: function() {
      var $el = this.$el;
      if ($el.is(':visible')) {
        $el.addClass('hidden');
        _.delay(function(){
          $el.remove();
        }, $el.getAnimDuration() );
      }
      return this;
    }
    
    
  });
  
  return new ConnectingView;
  
});