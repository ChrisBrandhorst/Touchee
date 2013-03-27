define([
  'jquery',
  'underscore',
  'Backbone',
  'views/_browser_views_popup',
  'text!views/_browser_views.html'
], function($, _, Backbone, BrowserViewsPopup , browserViewsTemplate) {
  browserViewsTemplate = _.template(browserViewsTemplate);


  var BrowserViewsView = Backbone.View.extend({

    el: '#views',


    // Events
    events: {
      'click [data-button=more]': 'showMoreViews'
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
      
      // $nav.css('margin-left', (elWidth - buttonsWidth) / 2 + 'px');
    }, 100),


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


    // Update the list
    update: function(container, selectedView) {
      this.container    = container;
      this.selectedView = selectedView;
      this.render();
    }, 


    // Open the more views popup
    showMoreViews: function(ev) {
      var $selected = this.$('> nav > .selected').removeClass('selected'),
          $more     = $(ev.target).addClass('selected');

      new BrowserViewsPopup({viewsView:this})
      .showRelativeTo($more)
      .on('beforeHide', function(){
        $more.removeClass('selected');
        $selected.addClass('selected');
      });
    }

  });

  return BrowserViewsView;

});