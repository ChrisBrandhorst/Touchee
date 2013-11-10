define([
  'jquery',
  'underscore',
  'Backbone'
], function($, _, Backbone) {
  
  var SplitView = Backbone.View.extend({
    
    
    
    // Backbone View options
    // ---------------------
    tagName:      'section',
    
    
    
    
    // SplitView options
    // -----------------
    
    
    // Render the view by placing the left and right panels
    render: function() {
      this.$el.addClass('split_view');
      if (this.left)  this.renderLeft();
      if (this.right) this.renderRight();
    },
    
    
    // 
    setLeft: function(view, options) {
      if (this.left)
        this.left.remove();
      this.left = view;
      if (!options || options.render !== false)
        this.renderLeft();
    },


    //
    renderLeft: function() {
      if (!this.left) return;
      if (this.left.el.parentNode != this.el)
        this.left.$el.prependTo(this.$el);
      this.left.render();
    },
    
    
    // 
    setRight: function(view, options) {
      if (this.right)
        this.right.remove();
      this.right = view;
      if (!options || options.render !== false)
        this.renderRight();
    },
    
    
    //
    renderRight: function() {
      if (!this.right) return;
      if (this.right.el.parentNode != this.el)
        this.right.$el.appendTo(this.$el);
      this.right.render();
    }


  });
  
  return SplitView;
  
});