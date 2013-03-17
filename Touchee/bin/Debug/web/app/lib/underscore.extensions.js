require(['underscore'], function(_){
  _.mixin({

    asCssString: function(style) {
      return style = _.map(_.keys(style), function(k) {
        return k + ":" + style[k];
      }).join(';');
    }


  });
});