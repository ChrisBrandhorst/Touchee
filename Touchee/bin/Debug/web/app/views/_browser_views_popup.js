define([
  'jquery',
  'underscore',
  'Backbone',
  'views/popup',
  'text!views/_browser_views_popup.html'
], function($, _, Backbone, Popup, browserViewsPopupTemplate) {
  browserViewsPopupTemplate = _.template(browserViewsPopupTemplate);

  var BrowserViewsPopup = Popup.extend({

    id: 'views_popup',

    // Remove the popup from the DOM when it is hidden
    removeOnHide: true,
    
    // Events
    events: {
      'click a':    'hide'
    },

    // Constructor
    initialize: function(options) {
      this.viewsView = options.viewsView;
      Popup.prototype.initialize.apply(this, arguments);
    },

    // Render
    render: function() {
      this.$el.append( browserViewsPopupTemplate(this.viewsView) );
      this.resizeToContents();
    },

    // Do not trigger events on this hide
    hide: function(options) {
      Popup.prototype.hide.call(this, _.extend({trigger:false}, options));
    }

  });

  return BrowserViewsPopup;

});