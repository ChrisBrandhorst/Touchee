define([
  'models/item'
], function(Item) {

  var DummyItem = Item.extend({
    get: function() {
      return "&nbsp;";
    }
  });

  return DummyItem;

});