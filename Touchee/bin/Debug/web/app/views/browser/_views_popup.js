define([
  'jquery',
  'underscore',
  'Backbone',
  'views/popup/base',
  'text!views/browser/_views_popup.html'
], function($, _, Backbone, PopupView, browserViewsPopupTemplate) {
  browserViewsPopupTemplate = _.template(browserViewsPopupTemplate);

  var BrowserViewsPopup = PopupView.extend({


    id: 'views_popup',


    // Remove the popup from the DOM when it is hidden
    removeOnHide: true,


    // Events
    events: {
      'release a': 'hide'
    },


    // Constructor
    initialize: function(options) {
      this.viewsView = options.viewsView;
      PopupView.prototype.initialize.apply(this, arguments);
    },


    // Render
    render: function() {
      var $list     = $(browserViewsPopupTemplate(this.viewsView)),
          $existing = this.$el.children('nav');
      this.$el.append($list);
      $existing.remove();
      $list.touchscrollselect();
      PopupView.prototype.render.apply(this, arguments);
    },


    // Do not trigger events on this hide
    hide: function(options) {
      PopupView.prototype.hide.call(this, _.extend({trigger:false}, options));
    }


  });

  return BrowserViewsPopup;

});