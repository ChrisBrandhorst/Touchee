define([
  'jquery',
  'underscore',
  'Backbone',
  'views/popup/base',
  'text!views/browser/_views_popup.html'
], function($, _, Backbone, Popup, browserViewsPopupTemplate) {
  browserViewsPopupTemplate = _.template(browserViewsPopupTemplate);

  var BrowserViewsPopup = Popup.extend({


    id: 'views_popup',


    // Remove the popup from the DOM when it is hidden
    removeOnHide: true,


    // Events
    events: {
      'tap a':    'hide'
    },


    // Constructor
    initialize: function(options) {
      this.viewsView = options.viewsView;
      Popup.prototype.initialize.apply(this, arguments);
    },


    // Render
    render: function() {
      var $list     = $(browserViewsPopupTemplate(this.viewsView)),
          $existing = this.$el.children('nav');
      this.$el.append($list);
      $existing.remove();
      $list.touchscrollselect();
      this.resizeToContents();
    },


    // Do not trigger events on this hide
    hide: function(options) {
      Popup.prototype.hide.call(this, _.extend({trigger:false}, options));
    }


  });

  return BrowserViewsPopup;

});