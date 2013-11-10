define([
  'models/item'
], function(Item) {

  var DummyItem = Item.extend({
    url: "/",
    get: function() {
      return "&nbsp;";
    }
  });

  return DummyItem;

});