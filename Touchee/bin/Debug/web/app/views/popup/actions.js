define([
  'jquery',
  'underscore',
  'Backbone',
  'views/popup/base',
  'text!views/popup/actions.html'
], function($, _, Backbone, BasePopupView, actionsPopupTemplate) {
  actionsPopupTemplate = _.template(actionsPopupTemplate);
  
  
  var ActionsPopupView = BasePopupView.extend({

    // Backbone view options
    className: 'action',

    // Popup options
    removeOnHide: true,

    // Events
    events: {
      'tap button': 'tappedButton'
    },

    // Constructor
    initialize: function(options) {
      this.header = options.header;
      this.buttons = options.buttons;
      BasePopupView.prototype.initialize.apply(this, arguments);
    },

    //
    render: function() {
      this.$el.append(
        actionsPopupTemplate(this)
      );
    },

    //
    tappedButton: function(ev) {
      var i     = $(ev.target).prevAll('button').length,
          func  = this.buttons[i].action;
      action.call(this, ev);
    }

  });

  return ActionsPopupView;

});