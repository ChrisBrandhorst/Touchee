define([
  'jquery',
  'underscore',
  'Backbone',
  'views/popup',
  'text!views/_browser_views.html',
  'text!views/_browser_views_popup.html'
], function($, _, Backbone, Popup, browserViewsTemplate, browserViewsPopupTemplate) {
  browserViewsTemplate = _.template(browserViewsTemplate);
  browserViewsPopupTemplate = _.template(browserViewsPopupTemplate);
  


  var BrowserViewsPopup = Popup.extend({

    render: function() {

    }

  });

  
  var BrowserViewsView = Backbone.View.extend({

    el: '#views',


    // Events
    events: {
      'click [data-button=more]': 'showMoreViews'
    },

    hiddenViews: [],

    // 
    render: function() {

      // Render the buttons
      this.$el.html(
        browserViewsTemplate(this)
      );

      // Check size
      this.checkSize();
    },


    // 
    checkSize: function() {
      var $nav            = this.$('> nav'),
          elWidth         = this.$el.width(),
          buttons         = $nav.children().get(),
          removedButtons  = [],
          buttonsWidth;

      var getButtonsWidth = function() {
        return _.reduce(buttons, function(sum, b){ return sum + $(b).outerWidth(); }, 0);
      };

      while ((buttonsWidth = getButtonsWidth()) > elWidth && buttons.length) {
        removedButtons.push( buttons.pop() );
      }
      if (removedButtons.length) {
        removedButtons.push( buttons.pop() );
        $('<button data-button="more">' + I18n.browser.moreViews + '</button>').appendTo($nav);
        this.hiddenViews = _.map(removedButtons, function(b){ b.parentNode.removeChild(b); return b.getAttribute('data-view'); }).reverse();
      }
      else
        this.hiddenViews = [];
      
      $nav.css('margin-left', (elWidth - buttonsWidth) / 2 + 'px');
    },


    // Gets the view text for the given container and view
    getViewText: function(container, view) {
      var plugin  = container.get('plugin'),
          key     = 'p.'+plugin+'.views.'+view,
          text    = I18n.t(key);
      
      if (key == text) {
        key     = 'p.'+plugin+'.models.'+view;
        text    = I18n.t(key, {count:2});
        if (key == text)
          text = view;
      }
      
      return text.toTitleCase();
    },


    //
    update: function(container, selectedView) {
      this.container    = container;
      this.selectedView = selectedView;
      this.render();
    }, 


    // 
    showMoreViews: function(ev) {
      var $selected = this.$('> nav > .selected').removeClass('selected'),
          $more     = $(ev.target).addClass('selected');

      var popup = new Popup().showRelativeTo($more);
      popup.$el.append(
        browserViewsPopupTemplate(this)
      );
      // popup.resizeToContents();

      // $more.removeClass('selected');
      // $selected.addClass('selected');
    }

  });

  return BrowserViewsView;

});