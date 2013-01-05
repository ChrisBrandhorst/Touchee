define([
  'jquery',
  'underscore',
  'Backbone',
  'text!views/browser.html',
  'i18n!nls/locale',
  
  'views/popup'
], function($, _, Backbone,
            browserTemplate,
            I18n,
            
            PopupView) {
  
  browserTemplate = _.template(browserTemplate);
  
  
  var BrowserView =  Backbone.View.extend({
    
    
    // Backbone view options
    tagName:    'section',
    id:         "browser",
    className:  'hidden',
    
    
    // Events
    events: {
      'click #album, [data-button=nav], [data-button=airplay], [data-button=prev], [data-button=next], [data-button=queue]': 'clickedAlbum'
    },
    
    
    
    // Constructor
    initialize: function(params) {
      
      // Immediately render
      this.render();
      
      this.$('#volume').slider({
        range:  'min',
        value:  100,
        min:    0,
        max:    100
      });
      var _this = this;
      _this.$('#lcd_position').slider({
        range:  'min',
        value:  28,
        min:    0,
        max:    216
      });
      
    },
    
    
    // Render browser view
    render: function() {
      this.$el.html(
        browserTemplate()
      ).appendTo(document.body);
      return this;
    },
    
    
    // Show the browser view
    show: function() {
      this.$el.removeClass('hidden');
      return this;
    },
    
    
    
    popup: null,
    
    clickedAlbum: function(ev) {
      if (!this.popup)
        this.popup = new PopupView();
      this.popup.showRelativeTo( $(ev.currentTarget) );
      
    }
    
    
    
    
    // === Container view handling ===
    
    // === / ===
    
    
    
    // === Buttons ===
    
    // === / ===
    
    
    
    // === Transport ===
    
    // === / ===
    
  });
  
  return new BrowserView;
});