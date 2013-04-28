define([
  'jquery',
  'underscore',
  'Backbone',
  'views/browser/_views_popup',
  'text!views/browser/_views.html'
], function($, _, Backbone, BrowserViewsPopup , browserViewsTemplate) {
  browserViewsTemplate = _.template(browserViewsTemplate);


  var BrowserViewsView = Backbone.View.extend({

    el: '#views',


    // Events
    events: {
      'tap [data-button=more]': 'showMoreViews'
    },


    // Contains strings of the views which are hidden
    hiddenViews: [],


    // Constructor
    initialize: function() {
      $(window).on('resize', _.bind(this.render, this));
    },


    // Render
    render: _.debounce(function() {

      // Render the buttons
      this.$el.html(
        browserViewsTemplate(this)
      );

      // Collect the items which must be out of view and hide them
      var $nav            = this.$('> nav'),
          elWidth         = this.$el.width(),
          buttons         = $nav.children().get(),
          removedButtons  = [],
          selectedRemoved = false,
          buttonsWidth;

      var getButtonsWidth = function() {
        return _.reduce(buttons, function(sum, b){ return sum + $(b).outerWidth(); }, 0);
      };

      while ((buttonsWidth = getButtonsWidth()) > elWidth && buttons.length)
        removedButtons.push( buttons.pop() );

      if (removedButtons.length) {
        removedButtons.push( buttons.pop() );
        var $more = $('<button data-button="more">' + I18n.browser.moreViews + '</button>').appendTo($nav).toggleClass('selected', selectedRemoved);
        this.hiddenViews = _.map(removedButtons, function(b){
          b.parentNode.removeChild(b);
          return b.getAttribute('data-view');
        }).reverse();

        if (!$nav.children('.selected').length) $more.addClass('selected');
      }
      else
        this.hiddenViews = [];
      
    }, 100),


    // Gets the view text for the given container and view
    getViewText: function(view) {
      var key   = 'views.' + view,
          text  = I18n.t(key);
      
      if (key == text) {
        key   = 'models.'+view;
        text  = I18n.t(key, {count:2});
        if (key == text)
          text = view;
      }
      
      return text.toTitleCase();
    },


    // Update the list
    update: function(container, selectedView) {
      this.container    = container;
      this.selectedView = selectedView;
      this.render();
      if (this.popup)
        this.popup.render();
    }, 


    // Open the more views popup
    showMoreViews: function(ev) {
      var $selected = this.$('> nav > .selected').removeClass('selected'),
          $more     = $(ev.target).addClass('selected'),
          view      = this;

      this.popup = new BrowserViewsPopup({viewsView:this})
        .showRelativeTo($more)
        .on('beforeHide', function(){
          delete view.popup;
          $more.removeClass('selected');
          $selected.addClass('selected');
        });
    }


  });

  return BrowserViewsView;

});