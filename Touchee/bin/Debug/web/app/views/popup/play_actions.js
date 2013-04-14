define([
  'jquery',
  'underscore',
  'Backbone',
  'views/popup/actions'
], function($, _, Backbone, ActionsPopupView) {
  
  var PlayActionsPopupView = ActionsPopupView.extend({},{

    // Shows the default play actions popup relative to the given element
    show: function(item, el, header) {
      new ActionsPopupView({
        item:     item,
        header:   header,
        buttons:  [
          {
            text:   'Play Next',
            action: function(ev){ Touchee.Queue.unshift(item); }
          },
          {
            text:   'Add to Up Next',
            action: function(ev){ Touchee.Queue.push(item); }
          }
        ]
      }).showRelativeTo(el);
    }

  });

  return PlayActionsPopupView;

});