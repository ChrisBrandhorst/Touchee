require(['underscore'], function(_){
  _.mixin({

    asCssString: function(style) {
      return style = _.map(_.keys(style), function(k) {
        return k + ":" + style[k];
      }).join(';');
    },

    // Haalt de waarde uit het object op welke wordt gerepresenteerd door de gegeven string.
    // De string kan hier een aaneenschakeling van keys zijn, gescheiden door '.', zodat door
    // de objectgestructuur wordt gezocht. Bijvoorbeeld: _.getRef(obj, 'config.database.name');
    getRef: function(obj, str) {
      return str.split(".").reduce(function(o, x) { return o && o[x]; }, obj);
    }

  });
});