define([
  'jquery',
  'underscore',
  'Backbone',
  'Touchee',
  'views/contents/base'
], function($, _, Backbone, Touchee, BaseView) {
  
  var SplitView = BaseView.extend({
    
    
    
    // Backbone View options
    // ---------------------
    tagName:      'section',
    className:    'split_view',
    
    
    
    
    // SplitView options
    // -----------------
    
    // Type of content
    contentType:    '',
    
    
    
    // Constructor
    initialize: function(options) {
      if (this.left)  this.setLeft(this.left, {render:false});
      if (this.right) this.setRight(this.right, {render:false});
    },
    
    
    // Render the view by placing the left and right panels
    render: function() {
      this.$el.addClass(this.contentType);
      if (this.left)  this.left.render();
      if (this.right) this.right.render();
    },
    
    
    // 
    setLeft: function(view, options) {
      if (this.left) this.left.$el.remove();
      view.$el.prependTo(this.$el);
      if (!options || options.render !== false) view.render();
      this.left = view;
    },
    
    
    // 
    setRight: function(view, options) {
      if (this.right) this.right.$el.remove();
      view.$el.appendTo(this.$el);
      if (!options || options.render !== false) view.render();
      this.right = view;
    },


    // 
    navigate: function(params, fragment, module) {
      // Check if any subview is selected
      if (!params[params.view]) return;

      // Build the subview
      var subView = module.buildView( this.model.contents.container, params, fragment, this.subView );
      subView.model.fetch();

      // Set in the right panel
      this.setRight(subView);
    }
    
    
  });
  
  return SplitView;
  
});