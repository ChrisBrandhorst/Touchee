define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/split'
], function($, _, Backbone, SplitView) {
  
  
  var CommonSplitView = SplitView.extend({


    // The view used for the list
    listView:     null,
    // The header used for the details. Can be a view or a template
    detailHeader: null,
    // The view used for the details
    detailView:   null,


    // Constructor
    initialize: function(options) {
      SplitView.prototype.initialize.apply(this, arguments);
      this.setLeft( new this.listView(options), {render:false} );
    },


    // 
    navigate: function(params, fragment, module) {

      // Check if an item is selected
      if (_.isUndefined(params.view)) return;

      // Build the detail view
      var detailView = module.buildView( this.model.contents.container, params, fragment, this.detailView );
      if (!detailView) return;
      detailView.model.fetch();

      // Set in the right panel
      this.setRight(detailView);
    }
    
    
  });
  
  
  return CommonSplitView;
  
});