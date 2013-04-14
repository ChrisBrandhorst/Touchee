define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/base'
], function($, _, Backbone, BaseView) {
  
  var SplitView = BaseView.extend({
    
    
    
    // Backbone View options
    // ---------------------
    tagName:      'section',
    
    
    
    
    // SplitView options
    // -----------------
    
    
    // Render the view by placing the left and right panels
    render: function() {
      this.$el.addClass('split_view');
      if (this.left)  this.left.render();
      if (this.right) this.right.render();
    },
    
    
    // 
    setLeft: function(view, options) {
      if (this.left) this.left.remove();
      view.$el.prependTo(this.$el);
      if (!options || options.render !== false) view.render();
      this.left = view;
    },
    
    
    // 
    setRight: function(view, options) {
      if (this.right) this.right.remove();
      view.$el.appendTo(this.$el);
      if (!options || options.render !== false) view.render();
      this.right = view;
    }
    
    
  });
  
  return SplitView;
  
});