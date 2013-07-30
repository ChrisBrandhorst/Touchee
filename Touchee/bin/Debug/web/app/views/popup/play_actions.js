define([
  'jquery',
  'underscore',
  'Backbone',
  'views/popup/actions'
], function($, _, Backbone, ActionsPopupView) {

  var playNextButton = {
        text:   'Play Next',
        action: function(ev){ Touchee.Queue.prioritize(item); }
      },
      addToUpNextButton = {
        text:   'Add to Up Next',
        action: function(ev){ Touchee.Queue.push(item); }
      };

  var PlayActionsPopupView = ActionsPopupView.extend({},{

    // Shows the default play actions popup relative to the given element
    show: function(item, el, header) {
      var buttons = [playNextButton];
      if (Touchee.Queue.length) buttons.push(addToUpNextButton);

      new ActionsPopupView({
        item:     item,
        header:   header,
        buttons:  buttons
      }).showRelativeTo(el);
    }

  });

  return PlayActionsPopupView;

});